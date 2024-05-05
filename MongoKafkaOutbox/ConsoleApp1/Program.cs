using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;


class Program
{
    static async Task Main(string[] args)
    {
        string bootstrapServers = "localhost:9092";
        string topic = "my_topic";

        // MongoDB connection string
        string mongoConnectionString = "mongodb://localhost:27017";

        // Start producer, consumer, and message storage asynchronously
        var producerTask = ProduceMessage(bootstrapServers, topic);
        var consumerTask = ConsumeMessage(bootstrapServers, topic, mongoConnectionString);

        // Wait for tasks to finish
        await Task.WhenAll(producerTask, consumerTask);
    }

    static async Task ProduceMessage(string bootstrapServers, string topic)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                while (true)
                {
                    var message = Console.ReadLine();
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

    static async Task ConsumeMessage(string bootstrapServers, string topic, string mongoConnectionString)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "my_consumer_group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var client = new MongoClient(mongoConnectionString);
        var database = client.GetDatabase("KafkaOutbox");
        var collection = database.GetCollection<BsonDocument>("Test1");

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(topic);

            try
            {
                while (true)
                {
                    var message = consumer.Consume();
                    Console.WriteLine($"Consumed message '{message.Message.Value}' from topic {message.Topic}, partition {message.Partition}, offset {message.Offset}");

                    // Storing message in MongoDB
                    var doc = new BsonDocument
                    {
                        { "Topic", message.Topic },
                        { "Partition", message.Partition.Value },
                        { "Offset", message.Offset.Value },
                        { "Message", message.Message.Value }
                    };

                    try
                    {
                        await collection.InsertOneAsync(doc);
                    }
                    catch (Exception ex)
                    {
                        // Rollback consumption by seeking back to the last committed offset
                        consumer.Seek(new TopicPartitionOffset(message.TopicPartition, message.Offset));
                        Console.WriteLine($"Failed to store message in MongoDB. Rolling back consumption to offset {message.Offset}");
                    }
                }
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"Error occurred: {ex.Error.Reason}");
            }
        }
    }
}