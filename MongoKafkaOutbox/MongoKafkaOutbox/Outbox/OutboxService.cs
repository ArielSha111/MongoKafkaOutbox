

using MongoDB.Bson;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;

namespace MongoKafkaOutbox.Outbox;

public class OutboxService<T>(IMongoDBService<T> mongoDBService, IKafkaService kafkaService) : IOutboxService<T>
{
    private Func<Task> mainTask { get; set; }
    private Func<Task> outboxTask { get; set; }


    public virtual async Task Add(T document)
    {
        mainTask = async () =>
        {
            await mongoDBService.Collection.InsertOneAsync(document);
        };      
    }

    public virtual async Task Publish<E>(E eventData, string topic)
    {
        var outboxEvent = new OutboxEvent()
        {
            Topic = topic,
            EventData = eventData,
            eventStatus = OutboxEventStatus.Stored
        };

        outboxTask = async () =>
        {
            await mongoDBService.OutboxCollection.InsertOneAsync(outboxEvent);
        };      
    }


    public virtual async Task<bool> SaveChanges()
    {
        try
        {
            await mongoDBService.AddToBothCollectionsWithTransaction(mainTask, outboxTask);

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
