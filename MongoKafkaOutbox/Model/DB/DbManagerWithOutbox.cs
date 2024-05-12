using Contracts;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;

namespace Model.DB;

public class DbManagerWithOutbox : IDbManagerWithOutBox
{
    protected IAvroOutboxManager _outboxManager;
    protected IMongoClient _mongoClient;
    protected IMongoDatabase _database;
    public IMongoCollection<Person> _persons { get; set; }

    public DbManagerWithOutbox(IMongoClient mongoClient, IAvroOutboxManager outboxManager)
    {
        _outboxManager = outboxManager;
        _mongoClient = mongoClient;
        _database = mongoClient.GetDatabase("KafkaOutbox");
        _persons = _database.GetCollection<Person>("Persons");
    }

    public async Task PutStuffInDbWithOutbox()
    {
        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();
        try
        {
            await _persons.InsertOneAsync(new Person());
            await _outboxManager.PublishMessageWithOutbox(new Event());
            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
        }
    }
}