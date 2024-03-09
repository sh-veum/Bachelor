using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

class Consumer
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

        string uniqueGroupId = $"kafka-dotnet-{Guid.NewGuid()}";
        configuration["group.id"] = uniqueGroupId; // Unique consumer group ID
        configuration["auto.offset.reset"] = "earliest"; // Start from the earliest message present in the topic

        List<string> topics = new List<string> { "species-updates", "org-updates", "rest-key-updates", "graphql-key-updates", "water-quality-updates" };

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using (var consumer = new ConsumerBuilder<string, string>(configuration.AsEnumerable()).Build())
        {
            consumer.Subscribe(topics);

            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"Consumed event from topic {cr.Topic}: key = {cr.Message.Key,-10} value = {cr.Message.Value}");
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl-C was pressed.
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}