using Avro.Specific;
using MongoDB.Driver;

namespace MongoKafkaOutbox.Outbox;

public interface IOutboxManager
{
    public Task<IClientSessionHandle> StartOutboxSessionAsync();

    public Task PublishMessage<T>(T message) where T : ISpecificRecord;
}