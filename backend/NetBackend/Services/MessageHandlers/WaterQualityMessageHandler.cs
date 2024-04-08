using Microsoft.EntityFrameworkCore;
using NetBackend.Constants;
using NetBackend.Models;
using NetBackend.Services.Interfaces;
using NetBackend.Services.Interfaces.MessageHandler;
using NetBackend.Tools;

namespace NetBackend.Services.MessageHandlers;

public class WaterQualityMessageHandler : IMessageHandler
{
    private readonly ILogger<WaterQualityMessageHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public WaterQualityMessageHandler(ILogger<WaterQualityMessageHandler> logger, IServiceScopeFactory scopeFactory)
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
            var phStr = ExtractionTools.ExtractValue(message, "pH:", ",");
            var turbidityStr = ExtractionTools.ExtractValue(message, "Turbidity:", "NTU");
            var temperatureStr = ExtractionTools.ExtractValue(message, "Temperature:", "C");

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

            // _logger.LogInformation($"Parsed timestamp: {timeStampUtc}");

            var logEntry = new WaterQualityLog
            {
                Offset = offset,
                TimeStamp = timeStampUtc,
                Ph = ph,
                Turbidity = turbidity,
                Temperature = temperature
            };

            // Now use timestamp which is a DateTimeOffset
            var userId = ExtractionTools.ExtractUserIdFromTopic(topic, KafkaConstants.WaterQualityLogTopic);
            // _logger.LogInformation($"SensorId: {userId}");

            using var scope = _scopeFactory.CreateScope();
            var dbContextService = scope.ServiceProvider.GetRequiredService<IDbContextService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var (user, error) = await userService.GetUserByIdAsync(userId);

            var dbContext = await dbContextService.GetUserDatabaseContext(user);

            // check for duplicate
            var existingLog = await dbContext.Set<WaterQualityLog>().AnyAsync(log => log.TimeStamp == logEntry.TimeStamp);

            if (!existingLog)
            {
                dbContext.Set<WaterQualityLog>().Add(logEntry);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Stored WaterQualityLog with id: {logEntry.Id} and offset: {offset}");
            }
            else
            {
                _logger.LogInformation($"Skipping storing WaterQualityLog with offset {logEntry.Offset} due to it being a duplicate.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to handle message from topic {topic}. Error: {ex.Message}");
        }
    }
}