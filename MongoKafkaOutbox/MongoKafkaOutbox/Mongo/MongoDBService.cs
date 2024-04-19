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

    public IMongoCollection<OutboxEvent> OutboxCollection => _database.GetCollection<OutboxEvent>("Outbox");//todo check if its ok to retrieve it every time
    public IMongoCollection<BsonDocument> StuffCollection => _database.GetCollection<BsonDocument>("Stuff");
    
    public async Task AddToBothCollectionsWithTransaction(OutboxEvent outboxEvent, BsonDocument stuffDocument)
    {
        var sessionOptions = new ClientSessionOptions { };
        using (var session = await _database.Client.StartSessionAsync(sessionOptions))
        {
            try
            {
                session.StartTransaction();

                // Insert document into Outbox collection
                await OutboxCollection.InsertOneAsync(session, outboxEvent);

                // Insert document into Stuff collection
                await StuffCollection.InsertOneAsync(session, stuffDocument);

                // Commit the transaction
                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex}");
                // Abort transaction if any error occurs
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}