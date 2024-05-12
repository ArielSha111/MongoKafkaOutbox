using MongoDB.Driver;
using Avro.Specific;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Serialization;

namespace MongoKafkaOutbox.Outbox;

public class AvroOutboxManager : IAvroOutboxManager
{
    protected IMongoClient _mongoClient;
    protected IMongoDatabase _database;
    private IAvroSerializationManager _avroSerializationManager;
    private IMongoCollection<OutboxAvroDto> _outboxCollection { get; set; }

    public AvroOutboxManager(IMongoClient mongoClient,
        IAvroSerializationManager avroSerializationManager,
        OutboxConfigurationBlock outboxConfigurationBlock)
    {
        _mongoClient = mongoClient;
        _database = mongoClient.GetDatabase(outboxConfigurationBlock.OutboxDbName);
        _outboxCollection = _database.GetCollection<OutboxAvroDto>(outboxConfigurationBlock.OutboxCollectionName);
        _avroSerializationManager = avroSerializationManager;
    }

    public async Task PublishMessageWithOutbox<T>(T message) where T : ISpecificRecord
    {
        var avroMessage = await _avroSerializationManager.GetAsAvroAsync(message);

        await _outboxCollection.InsertOneAsync
            (
                new OutboxAvroDto
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTime.Now,
                    Payload = avroMessage
                }
            );
    }
}
