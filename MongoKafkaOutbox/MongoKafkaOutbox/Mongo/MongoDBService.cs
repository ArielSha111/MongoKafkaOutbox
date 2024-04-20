using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public class MongoDBService
{
    private IMongoDatabase _database;

    public MongoDBService()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase("KafkaOutbox");
    }

    public IMongoCollection<OutboxEvent> OutboxCollection => _database.GetCollection<OutboxEvent>("Outbox");

    public IMongoCollection<BsonDocument> StuffCollection => _database.GetCollection<BsonDocument>("Stuff");
    
    public async Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument)
    {
        await OutboxCollection.InsertOneAsync(outboxEvent);
        await StuffCollection.InsertOneAsync(stuffDocument);          
    }


    //public async Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument)
    //{
    //    // Start a client session
    //    using (var session = await _database.Client.StartSessionAsync())
    //    {
    //        // Begin a transaction
    //        session.StartTransaction();

    //        try
    //        {
    //            // Insert into the outbox collection
    //            await OutboxCollection.InsertOneAsync(session, outboxEvent);

    //            // Insert into the stuff collection
    //            await StuffCollection.InsertOneAsync(session, stuffDocument);

    //            // Commit the transaction
    //            await session.CommitTransactionAsync();
    //        }
    //        catch (Exception ex)
    //        {
    //            // If any operation fails, abort the transaction
    //            await session.AbortTransactionAsync();
    //            throw ex; // or handle the exception as needed
    //        }
    //    }
    //}
}
