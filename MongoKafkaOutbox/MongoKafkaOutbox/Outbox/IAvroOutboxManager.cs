using Avro.Specific;

namespace MongoKafkaOutbox.Outbox;

public interface IAvroOutboxManager
{
    public Task PublishMessageWithOutbox<T>(T message) where T : ISpecificRecord;
}