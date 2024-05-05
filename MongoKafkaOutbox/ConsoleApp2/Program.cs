using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;

class Program
{
    static async Task Main(string[] args)
    {
        const string bootstrapServers = "localhost:29092";
        const string topic = "test";
        const string mongoConnectionString = "mongodb://localhost:27017";
        const string dbName = "testdb";
        const string collectionName = "outbox";

        var client = new MongoClient(mongoConnectionString);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        var producerTask = ProduceMessage(bootstrapServers, topic, client, collection);
        var consumerTask = ConsumeMessage(bootstrapServers, topic);

        await Task.WhenAll(producerTask, consumerTask);
    }

    static async Task ProduceMessage(string bootstrapServers, string topic, MongoClient client, IMongoCollection<BsonDocument> collection)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        using var session = await client.StartSessionAsync();

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        try
        {
            while (true)
            {
                var message = Console.ReadLine();

                session.StartTransaction();

                var doc = new BsonDocument
                {
                    { "Topic", topic },
                    { "Message", message }
                };

                await collection.InsertOneAsync(doc);
                var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

                await session.CommitTransactionAsync();
            }
        }
        catch (ProduceException<Null, string> ex)
        {
            await session.AbortTransactionAsync();
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
        }
    }

    static async Task ConsumeMessage(string bootstrapServers, string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "aaaa",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };


        using var consumer = new ConsumerBuilder<Null, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (true)
            {
                var message = consumer.Consume();
                Console.WriteLine($"Consumed message '{message.Message.Value}' from topic {message.Topic}, partition {message.Partition}, offset {message.Offset}");
            }
        }
        catch (ConsumeException ex)
        {
            Console.WriteLine($"Error occurred: {ex.Error.Reason}");
        }
    }
}