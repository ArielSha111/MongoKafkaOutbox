using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public class MongoDBService
{
    private IMongoDatabase _database;

    public IMongoCollection<OutboxEvent> OutboxCollection => _database.GetCollection<OutboxEvent>("Outbox");
    public IMongoCollection<BsonDocument> StuffCollection => _database.GetCollection<BsonDocument>("Stuff");

    public MongoDBService()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase("KafkaOutbox");
    }

    public async Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument)
    {
        //using var session = await _database.Client.StartSessionAsync();
        //session.StartTransaction();
        //try
        //{
        //    await OutboxCollection.InsertOneAsync(session, outboxEvent);
        //    await StuffCollection.InsertOneAsync(session, stuffDocument);
        //    await session.CommitTransactionAsync();
        //}
        //catch (Exception ex)
        //{
        //    await session.AbortTransactionAsync();
        //    throw ex;
        //}
     
        await OutboxCollection.InsertOneAsync(outboxEvent);
        await StuffCollection.InsertOneAsync(stuffDocument);          
    }
}
