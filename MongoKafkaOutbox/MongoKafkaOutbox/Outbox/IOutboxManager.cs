using Avro.Specific;

namespace MongoKafkaOutbox.Outbox;

public interface IOutboxManager
{
    public Task PublishMessageWithOutbox<T>(T message) where T : ISpecificRecord;
}