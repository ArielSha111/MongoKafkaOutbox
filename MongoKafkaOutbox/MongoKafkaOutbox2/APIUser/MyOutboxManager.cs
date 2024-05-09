using MongoDB.Driver;
using MongoKafkaOutbox2.Outbox;
using MongoKafkaOutbox2.Serialization.Avro;

namespace MongoKafkaOutbox2.APIUser;

public class MyOutboxManager : OutboxManagerBase, IMyOutboxManager
{
    public IMongoCollection<object> GeneralCollection { get; set; }

    public MyOutboxManager(IMongoClient mongoClient, IAvroSerializationManager avroSerializationManager) : base(mongoClient, avroSerializationManager)
    {
        GeneralCollection = _database.GetCollection<object>("General");
    }
}
