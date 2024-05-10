using MongoDB.Driver;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Serialization.Avro;

namespace MongoKafkaOutbox.Outbox.Default;

internal class GenericOutboxManager<T> : OutboxManagerBase, IGenericOutboxManager<T>
{
    public IMongoCollection<T> Collection { get; set; }

    public GenericOutboxManager(IMongoClient mongoClient, IAvroSerializationManager avroSerializationManager,
       OutboxConfigurationBlock outboxConfigurationBlock) :
        base(mongoClient, avroSerializationManager, outboxConfigurationBlock)
    {
        Collection = _database.GetCollection<T>(outboxConfigurationBlock.MongoCollectionNames["MainCollection"]);
    }
}
