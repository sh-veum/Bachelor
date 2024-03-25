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
    private volatile bool _isTemporaryConsumerActive = false;
    private ConcurrentQueue<(string Topic, string Message, long Offset)> _realTimeMessageQueue = new();
    private ConcurrentDictionary<TopicPartition, long> _latestProcessedOffsets = new();



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
                    // Second try for when the topic is not available yet
                    try
                    {
                        var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);

                        if (consumeResult != null && !consumeResult.IsPartitionEOF)
                        {
                            var currentOffset = consumeResult.TopicPartitionOffset.Offset.Value;
                            var topicPartition = consumeResult.TopicPartition;
                            var shouldProcessMessage = !_latestProcessedOffsets.TryGetValue(topicPartition, out var latestProcessedOffset) || currentOffset > latestProcessedOffset;

                            if (shouldProcessMessage && !_isTemporaryConsumerActive)
                            {
                                if (_isTemporaryConsumerActive)
                                {
                                    // Enqueue the message along with its offset
                                    _logger.LogInformation($"Enqueuing message for topic {consumeResult.Topic} with message {consumeResult.Message.Value} and offset {currentOffset}");
                                    _realTimeMessageQueue.Enqueue((consumeResult.Topic, consumeResult.Message.Value, currentOffset));
                                }
                                else
                                {
                                    _logger.LogInformation($"Consume Loop Sending Topic: {consumeResult.Topic}, Message: {consumeResult.Message.Value}, Offset: {currentOffset}");
                                    // Directly handle and send the message if not processing historical data
                                    await HandleMessage(consumeResult.Message.Value, consumeResult.Topic);
                                    await SendMessageToWebSocket(consumeResult.Topic, consumeResult.Message.Value, currentOffset);
                                }
                            }
                        }

                        // After historical data has been sent and _isTemporaryConsumerActive is false, process messages from the queue.
                        while (!_isTemporaryConsumerActive && _realTimeMessageQueue.TryDequeue(out var queuedMessage))
                        {
                            _logger.LogInformation($"Dequeuing message for topic {queuedMessage.Topic} with message {queuedMessage.Message} and offset {queuedMessage.Offset}");
                            await HandleMessage(queuedMessage.Message, queuedMessage.Topic); // Handle the dequeued message.
                            await SendMessageToWebSocket(queuedMessage.Topic, queuedMessage.Message, queuedMessage.Offset); // Send the message to WebSocket.
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
                        // await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds before retrying
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

        if (sendHistoricalData)
        {
            _logger.LogInformation($"Sending historical data for topic: {newTopic}");

            // if (sensorType == SensorType.waterQuality)
            // {
            //     Task.Run(() => SendHistoricalDataToClient(newTopic, KafkaConstants.WaterQualityLogTopic));
            // }
            // else if (sensorType == SensorType.boat)
            // {
            //     Task.Run(() => SendHistoricalDataToClient(newTopic, KafkaConstants.BoatLogTopic));
            // }
            // else
            // {
            //     _logger.LogWarning($"Unhandled sensor type: {sensorType} for topic {newTopic}.");
            // }

            Task.Run(() => SendHistoricalDataToClientUsingNewConsumer(newTopic));
        }

        var isNewSubscription = _activeTopics.TryAdd(newTopic, sensorType);

        if (isNewSubscription)
        {
            _logger.LogInformation($"activeTopics: {_activeTopics.Keys}");

            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
            InterruptAndRestartConsumeLoop();
        }
    }

    private async Task HandleMessage(string message, string topic)
    {
        if (!_activeTopics.TryGetValue(topic, out var sensorType))
        {
            _logger.LogWarning($"No sensor type found for topic {topic}. Unable to process message.");
            return;
        }

        switch (sensorType)
        {
            case SensorType.waterQuality:
                await HandleWaterQualityMessage(message, topic);
                break;
            case SensorType.boat:
                await HandleBoatMessage(message, topic);
                break;
            default:
                _logger.LogWarning($"Unhandled sensor type: {sensorType} for topic {topic}.");
                break;
        }
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

    private async Task HandleWaterQualityMessage(string message, string topic)
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
                dbContext.Set<WaterQualityLog>().Add(logEntry);
                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation($"Stored water quality log with id: {logEntry.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }
    }

    private async Task HandleBoatMessage(string message, string topic)
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
                dbContext.Set<BoatLocationLog>().Add(logEntry);
                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation($"Stored boat location log with id: {logEntry.Id}");
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

    private async Task SendHistoricalDataFromDBToClient(string topic, string topicPrefix, long offset)
    {
        try
        {
            if (topicPrefix == KafkaConstants.WaterQualityLogTopic)
            {
                var historicalData = await FetchHistoricalDataForTopic<WaterQualityLog>(topic, topicPrefix);
                _logger.LogInformation($"Sending {historicalData.Count()} water quality logs to frontend.");
                foreach (var log in historicalData)
                {
                    string message = $"TimeStamp: {log.TimeStamp:o}, pH: {log.Ph}, Turbidity: {log.Turbidity} NTU, Temperature: {log.Temperature}C";
                    await SendMessageToWebSocket(topic, message, offset);
                }
            }
            else if (topicPrefix == KafkaConstants.BoatLogTopic)
            {
                var historicalData = await FetchHistoricalDataForTopic<BoatLocationLog>(topic, topicPrefix);
                _logger.LogInformation($"Sending {historicalData.Count()} boat location logs to frontend.");
                foreach (var log in historicalData)
                {
                    string message = $"TimeStamp: {log.TimeStamp:o}, Latitude: {log.Latitude}, Longitude: {log.Longitude}";
                    await SendMessageToWebSocket(topic, message, offset);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send historical data for topic {topic}. Error: {ex.Message}");
        }
    }

    private async Task SendHistoricalDataToClientUsingNewConsumer(string topic)
    {
        _logger.LogInformation($"Making new consumer for topic: {topic}");
        _isTemporaryConsumerActive = true;

        var tempConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = Guid.NewGuid().ToString(), // Unique GroupId for each temporary consumer
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };

        using (var temporaryConsumer = new ConsumerBuilder<Ignore, string>(tempConsumerConfig).Build())
        {
            temporaryConsumer.Subscribe(topic);

            try
            {
                var partitionOffsets = new Dictionary<TopicPartition, long>();

                _logger.LogInformation($"Trying to consume historical data for topic: {topic}");
                while (true)
                {
                    var consumeResult = temporaryConsumer.Consume(10000); // Adjust the timeout as needed
                    if (consumeResult == null || consumeResult.IsPartitionEOF)
                    {
                        if (consumeResult != null)
                        {
                            partitionOffsets[consumeResult.TopicPartition] = consumeResult.Offset.Value;
                        }
                        break;
                    }

                    if (consumeResult.Message?.Value != null)
                    {
                        _logger.LogInformation($"Temp Topic: {topic}, Temp Message: {consumeResult.Message.Value}, Temp Offset: {consumeResult.TopicPartitionOffset.Offset.Value}");
                        await SendMessageToWebSocket(topic, consumeResult.Message.Value, consumeResult.TopicPartitionOffset.Offset.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Consume result message or message value is null.");
                    }
                }

                // After consuming historical data, update the latest processed offsets for each partition
                foreach (var po in partitionOffsets)
                {
                    _latestProcessedOffsets.AddOrUpdate(po.Key, po.Value, (key, oldValue) => Math.Max(oldValue, po.Value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while sending historical data for topic {topic}. Error: {ex.Message}");
            }
        }

        _isTemporaryConsumerActive = false;
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

    private async Task<IEnumerable<T>> FetchHistoricalDataForTopic<T>(string topic, string topicPrefix) where T : class
    {
        var userId = ExtractUserIdFromTopic(topic, topicPrefix);

        List<T> historicalData = [];

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var (user, error) = await userService.GetUserByIdAsync(userId);

            DbContext dbContext = await dbContextService.GetUserDatabaseContext(user);

            if (topicPrefix == KafkaConstants.WaterQualityLogTopic)
            {
                historicalData = await dbContext.Set<T>().ToListAsync();
            }
            else if (topicPrefix == KafkaConstants.BoatLogTopic)
            {
                historicalData = await dbContext.Set<T>().ToListAsync();
            }
        }

        return historicalData;
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