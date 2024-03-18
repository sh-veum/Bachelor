using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services.Kafka;

public class WaterQualityConsumerService : BackgroundService, IWaterQualityConsumerService
{
    private readonly ILogger<WaterQualityConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IAppWebSocketManager _webSocketManager;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, bool> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;

    public WaterQualityConsumerService(IConfiguration configuration, ILogger<WaterQualityConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;
        _activeTopics = new ConcurrentDictionary<string, bool>();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Water Stopping token: {stoppingToken}");
        _stoppingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _logger.LogInformation($"Water Stopping Cancel token: {_stoppingCancellationTokenSource.Token}");
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
                    var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);
                    _logger.LogInformation($"Water Consume value: {consumeResult.Message.Value}");

                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        if (consumeResult.Message.Value != null)
                        {
                            await HandleWaterQualityMessage(consumeResult.Message.Value, consumeResult.Topic);
                        }
                        else
                        {
                            _logger.LogWarning("Consume result or message value is null.");
                        }

                        _logger.LogInformation($"Consumed message from topic {consumeResult.Topic}: Key: {consumeResult.Message.Key}, Value: {consumeResult.Message.Value}");

                        var webSocketMessage = new
                        {
                            topic = consumeResult.Topic,
                            message = consumeResult.Message.Value
                        };
                        var serializedMessage = JsonSerializer.Serialize(webSocketMessage);
                        await _webSocketManager.SendMessageAsync(serializedMessage);
                        _logger.LogInformation($"WebSocket message sent: {serializedMessage}");
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Reconfiguring consumer subscriptions, restarting consume loop...");
                        ResetLoopCancellationToken();
                        continue; // Important: Continue in the loop after resetting the token
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
            _consumer.Close();
        }, stoppingToken);
    }

    // public void SubscribeToTopic(string newTopic)
    // {
    //     if (_activeTopics.TryAdd(newTopic, true))
    //     {
    //         _consumer.Subscribe(_activeTopics.Keys);
    //         _logger.LogInformation($"Subscribed to new topic: {newTopic}");
    //         InterruptAndRestartConsumeLoop();
    //     }
    // }

    public void SubscribeToTopic(string newTopic, bool sendHistoricalData = false)
    {
        _logger.LogInformation($"Subscribing to topic: {newTopic}, sendHistoricalData: {sendHistoricalData}");

        // Always attempt to add the topic to the dictionary of active topics
        var isNewSubscription = _activeTopics.TryAdd(newTopic, true);

        // Subscribe to the topic if it's a new subscription
        if (isNewSubscription)
        {
            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
        }

        // Check if historical data needs to be sent regardless of whether the subscription is new
        if (sendHistoricalData)
        {
            _logger.LogInformation($"Sending historical data for topic: {newTopic}");
            // Asynchronously handle historical data to avoid blocking
            Task.Run(() => SendHistoricalDataToClient(newTopic));
        }

        // Only interrupt and restart the consume loop if this is a new subscription
        // This avoids unnecessary restarts if you're only fetching historical data for an existing topic
        if (isNewSubscription)
        {
            InterruptAndRestartConsumeLoop();
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
            var userId = ExtractUserIdFromTopic(topic);
            _logger.LogInformation($"SensorId: {userId}");

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
                var mainDbContext = await dbContextService.GetDatabaseContextByName(DatabaseConstants.MainDbName);

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

    private static string ExtractUserIdFromTopic(string topic)
    {
        // Define the prefix to remove from the topic to extract the sensor ID
        string prefix = $"{KafkaConstants.WaterQualityLogTopic}-";
        if (topic.StartsWith(prefix))
        {
            return topic.Substring(prefix.Length);
        }

        throw new ArgumentException($"Topic '{topic}' does not start with the expected prefix '{prefix}'.", nameof(topic));
    }

    private string ExtractValue(string message, string label, string endDelimiter = ",")
    {
        int startIndex = message.IndexOf(label) + label.Length;
        if (startIndex < label.Length) return string.Empty; // Label not found

        int endIndex = message.IndexOf(endDelimiter, startIndex);
        if (endDelimiter != "," && endIndex == -1) endIndex = message.Length; // For last item or items not ending with comma

        string value = message.Substring(startIndex, endIndex - startIndex).Trim();

        // If extracting a timestamp, return the full value without truncating non-numeric characters
        if (label.StartsWith("TimeStamp"))
        {
            return value;
        }

        // For numeric values, continue removing any trailing non-numeric characters
        return new string(value.TakeWhile(c => char.IsDigit(c) || c == '.' || c == '-').ToArray()).Trim();
    }

    private async Task SendHistoricalDataToClient(string topic)
    {
        try
        {
            var historicalData = await FetchHistoricalDataForTopic(topic);
            _logger.LogInformation($"Amount of logs sent to frontend: {historicalData.Count()}");

            foreach (var data in historicalData)
            {
                string message = $"TimeStamp: {data.TimeStamp:o}, pH: {data.Ph}, Turbidity: {data.Turbidity} NTU, Temperature: {data.Temperature}C";

                var webSocketMessage = new
                {
                    topic = topic,
                    message = message
                };

                var serializedMessage = JsonSerializer.Serialize(webSocketMessage);
                await _webSocketManager.SendMessageAsync(serializedMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send historical data for topic {topic}. Error: {ex.Message}");
        }
    }
    private async Task<IEnumerable<WaterQualityLog>> FetchHistoricalDataForTopic(string topic)
    {
        var userId = ExtractUserIdFromTopic(topic);

        List<WaterQualityLog> historicalData = [];

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var (user, error) = await userService.GetUserByIdAsync(userId);

            DbContext dbContext = await dbContextService.GetUserDatabaseContext(user);

            historicalData = await dbContext.Set<WaterQualityLog>().ToListAsync();
        }

        return historicalData;
    }
}