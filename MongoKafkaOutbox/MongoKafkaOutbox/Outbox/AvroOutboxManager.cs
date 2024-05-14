using MongoDB.Driver;
using Avro.Specific;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Serialization;
using Avro.Generic;

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

    public async Task PublishMessageWithOutbox(ISpecificRecord message)
    {
        await PublishMessageWithOutbox<ISpecificRecord>(message);
    }

    public async Task PublishMessageWithOutbox(GenericRecord message)
    {
        await PublishMessageWithOutbox<GenericRecord>(message);
    }

    private async Task PublishMessageWithOutbox<T>(T message)
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
