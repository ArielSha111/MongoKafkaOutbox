using MongoDB.Driver;
using MongoKafkaOutbox2.Serialization.Avro;

namespace MongoKafkaOutbox2.Outbox.Default;

internal class GenericOutboxManager<T> : OutboxManagerBase, IGenericOutboxManager<T>
{
    public IMongoCollection<T> Collection { get; set; }

    public GenericOutboxManager(IMongoClient mongoClient, IAvroSerializationManager avroSerializationManager, 
        string collectionName, string outboxCollectionName, string dbName) :
        base(mongoClient, avroSerializationManager, outboxCollectionName, dbName)
    {
        Collection = _database.GetCollection<T>(collectionName);
    }
}
