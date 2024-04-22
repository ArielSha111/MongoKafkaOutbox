using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Messaging;

public interface IKafkaOutbox
{
    public Task Publish<T>(T value, string topic);
}
