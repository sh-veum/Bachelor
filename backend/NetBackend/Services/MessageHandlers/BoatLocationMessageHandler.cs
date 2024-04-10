using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.MessageHandler;
using NetBackend.Tools;

namespace NetBackend.Services.MessageHandlers;

public class BoatLocationMessageHandler : IMessageHandler, IDisposable
{
    private readonly ILogger<WaterQualityMessageHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentBag<BoatLocationLog> _messageBuffer;
    private readonly Timer _flushTimer;
    private volatile string _topic;

    public BoatLocationMessageHandler(ILogger<WaterQualityMessageHandler> logger, IServiceScopeFactory scopeFactory)
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
            var latitudeStr = ExtractionTools.ExtractValue(message, "Latitude:", ",");
            var longitudeStr = ExtractionTools.ExtractValue(message, "Longitude:", "");

            if (!DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                _logger.LogError($"Failed to parse timestamp: {timestampStr}");
                return Task.CompletedTask;
            }

            if (!double.TryParse(latitudeStr, out var latitude))
            {
                _logger.LogError($"Failed to parse latitude value: '{latitudeStr}'");
                return Task.CompletedTask;
            }

            if (!double.TryParse(longitudeStr, out var longitude))
            {
                _logger.LogError($"Failed to parse longitude value: '{longitudeStr}'");
                return Task.CompletedTask;
            }

            DateTime timeStampUtc = timestamp.UtcDateTime;

            // _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new BoatLocationLog
            {
                Offset = offset,
                TimeStamp = timeStampUtc,
                Latitude = latitude,
                Longitude = longitude
            };

            _topic = topic;

            // Now use timestamp which is a DateTimeOffset
            // var userId = ExtractionTools.ExtractUserIdFromTopic(topic, KafkaConstants.BoatLogTopic);
            // // _logger.LogInformation($"SensorId: {userId}");

            // using var scope = _scopeFactory.CreateScope();
            // var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            // var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            // var (user, error) = await userService.GetUserByIdAsync(userId);

            // var dbContext = await dbContextService.GetUserDatabaseContext(user);

            // check for duplicate
            // var existingLog = await dbContext.Set<BoatLocationLog>().AnyAsync(log => log.TimeStamp == logEntry.TimeStamp);

            // if (!existingLog)
            // {
            //     // dbContext.Set<BoatLocationLog>().Add(logEntry);
            //     // await dbContext.SaveChangesAsync();
            //     _logger.LogInformation($"Stored BoatLocationLog log with id: {logEntry.Id}");
            // }
            // else
            // {
            //     _logger.LogInformation($"Skipping storing BoatLocationLog with offset {logEntry.Offset} due to it being a duplicate.");
            // }

            _logger.LogInformation($"Adding BoatLocationLog with offset {logEntry.Offset} to buffer.");
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
            List<BoatLocationLog> messagesToFlush;
            lock (_messageBuffer)
            {
                messagesToFlush = [.. _messageBuffer];
                _messageBuffer.Clear();
            }

            _logger.LogInformation($"Flushing {messagesToFlush.Count} messages to the database.");

            var userId = ExtractionTools.ExtractUserIdFromTopic(_topic, KafkaConstants.BoatLogTopic);

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
                    var existingLog = await dbContext.Set<BoatLocationLog>().AnyAsync(log => log.TimeStamp == logEntry.TimeStamp);

                    if (!existingLog)
                    {
                        dbContext.Set<BoatLocationLog>().Add(logEntry);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Stored BoatLocationLog with id: {logEntry.Id} and offset: {logEntry.Offset}");
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