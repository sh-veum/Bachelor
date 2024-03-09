using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace WaterQualitySensor.Sensor;

public class MockWaterQualitySensor(IConfiguration configuration, string topic)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly string _topic = topic;

    public void Start()
    {
        using var producer = new ProducerBuilder<string, string>(_configuration.AsEnumerable()).Build();
        var rnd = new Random();
        while (true)
        {
            var waterQualityMetric = $"pH: {rnd.NextDouble() * 14:0.00}, Temperature: {rnd.Next(0, 35)}C";
            producer.Produce(_topic, new Message<string, string> { Key = DateTime.UtcNow.ToString("o"), Value = waterQualityMetric },
                (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced event to topic {_topic}: key = {deliveryReport.Message.Key,-20} value = {deliveryReport.Message.Value}");
                    }
                });

            producer.Flush(TimeSpan.FromSeconds(1));
            Thread.Sleep(1000);
        }
    }
}