namespace MongoKafkaOutbox.Messaging;

public abstract class KafkaOutbox : IKafkaOutbox
{
    public abstract Task Publish<T>(T value, string topic);
}
