namespace NetBackend.Services.Interfaces.Kafka;

public interface IKafkaProducerService
{
    Task ProduceAsync<T>(string topic, T message);
}