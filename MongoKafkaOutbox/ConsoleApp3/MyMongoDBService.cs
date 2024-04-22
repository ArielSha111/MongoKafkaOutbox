using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Mongo;
using MongoKafkaOutbox.Outbox;

namespace ConsoleApp3;

public class MyMongoDBService : MongoDBService
{
    public override IMongoCollection<BsonDocument> Collection { get; set; }
    public override IMongoCollection<OutboxEvent> OutboxCollection { get; set; }

    public MyMongoDBService()
    {
        var client = new MongoClient("mongodb://localhost:28017");
        _database = client.GetDatabase("attachment-api-local-dev");
        Collection = _database.GetCollection<BsonDocument>("Stuff");
        OutboxCollection = _database.GetCollection<OutboxEvent>("Outbox");
    }
}
