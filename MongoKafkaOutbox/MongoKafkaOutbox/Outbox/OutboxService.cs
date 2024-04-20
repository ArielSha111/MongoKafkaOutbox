using MongoDB.Bson;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;


namespace MongoKafkaOutbox.Outbox;

public class OutboxService
{
    private IMongoDBService _mongoDBService;
    private IKafkaService _kafkaService;

    public BsonDocument StuffDocument { get; set; }
    public OutboxEvent TempEvent { get; set; }


    public OutboxService()
    {
        _mongoDBService = new MongoDBService();
        _kafkaService = new KafkaService();
    }

    public async Task Add(BsonDocument stuffDocument)
    {
        try
        {
            StuffDocument = stuffDocument;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex}");
            throw;
        }
    }

    public async Task Publish<T>(T eventData)
    {     
        TempEvent = new OutboxEvent()
        {
            EventData = eventData,
            eventStatus = OutboxEventStatus.Stored
        };
    }

 
    public async Task<bool> SaveChanges()
    {
        try
        {
            await _mongoDBService.AddToBothCollectionsWithTransaction(TempEvent, StuffDocument);

            //todo, remove from here as it should be a standalone publisher that does that using redis locks and dates the avoid starvation
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var eventToPublish = await _mongoDBService.ReadAndUpdateOutbox();
                        await _kafkaService.ProduceMessageAsync(eventToPublish);
                        await _mongoDBService.UpdateOutbox(eventToPublish.Id);
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
