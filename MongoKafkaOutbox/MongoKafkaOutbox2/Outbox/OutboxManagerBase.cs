using MongoDB.Driver;
using MongoKafkaOutbox2.Serialization.Avro;
using MongoKafkaOutbox2.Dtos;

namespace MongoKafkaOutbox2.Outbox;

public abstract class OutboxManagerBase : IOutboxManager
{
    protected IMongoClient _mongoClient;
    protected IMongoDatabase _database;
    private IAvroSerializationManager _avroSerializationManager;
    private IMongoCollection<OutboxAvroDto> _outboxCollection { get; set; }

    public OutboxManagerBase(IMongoClient mongoClient,  IAvroSerializationManager avroSerializationManager)
    {
        _database = mongoClient.GetDatabase("");
        _outboxCollection = _database.GetCollection<OutboxAvroDto>("Outbox");
        _avroSerializationManager = avroSerializationManager;
    }

    public async Task PublishMessage<T>(T message)
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

    public async Task<IOutboxClientSessionHandle> StartSessionAsync()
    {
        var clientSessionHandle = await _mongoClient.StartSessionAsync();
        return new OutboxClientSessionHandle(clientSessionHandle);
    }
}
