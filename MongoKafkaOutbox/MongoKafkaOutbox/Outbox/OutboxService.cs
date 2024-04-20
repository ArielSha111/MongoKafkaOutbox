using MongoDB.Bson;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;


namespace MongoKafkaOutbox.Outbox;

public class OutboxService
{
    private MongoDBService _mongoDBService;
    private KafkaService _kafkaService;

    public BsonDocument StuffDocument { get; set; }
    public OutboxEvent OutboxEvent { get; set; }


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
        OutboxEvent = new OutboxEvent()
        {
            Id = ObjectId.GenerateNewId(),
            SessionId = "",
            EventData = eventData,
            Sent = false
        };
    }

 
    public async Task<bool> SaveChanges()
    {
        try
        {
            await _mongoDBService.AddToBothCollectionsWithTransaction(OutboxEvent, StuffDocument);
            //todo get the event from mongo while setting its state
            await _kafkaService.ProduceMessageAsync(OutboxEvent);
            return true;
        }
        catch (Exception)
        {

            return false;
        }
    }
}
