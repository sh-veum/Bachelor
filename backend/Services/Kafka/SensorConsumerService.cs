using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Models.Enums;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services.Kafka;

public class SensorConsumerService : BackgroundService, ISensorConsumerService
{
    private readonly ILogger<SensorConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, SensorType> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;

    public SensorConsumerService(IConfiguration configuration, ILogger<SensorConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;
        _activeTopics = new ConcurrentDictionary<string, SensorType>();
        _configuration = configuration;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Stopping token: {stoppingToken}");
        _stoppingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _logger.LogInformation($"Stopping Cancel token: {_stoppingCancellationTokenSource.Token}");
        StartConsumeLoop(_stoppingCancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    private void StartConsumeLoop(CancellationToken stoppingToken)
    {
        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested && !_loopCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);

                        if (consumeResult != null && !consumeResult.IsPartitionEOF)
                        {
                            _logger.LogInformation($"Consume Loop Sending Topic: {consumeResult.Topic}, Message: {consumeResult.Message.Value}, Offset: {consumeResult.Offset}");
                            // Directly handle and send the message if not processing historical data
                            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, consumeResult.Offset);
                            HandleMessage(consumeResult.Message.Value, consumeResult.Topic, consumeResult.Offset);
                        }
                    }
                    catch (ConsumeException ex) when (ex.Error.Reason.Contains("Unknown topic or partition"))
                    {
                        _logger.LogWarning($"Topic not available yet, waiting before retrying. Error: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds before retrying
                        ResetLoopCancellationToken();
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Reconfiguring consumer subscriptions, restarting consume loop...");
                        ResetLoopCancellationToken();
                        continue; // Continue in the loop after resetting the token.
                    }
                    else
                    {
                        _logger.LogInformation("Stopping due to application shutdown.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error consuming Kafka message: {ex.Message}");
                }
            }

            _logger.LogInformation("Closing main consumer...");
            _consumer.Close();
        }, stoppingToken);
    }

    public void SubscribeToTopic(string newTopic, SensorType sensorType, bool sendHistoricalData = false)
    {
        _logger.LogInformation($"Subscribing to topic: {newTopic}, sendHistoricalData: {sendHistoricalData}");

        _consumer.Unsubscribe();

        var isNewSubscription = _activeTopics.TryAdd(newTopic, sensorType);

        if (isNewSubscription)
        {
            _logger.LogInformation($"activeTopics: {_activeTopics.Keys}");
            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
        }

        if (sendHistoricalData)
        {
            Task.Run(() => ConsumeHistoricalData(newTopic));
        }

        if (isNewSubscription)
        {
            InterruptAndRestartConsumeLoop();
        }
    }

    // TODO: Send to a separate WebSocket endpoint for historical data
    private async Task ConsumeHistoricalData(string topic)
    {
        var partitionBuffers = new Dictionary<int, SortedList<long, ConsumeResult<Ignore, string>>>();
        var adminConfig = new AdminClientConfig { BootstrapServers = _configuration["Kafka:BootstrapServers"] };
        using (var adminClient = new AdminClientBuilder(adminConfig).Build())
        {
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(10));
            foreach (var partition in metadata.Topics[0].Partitions)
            {
                partitionBuffers[partition.PartitionId] = [];
                _consumer.Assign(new TopicPartitionOffset(topic, partition.PartitionId, Offset.Beginning));
            }
        }

        bool shouldContinue = true;
        while (shouldContinue)
        {
            var consumeResult = _consumer.Consume(100); // Adjust the timeout as needed
            if (consumeResult == null)
            {
                shouldContinue = false;
                continue;
            }

            if (consumeResult.IsPartitionEOF)
            {
                // Process messages for this partition.
                await ProcessPartitionBuffer(partitionBuffers[consumeResult.Partition.Value], consumeResult.Partition.Value);
                partitionBuffers.Remove(consumeResult.Partition.Value);
                shouldContinue = partitionBuffers.Count > 0;
                continue;
            }

            if (consumeResult.Message != null)
            {
                // Add the message to the partition's buffer.
                partitionBuffers[consumeResult.Partition.Value].Add(consumeResult.Offset.Value, consumeResult);
            }
        }

        // Ensure all buffers are processed at the end.
        foreach (var kvp in partitionBuffers)
        {
            await ProcessPartitionBuffer(kvp.Value, kvp.Key);
        }

        _consumer.Unassign();
        _consumer.Subscribe(_activeTopics.Keys);
    }

    private async Task ProcessPartitionBuffer(SortedList<long, ConsumeResult<Ignore, string>> buffer, int partitionId)
    {
        foreach (var kvp in buffer)
        {
            var consumeResult = kvp.Value;

            var currentOffset = consumeResult.Offset.Value;
            _logger.LogInformation($"Processing message for Topic: {consumeResult.Topic}, Partition: {partitionId}, Offset: {currentOffset}");

            // await HandleMessage(consumeResult.Message.Value, consumeResult.Topic);
            await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, currentOffset);
        }
        buffer.Clear();
    }

    private void HandleMessage(string message, string topic, long currentOffset)
    {
        Task.Run(async () =>
       {
           try
           {
               if (!_activeTopics.TryGetValue(topic, out var sensorType))
               {
                   _logger.LogWarning($"No sensor type found for topic {topic}. Unable to process message.");
                   return;
               }

               switch (sensorType)
               {
                   case SensorType.waterQuality:
                       await HandleWaterQualityMessage(message, topic, currentOffset);
                       break;
                   case SensorType.boat:
                       await HandleBoatMessage(message, topic, currentOffset);
                       break;
                   default:
                       _logger.LogWarning($"Unhandled sensor type: {sensorType} for topic {topic}.");
                       break;
               }
           }
           catch (Exception ex)
           {
               _logger.LogError($"Failed to store message in database. Error: {ex.Message}");
           }
       });
    }

    private void InterruptAndRestartConsumeLoop()
    {
        _loopCancellationTokenSource.Cancel();
        ResetLoopCancellationToken();
        if (_stoppingCancellationTokenSource != null)
        {
            StartConsumeLoop(_stoppingCancellationTokenSource.Token);
        }
        else
        {
            _logger.LogWarning("Stopping token source is null, cannot restart consume loop.");
        }
    }

    private void ResetLoopCancellationToken()
    {
        _loopCancellationTokenSource.Dispose();
        _loopCancellationTokenSource = new CancellationTokenSource();
    }

    private async Task HandleWaterQualityMessage(string message, string topic, long messageOffset)
    {
        try
        {
            _logger.LogInformation($"Handling message from topic {topic}: {message}");

            var timestampStr = ExtractValue(message, "TimeStamp:", ",");
            var phStr = ExtractValue(message, "pH:", ",");
            var turbidityStr = ExtractValue(message, "Turbidity:", "NTU");
            var temperatureStr = ExtractValue(message, "Temperature:", "C");

            // _logger.LogInformation($"Extracted values BEFORE tryParse: Timestamp: {timestampStr}, pH: {phStr}, Turbidity: {turbidityStr}, Temperature: {temperatureStr}");

            if (!DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                _logger.LogError($"Failed to parse timestamp: {timestampStr}");
                return;
            }

            if (!double.TryParse(phStr, out var ph))
            {
                _logger.LogError($"Failed to parse pH value: '{phStr}'");
                return;
            }

            if (!double.TryParse(turbidityStr, out var turbidity))
            {
                _logger.LogError($"Failed to parse turbidity value: '{turbidityStr}'");
                return;
            }

            if (!double.TryParse(temperatureStr, out var temperature))
            {
                _logger.LogError($"Failed to parse temperature value: '{temperatureStr}'");
                return;
            }

            // _logger.LogInformation($"Extracted values AFTER tryParse: Timestamp: {timestamp}, pH: {ph}, Turbidity: {turbidity}, Temperature: {temperature}");

            DateTime timeStampUtc = timestamp.UtcDateTime;

            _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new WaterQualityLog
            {
                Offset = messageOffset,
                TimeStamp = timeStampUtc,
                Ph = ph,
                Turbidity = turbidity,
                Temperature = temperature
            };

            // Now use timestamp which is a DateTimeOffset
            var userId = ExtractUserIdFromTopic(topic, KafkaConstants.WaterQualityLogTopic);
            _logger.LogInformation($"SensorId: {userId}");

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var (user, error) = await userService.GetUserByIdAsync(userId);

                var dbContext = await dbContextService.GetUserDatabaseContext(user);

                // Check if the offset allows for storing the message
                var mostRecentLog = await dbContext.Set<WaterQualityLog>().OrderByDescending(log => log.Offset).FirstOrDefaultAsync();

                if (mostRecentLog == null || mostRecentLog.Offset < messageOffset)
                {
                    dbContext.Set<WaterQualityLog>().Add(logEntry);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Stored water quality log with id: {logEntry.Id} and offset: {messageOffset}");
                }
                else
                {
                    _logger.LogInformation($"Skipping storage for water quality log due to offset {messageOffset} being less than the most recent log's offset {mostRecentLog.Offset}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }
    }

    private async Task HandleBoatMessage(string message, string topic, long messageOffset)
    {
        try
        {
            _logger.LogInformation($"Handling message from topic {topic}: {message}");

            var timestampStr = ExtractValue(message, "TimeStamp:", ",");
            var latitudeStr = ExtractValue(message, "Latitude:", ",");
            var longitudeStr = ExtractValue(message, "Longitude:", "");

            if (!DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                _logger.LogError($"Failed to parse timestamp: {timestampStr}");
                return;
            }

            if (!double.TryParse(latitudeStr, out var latitude))
            {
                _logger.LogError($"Failed to parse latitude value: '{latitudeStr}'");
                return;
            }

            if (!double.TryParse(longitudeStr, out var longitude))
            {
                _logger.LogError($"Failed to parse longitude value: '{longitudeStr}'");
                return;
            }

            DateTime timeStampUtc = timestamp.UtcDateTime;

            _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new BoatLocationLog
            {
                Offset = messageOffset,
                TimeStamp = timeStampUtc,
                Latitude = latitude,
                Longitude = longitude
            };

            // Now use timestamp which is a DateTimeOffset
            var userId = ExtractUserIdFromTopic(topic, KafkaConstants.BoatLogTopic);
            _logger.LogInformation($"SensorId: {userId}");

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var (user, error) = await userService.GetUserByIdAsync(userId);

                var dbContext = await dbContextService.GetUserDatabaseContext(user);

                // Check if the offset allows for storing the message
                var mostRecentLog = await dbContext.Set<BoatLocationLog>().OrderByDescending(log => log.Offset).FirstOrDefaultAsync();
                if (mostRecentLog == null || mostRecentLog.Offset < messageOffset)
                {
                    dbContext.Set<BoatLocationLog>().Add(logEntry);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Stored boat location log with id: {logEntry.Id}");
                }
                else
                {
                    _logger.LogInformation($"Skipping storage for boat location log due to offset {messageOffset} being less than the most recent log's offset {mostRecentLog.Offset}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }
    }

    private static string ExtractUserIdFromTopic(string topic, string topicPrefix)
    {
        string prefix = $"{topicPrefix}-";
        if (topic.StartsWith(prefix))
        {
            return topic[prefix.Length..];
        }

        throw new ArgumentException($"Topic '{topic}' does not start with the expected prefix '{prefix}'.", nameof(topic));
    }

    private async Task SendMessageToWebSocket(string topic, string message, long offset)
    {
        var webSocketMessage = new
        {
            topic,
            message,
            offset
        };
        var serializedMessage = JsonSerializer.Serialize(webSocketMessage);
        await _webSocketManager.SendMessageAsync(serializedMessage);
    }

    private static string ExtractValue(string message, string label, string endDelimiter)
    {
        int startIndex = message.IndexOf(label) + label.Length;
        if (startIndex < label.Length) return string.Empty; // Label not found

        int endIndex = endDelimiter != "" ? message.IndexOf(endDelimiter, startIndex) : -1;
        if (endIndex == -1) endIndex = message.Length;

        string value = message.Substring(startIndex, endIndex - startIndex).Trim();

        // Special handling for timestamp to ensure full ISO8601 format is preserved
        if (label.StartsWith("TimeStamp"))
        {
            return value;
        }

        // Adjusting logic to safely handle numeric values including negatives
        return new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray()).Trim();
    }

}