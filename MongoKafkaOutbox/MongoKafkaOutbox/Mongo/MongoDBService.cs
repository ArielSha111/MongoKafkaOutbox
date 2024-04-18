using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public class MongoDBService
{
    private IMongoDatabase _database;

    public MongoDBService(string connectionString, string databaseName)
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase("KafkaOutbox");
    }

    public IMongoCollection<OutboxEvent> OutboxCollection => _database.GetCollection<OutboxEvent>("Outbox");//todo check if its ok to retrive it every time
    public IMongoCollection<OutboxState> OutboxStateCollection => _database.GetCollection<OutboxState>("OutboxState");
    public IMongoCollection<BsonDocument> InboxCollection => _database.GetCollection<BsonDocument>("Inbox");
    public IMongoCollection<BsonDocument> StuffCollection => _database.GetCollection<BsonDocument>("Stuff");
}