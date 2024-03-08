namespace NetBackend.Services.Interfaces;

public interface IKafkaProducerService
{
    Task ProduceAsync<T>(string topic, T message);
}