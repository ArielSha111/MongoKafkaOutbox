using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MongoDB.Bson;
using MongoDB.Driver;

class Program
{
    static async Task Main(string[] args)
    {
        const string bootstrapServers = "localhost:19092";
        const string topic = "test_topic";
        const string mongoConnectionString = "mongodb://localhost:27017";
        const string dbName = "testdb";
        const string collectionName = "outbox";

        var client = new MongoClient(mongoConnectionString);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        var producerTask = ProduceMessage(bootstrapServers, topic, collection);
        var consumerTask = ConsumeMessage(bootstrapServers, topic);

        await Task.WhenAll(producerTask, consumerTask);
    }

    static async Task ProduceMessage(string bootstrapServers, string topic, IMongoCollection<BsonDocument> collection)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                while (true)
                {
                    var message = Console.ReadLine();
              
                    var doc = new BsonDocument
                    {
                        { "Topic", topic },
                        { "Message", message }
                    };
                    await collection.InsertOneAsync(doc);

                    var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
                    Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");
                }
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
            }
        }
    }

    static async Task ConsumeMessage(string bootstrapServers, string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "my_consumer_group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
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
}
