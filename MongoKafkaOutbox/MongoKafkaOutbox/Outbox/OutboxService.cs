

using MongoDB.Bson;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;

namespace MongoKafkaOutbox.Outbox;

public class OutboxService(IMongoDBService mongoDBService, IKafkaService kafkaService) : IOutboxService
{
    private BsonDocument TempDocument { get; set; }
    private OutboxEvent TempEvent { get; set; }


    public virtual async Task Add(BsonDocument document)
    {
       
        try
        {
            TempDocument = document;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex}");
            throw;
        }
    }

    public virtual async Task Publish<T>(T eventData, string topic)
    {    
        TempEvent = new OutboxEvent()
        {
            Topic = topic,
            EventData = eventData,
            eventStatus = OutboxEventStatus.Stored
        };
    }

    public virtual async Task<bool> SaveChanges()
    {
        try
        {
            await mongoDBService.AddToBothCollectionsWithTransaction(TempEvent, TempDocument);

            //todo, remove from here as it should be a standalone publisher that does that using redis locks and dates the avoid starvation, https://debezium.io/ may also solve it
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var eventToPublish = await mongoDBService.ReadAndUpdateOutbox();
                        await kafkaService.ProduceMessageAsync(eventToPublish);
                        await mongoDBService.UpdateOutbox(eventToPublish.Id);
                        return;
                    }
                    catch
                    {
                        Console.WriteLine("");
                    }
                }
            });

            return true;

        }
        catch (Exception)
        {
            return false;
        }
    }
}
