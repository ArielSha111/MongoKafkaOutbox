
using Avro.Specific;
using MongoKafkaOutbox2.MongoSessionHandlers;

namespace MongoKafkaOutbox2.Outbox;

public interface IOutboxManager
{
    public Task<IOutboxClientSessionHandle> StartOutboxSessionAsync();

    public Task PublishMessage<T>(T message) where T : ISpecificRecord;
}