using MongoDB.Driver;
using Avro.Specific;
using MongoKafkaOutbox.Serialization.Avro;
using MongoKafkaOutbox.Contracts;

namespace MongoKafkaOutbox.Outbox;

public abstract class OutboxManagerBase : IOutboxManager
{
    protected IMongoClient _mongoClient;
    protected IMongoDatabase _database;
    private IAvroSerializationManager _avroSerializationManager;
    private IMongoCollection<OutboxAvroDto> _outboxCollection { get; set; }

    public OutboxManagerBase(IMongoClient mongoClient, IAvroSerializationManager avroSerializationManager,
        OutboxConfigurationBlock outboxConfigurationBlock)
    {
        _mongoClient = mongoClient;
        _database = mongoClient.GetDatabase(outboxConfigurationBlock.MongoDBName);
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

    public async Task<IClientSessionHandle> StartOutboxSessionAsync()
    {
        return await _mongoClient.StartSessionAsync();
    }
}
