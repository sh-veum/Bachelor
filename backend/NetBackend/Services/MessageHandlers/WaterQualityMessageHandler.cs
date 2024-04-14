using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.MessageHandler;
using NetBackend.Tools;

namespace NetBackend.Services.MessageHandlers;

public class WaterQualityMessageHandler : IMessageHandler, IDisposable
{
    private readonly ILogger<WaterQualityMessageHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentBag<WaterQualityLog> _messageBuffer;
    private readonly Timer _flushTimer;
    private volatile string _topic;

    public WaterQualityMessageHandler(ILogger<WaterQualityMessageHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _messageBuffer = [];
        _topic = "";

        _flushTimer = new Timer(async _ => await FlushMessagesAsync(),
                                                 null,
                                                 TimeSpan.Zero,
                                                 TimeSpan.FromSeconds(10));
    }

    public Task HandleMessage(string message, string topic, long offset)
    {
        try
        {
            _logger.LogInformation($"Handling message from topic {topic}: {message}");

            _topic = topic;

            var timestampStr = ExtractionTools.ExtractValue(message, "TimeStamp:", ",");
            var phStr = ExtractionTools.ExtractValue(message, "pH:", ",");
            var turbidityStr = ExtractionTools.ExtractValue(message, "Turbidity:", "NTU");
            var temperatureStr = ExtractionTools.ExtractValue(message, "Temperature:", "C");

            if (!DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                _logger.LogError($"Failed to parse timestamp: {timestampStr}");
                return Task.CompletedTask;
            }

            if (!double.TryParse(phStr, out var ph))
            {
                _logger.LogError($"Failed to parse pH value: '{phStr}'");
                return Task.CompletedTask;
            }

            if (!double.TryParse(turbidityStr, out var turbidity))
            {
                _logger.LogError($"Failed to parse turbidity value: '{turbidityStr}'");
                return Task.CompletedTask;
            }

            if (!double.TryParse(temperatureStr, out var temperature))
            {
                _logger.LogError($"Failed to parse temperature value: '{temperatureStr}'");
                return Task.CompletedTask;
            }

            // _logger.LogInformation($"Extracted values AFTER tryParse: Timestamp: {timestamp}, pH: {ph}, Turbidity: {turbidity}, Temperature: {temperature}");

            DateTime timeStampUtc = timestamp.UtcDateTime;

            // _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new WaterQualityLog
            {
                Offset = offset,
                TimeStamp = timeStampUtc,
                Ph = ph,
                Turbidity = turbidity,
                Temperature = temperature
            };

            // // Now use timestamp which is a DateTimeOffset
            // var userId = ExtractionTools.ExtractUserIdFromTopic(topic, KafkaConstants.WaterQualityLogTopic);
            // // _logger.LogInformation($"SensorId: {userId}");

            // using var scope = _scopeFactory.CreateScope();
            // var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            // var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            // var (user, error) = await userService.GetUserByIdAsync(userId);

            // var dbContext = await dbContextService.GetUserDatabaseContext(user);

            // // check for duplicate
            // var existingLog = await dbContext.Set<WaterQualityLog>().AnyAsync(log => log.TimeStamp == logEntry.TimeStamp);

            // if (!existingLog)
            // {
            //     dbContext.Set<WaterQualityLog>().Add(logEntry);
            //     await dbContext.SaveChangesAsync();
            //     _logger.LogInformation($"Stored WaterQualityLog with id: {logEntry.Id} and offset: {offset}");
            // }
            // else
            // {
            //     _logger.LogInformation($"Skipping storing WaterQualityLog with offset {logEntry.Offset} due to it being a duplicate.");
            // }

            _logger.LogInformation($"Adding WaterQualityLog with offset {logEntry.Offset} to buffer.");
            _messageBuffer.Add(logEntry);
            _logger.LogInformation($"_messageBuffer count {_messageBuffer.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private async Task FlushMessagesAsync()
    {
        if (!_messageBuffer.IsEmpty)
        {
            List<WaterQualityLog> messagesToFlush;
            lock (_messageBuffer)
            {
                messagesToFlush = [.. _messageBuffer];
                _messageBuffer.Clear();
            }

            _logger.LogInformation($"Flushing {messagesToFlush.Count} messages to the database.");

            var userId = ExtractionTools.ExtractUserIdFromTopic(_topic, KafkaConstants.WaterQualityLogTopic);

            using var scope = _scopeFactory.CreateScope();
            var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var (user, error) = await userService.GetUserByIdAsync(userId);

            var dbContext = await dbContextService.GetUserDatabaseContext(user);

            // check for duplicate

            foreach (var logEntry in messagesToFlush)
            {
                try
                {
                    var existingLog = await dbContext.Set<WaterQualityLog>().AnyAsync(log => log.TimeStamp == logEntry.TimeStamp);

                    if (!existingLog)
                    {
                        dbContext.Set<WaterQualityLog>().Add(logEntry);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Stored WaterQualityLog with id: {logEntry.Id} and offset: {logEntry.Offset}");
                    }
                    else
                    {
                        _logger.LogInformation($"Skipping storing WaterQualityLog with offset {logEntry.Offset} due to it being a duplicate.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to store message in database. Error: {ex.Message}");
                }
            }
        }
    }

    public void Dispose()
    {
        _flushTimer?.Change(Timeout.Infinite, 0);
        _flushTimer?.Dispose();
    }
}