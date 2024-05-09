using MongoDB.Driver;
using MongoKafkaOutbox2.Serialization.Avro;
using MongoKafkaOutbox2.Dtos;
using MongoKafkaOutbox2.MongoSessionHandlers;
using Avro.Specific;

namespace MongoKafkaOutbox2.Outbox;

public abstract class OutboxManagerBase : IOutboxManager
{
    protected IMongoClient _mongoClient;
    protected IMongoDatabase _database;
    private IAvroSerializationManager _avroSerializationManager;
    private IMongoCollection<OutboxAvroDto> _outboxCollection { get; set; }

    public OutboxManagerBase(IMongoClient mongoClient,  IAvroSerializationManager avroSerializationManager, string outboxCollectionName, string dbName)
    {
        _database = mongoClient.GetDatabase(dbName);
        _outboxCollection = _database.GetCollection<OutboxAvroDto>(outboxCollectionName);
        _avroSerializationManager = avroSerializationManager;
    }

    public async Task PublishMessage<T>(T message) where T: ISpecificRecord
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

    public async Task<IOutboxClientSessionHandle> StartOutboxSessionAsync()
    {
        var clientSessionHandle = await _mongoClient.StartSessionAsync();
        return new OutboxClientSessionHandle(clientSessionHandle);
    }
}
