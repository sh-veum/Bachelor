namespace NetBackend.Services.Interfaces;

public interface IKafkaConsumerService
{
    void SubscribeToTopic(string newTopic);
    List<string> GetSubscribedTopics();
}