using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public class MongoDBService : IMongoDBService
{
    private IMongoDatabase _database;
    public IMongoCollection<BsonDocument> StuffCollection => _database.GetCollection<BsonDocument>("Stuff");

    public IMongoCollection<OutboxEvent> OutboxCollection => _database.GetCollection<OutboxEvent>("Outbox");

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

    public async Task<OutboxEvent> ReadAndUpdateOutbox()
    {
        //var filter = Builders<OutboxEvent>.Filter.Eq(e => e.eventStatus, OutboxEventStatus.Stored);
        //var update = Builders<OutboxEvent>.Update.Set(e => e.eventStatus, OutboxEventStatus.InProcess);

        //using (var session = _database.Client.StartSession())
        //{
        //    session.StartTransaction();

        //    try
        //    {
        //        var options = new FindOneAndUpdateOptions<OutboxEvent>
        //        {
        //            ReturnDocument = ReturnDocument.After
        //        };

        //        var outboxEvent = OutboxCollection.FindOneAndUpdate(session, filter, update, options);

        //        if (outboxEvent != null)
        //        {
        //            session.CommitTransaction();
        //            return outboxEvent;
        //        }
        //        else
        //        {
        //            session.AbortTransaction();
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        session.AbortTransaction();
        //        throw ex;
        //    }
        //}

        var filter = Builders<OutboxEvent>.Filter.Eq(e => e.eventStatus, OutboxEventStatus.Stored);
        var update = Builders<OutboxEvent>.Update.Set(e => e.eventStatus, OutboxEventStatus.InProcess);

        var outboxEvent = await OutboxCollection.FindOneAndUpdateAsync(filter, update);

        return outboxEvent;
    }

    public async Task UpdateOutbox(ObjectId objectId)
    {
        var filter = Builders<OutboxEvent>.Filter.Eq(e => e.Id, objectId);
        var update = Builders<OutboxEvent>.Update.Set(e => e.eventStatus, OutboxEventStatus.Published);
        await OutboxCollection.UpdateOneAsync(filter, update);
    }
}
