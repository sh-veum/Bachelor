using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.MessageHandler;
using NetBackend.Tools;

namespace NetBackend.Services.MessageHandlers;

public class BoatLocationMessageHandler : IMessageHandler
{
    private readonly ILogger<WaterQualityMessageHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public BoatLocationMessageHandler(ILogger<WaterQualityMessageHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task HandleMessageAsync(string message, string topic, long offset)
    {
        try
        {
            _logger.LogInformation($"Handling message from topic {topic}: {message}");

            var timestampStr = ExtractionTools.ExtractValue(message, "TimeStamp:", ",");
            var latitudeStr = ExtractionTools.ExtractValue(message, "Latitude:", ",");
            var longitudeStr = ExtractionTools.ExtractValue(message, "Longitude:", "");

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

            // _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new BoatLocationLog
            {
                Offset = offset,
                TimeStamp = timeStampUtc,
                Latitude = latitude,
                Longitude = longitude
            };

            // Now use timestamp which is a DateTimeOffset
            var userId = ExtractionTools.ExtractUserIdFromTopic(topic, KafkaConstants.BoatLogTopic);
            // _logger.LogInformation($"SensorId: {userId}");

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var (user, error) = await userService.GetUserByIdAsync(userId);

                var dbContext = await dbContextService.GetUserDatabaseContext(user);

                // Check if the offset allows for storing the message
                var mostRecentLog = await dbContext.Set<BoatLocationLog>().OrderByDescending(log => log.Offset).FirstOrDefaultAsync();
                if (mostRecentLog == null || mostRecentLog.Offset < offset)
                {
                    dbContext.Set<BoatLocationLog>().Add(logEntry);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Stored boat location log with id: {logEntry.Id}");
                }
                else
                {
                    _logger.LogInformation($"Skipping storage for boat location log due to offset {offset} being less than the most recent log's offset {mostRecentLog.Offset}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }
    }
}