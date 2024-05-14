using Avro;
using Avro.Generic;
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
            var person = new Person();
            await _persons.InsertOneAsync(person);

            var specificOutboxEvent = new Event() { StoredPerson = person };
            await _outboxManager.PublishMessageWithOutbox(specificOutboxEvent);

            var genericOutboxEvent = GetGenericOutboxEvent(person, specificOutboxEvent);
            await _outboxManager.PublishMessageWithOutbox(genericOutboxEvent);

            await session.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync();
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    private static GenericRecord GetGenericOutboxEvent(Person person, Event outboxEvent)
    {
        var personSchema = Schema.Parse(person.Schema.ToString()) as RecordSchema;
        var personRecord = new GenericRecord(personSchema);
        personRecord.Add("Name", person.Name);
        personRecord.Add("Age", person.Age);


        var eventSchema = Schema.Parse(outboxEvent.Schema.ToString()) as RecordSchema;
        var eventRecord = new GenericRecord(eventSchema);

        eventRecord.Add("Description", outboxEvent.Description);
        eventRecord.Add("Id", outboxEvent.Id);
        eventRecord.Add("StoredPerson", personRecord);
        return eventRecord;
    }
}