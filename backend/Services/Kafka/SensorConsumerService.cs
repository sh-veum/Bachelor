using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly ConcurrentDictionary<string, SensorType> _activeTopics;
    private CancellationTokenSource _loopCancellationTokenSource = new();
    private CancellationTokenSource? _stoppingCancellationTokenSource;

    public SensorConsumerService(IConfiguration configuration, ILogger<SensorConsumerService> logger, IAppWebSocketManager webSocketManager, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _webSocketManager = webSocketManager;
        _scopeFactory = scopeFactory;
        _activeTopics = new ConcurrentDictionary<string, SensorType>();

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
                    var consumeResult = _consumer.Consume(_loopCancellationTokenSource.Token);
                    _logger.LogInformation($"Consume value: {consumeResult.Message.Value}");

                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        if (consumeResult.Message.Value != null)
                        {
                            await HandleMessage(consumeResult.Message.Value, consumeResult.Topic);
                        }
                        else
                        {
                            _logger.LogWarning("Consume result or message value is null.");
                        }

                        _logger.LogInformation($"Consumed message from topic {consumeResult.Topic}: Value: {consumeResult.Message.Value}");

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
                        continue; // Continue in the loop after resetting the token
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

    public void SubscribeToTopic(string newTopic, SensorType sensorType, bool sendHistoricalData = false)
    {
        _logger.LogInformation($"Subscribing to topic: {newTopic}, sendHistoricalData: {sendHistoricalData}");

        var isNewSubscription = _activeTopics.TryAdd(newTopic, sensorType);

        if (isNewSubscription)
        {
            _consumer.Subscribe(_activeTopics.Keys);
            _logger.LogInformation($"Subscribed to new topic: {newTopic}");
        }

        if (sendHistoricalData)
        {
            _logger.LogInformation($"Sending historical data for topic: {newTopic}");

            if (sensorType == SensorType.waterQuality)
            {
                Task.Run(() => SendHistoricalDataToClient(newTopic, KafkaConstants.WaterQualityLogTopic));
            }
            else if (sensorType == SensorType.boat)
            {
                Task.Run(() => SendHistoricalDataToClient(newTopic, KafkaConstants.BoatLogTopic));
            }
            else
            {
                _logger.LogWarning($"Unhandled sensor type: {sensorType} for topic {newTopic}.");
            }
        }

        if (isNewSubscription)
        {
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
    private async Task SendHistoricalDataToClient(string topic, string topicPrefix)
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
                    await SendMessageAsync(topic, message);
                }
            }
            else if (topicPrefix == KafkaConstants.BoatLogTopic)
            {
                var historicalData = await FetchHistoricalDataForTopic<BoatLocationLog>(topic, topicPrefix);
                _logger.LogInformation($"Sending {historicalData.Count()} boat location logs to frontend.");
                foreach (var log in historicalData)
                {
                    string message = $"TimeStamp: {log.TimeStamp:o}, Latitude: {log.Latitude}, Longitude: {log.Longitude}";
                    await SendMessageAsync(topic, message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send historical data for topic {topic}. Error: {ex.Message}");
        }
    }

    private async Task SendMessageAsync(string topic, string message)
    {
        var webSocketMessage = new
        {
            topic,
            message
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