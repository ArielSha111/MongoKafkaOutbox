using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace MongoKafkaOutbox.Mongo;

public abstract class MongoDBService : IMongoDBService<BsonDocument>
{
    protected IMongoDatabase _database { get; set; }
    public abstract IMongoCollection<BsonDocument> Collection { get; set; }
    public abstract IMongoCollection<OutboxEvent> OutboxCollection { get; set; }


    public async Task AddToBothCollectionsWithTransaction(Func<Task> mainTask, Func<Task> outboxTask)
    {
        //using var session = await _database.Client.StartSessionAsync();
        //session.StartTransaction();
        //try
        //{
        //    await mainTask();
        //    await outboxTask();
        //    await session.CommitTransactionAsync();
        //}
        //catch (Exception ex)
        //{
        //    await session.AbortTransactionAsync();
        //    throw ex;
        //}

        await mainTask();
        await outboxTask();
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
