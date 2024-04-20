using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Mongo;
using MongoKafkaOutbox.Outbox;

namespace ConsoleApp2;

public class MyMongoDBService : MongoDBService
{
    protected override IMongoCollection<BsonDocument> Collection { get; set; }
    protected override IMongoCollection<OutboxEvent> OutboxCollection { get; set; }

    public MyMongoDBService()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase("KafkaOutbox");
        Collection = _database.GetCollection<BsonDocument>("Stuff");
        OutboxCollection = _database.GetCollection<OutboxEvent>("Outbox");
    }
}
