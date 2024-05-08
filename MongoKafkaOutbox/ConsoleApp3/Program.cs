using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using ConsoleApp3;
using Confluent.Kafka.SyncOverAsync;
using MongoDB.Bson;
using MongoDB.Driver;
using Avro;
using static Confluent.Kafka.ConfigPropertyNames;

class Program
{
    const string bootstrapServers = "localhost:19092";
    const string schemaRegistryUrl = "http://localhost:8081";
    const string topicName = "my-topic4";
    const string consumerGroup = "my_consumer_group";
    const string mongoConnectionString = "mongodb://localhost:28017";


    public static async Task Main()
    {
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = schemaRegistryUrl
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);


        StartProducer(schemaRegistry);
        StartConsumer(schemaRegistry);

        Console.ReadLine();
    }

    private static async Task StartProducer(CachedSchemaRegistryClient schemaRegistry)
    {
        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase("KafkaOutbox");
        var mainCollection = database.GetCollection<Person>("MainCollection");
        var outboxCollection = database.GetCollection<BsonDocument>("Outbox");


        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };


        var serializer = new AvroSerializer<Person>(schemaRegistry);

        using var producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
        var serializationContext = new SerializationContext(MessageComponentType.Value, topicName);
        while (true)
        {
            try
            {

                var person = new Person
                {
                    Age = 13,
                    Name = Guid.NewGuid().ToString(),
                };


                var serializedBytes = await serializer.SerializeAsync(person, serializationContext);

                // Storing message in MongoDB
                var doc = new BsonDocument
                {
                    { "Topic", topicName},
                    { "serializedBytes", serializedBytes },
                    { "DateTime", DateTime.Now}
                };

                using var session = await mongoClient.StartSessionAsync();
                
                session.StartTransaction();
                try
                {
                    await mainCollection.InsertOneAsync(person);
                    await outboxCollection.InsertOneAsync(doc);
                    // Commit the transaction
                    await session.CommitTransactionAsync();

                    Console.WriteLine("Transaction committed successfully.");
                }

                catch (Exception ex)
                {             
                    await session.AbortTransactionAsync();
                    Console.WriteLine("Transaction aborted due to an error: " + ex.Message);
                }



                // Retrieve the document from the collection
                var sort = Builders<BsonDocument>.Sort.Descending("DateTime");

                // Retrieve the latest document from the collection
                var latestDocument = await outboxCollection.Find(new BsonDocument()).Sort(sort).FirstOrDefaultAsync();


                if (latestDocument != null)
                {
                    Console.WriteLine("Document found:");
                    Console.WriteLine(latestDocument);

                    var serializedBytesFromMongo = latestDocument.GetValue("serializedBytes").AsByteArray;
                    var result = await producer.ProduceAsync(topicName, new Message<Null, byte[]> { Value = serializedBytesFromMongo });
                    Console.WriteLine($"produce message with person: {person.Name}, {BitConverter.ToString(result.Message.Value).Replace("-", "")}");
                }
                else
                {
                    Console.WriteLine("Document not found.");
                }

                
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }

    private static async Task StartConsumer(CachedSchemaRegistryClient schemaRegistry)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };


        using var consumer = new ConsumerBuilder<Null, Person>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<Person>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build();


        consumer.Subscribe(topicName);


        while (true)
        {
            try
            {
                var cr = consumer.Consume();
                var person = cr.Message.Value;
                Console.WriteLine($"Consumed message with person: {person.Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
