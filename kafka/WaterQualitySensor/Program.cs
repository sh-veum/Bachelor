using Microsoft.Extensions.Configuration;
using WaterQualitySensor.Sensor;

namespace WaterQualitySensor;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide the configuration file path as a command line argument");
            return;
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddIniFile(args[0])
            .Build();

        const string topic = "water-quality-updates";

        var sensor = new MockWaterQualitySensor(configuration, topic);
        sensor.Start();
    }
}