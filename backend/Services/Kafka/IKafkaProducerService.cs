namespace NetBackend.Services.Kafka;

public interface IKafkaProducerService
{
    Task ProduceAsync<T>(string topic, T message);
}