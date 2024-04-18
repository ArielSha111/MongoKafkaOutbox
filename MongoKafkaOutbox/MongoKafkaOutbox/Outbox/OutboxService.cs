using MongoDB.Driver;
using MongoKafkaOutbox.Messaging;
using MongoKafkaOutbox.Mongo;

namespace MongoKafkaOutbox.Outbox;

public class OutboxService
{
    private MongoDBService _mongoDBService;
    private KafkaService _kafkaService;

    public OutboxService(string mongoConnectionString, string mongoDatabaseName, string kafkaBootstrapServers, string schemaRegistryUrl)
    {
        _mongoDBService = new MongoDBService(mongoConnectionString, mongoDatabaseName);
        _kafkaService = new KafkaService(kafkaBootstrapServers, schemaRegistryUrl);
    }

    public string Store(object eventData)
    {
        var sessionId = Guid.NewGuid().ToString();
        var avroSerializedData = SerializeToAvro(eventData);
        var outboxEvent = new OutboxEvent { SessionId = sessionId, EventData = avroSerializedData.ToString(), Sent = false };

        _mongoDBService.OutboxCollection.InsertOne(outboxEvent);
        _mongoDBService.OutboxStateCollection.InsertOne(new OutboxState { SessionId = sessionId, Completed = false });

        return sessionId;
    }

    public async Task Publish(string sessionId)
    {
        var outboxEvent = await _mongoDBService.OutboxCollection.FindOneAndUpdateAsync(
            Builders<OutboxEvent>.Filter.Eq(e => e.SessionId, sessionId) & Builders<OutboxEvent>.Filter.Eq(e => e.Sent, false),
            Builders<OutboxEvent>.Update.Set(e => e.Sent, true));

        if (outboxEvent != null)
        {
            await _kafkaService.ProduceAvroMessageAsync("OutboxEvents", sessionId, outboxEvent.EventData);
        }
    }

    public bool SaveChanges(string sessionId)
    {
        var updatedOutboxState = _mongoDBService.OutboxStateCollection.UpdateOne(
            Builders<OutboxState>.Filter.Eq(s => s.SessionId, sessionId),
            Builders<OutboxState>.Update.Set(s => s.Completed, true));

        return updatedOutboxState.ModifiedCount > 0;
    }

    public async Task<bool> StoreAndPublish(string sessionId, object eventData)
    {
        var storedSessionId = Store(eventData);
        if (storedSessionId == null) 
            return false;

        await Publish(sessionId);
        return SaveChanges(sessionId);
    }

    public async Task StoreAndPublishWithoutSaving(string sessionId, object eventData)
    {
        var avroSerializedData = SerializeToAvro(eventData);
        await _kafkaService.ProduceAvroMessageAsync("OutboxEvents", sessionId, avroSerializedData);
    }

    private byte[] SerializeToAvro(object data)
    {
        // Implement AVRO serialization logic here
        return null; // Placeholder for AVRO serialized data
    }
}
