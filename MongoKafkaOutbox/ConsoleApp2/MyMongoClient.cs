using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Mongo;
using MongoKafkaOutbox.Outbox;

namespace ConsoleApp2;

public class MyMongoClient : MongoOutbox
{
    public override IMongoCollection<OutboxEvent> OutboxCollection { get; set; }


    protected IMongoDatabase _database { get; set; }

    public IMongoCollection<BsonDocument> Collection { get; set; }


    public MyMongoClient()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase("KafkaOutbox");
        Collection = _database.GetCollection<BsonDocument>("Stuff");
        OutboxCollection = _database.GetCollection<OutboxEvent>("Outbox");
    }

    public async Task AddStuff(BsonDocument obj)
    {
        await Collection.InsertOneAsync(obj);
    }
}
