using Avro.Generic;
using Avro.Specific;

namespace MongoKafkaOutbox.Outbox;

public interface IAvroOutboxManager
{
    public Task PublishMessageWithOutbox(ISpecificRecord message);
    public Task PublishMessageWithOutbox(GenericRecord message);
}