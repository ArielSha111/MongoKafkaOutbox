using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoKafkaOutbox.Outbox;
using System.Text.Json;

namespace ConsoleApp3
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var mongoClient = new MongoClient("mongodb://localhost:28017");
            var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
            var kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
            var outboxCollection = mongoClient.GetDatabase("attachment-api-local-dev").GetCollection<OutboxRecord>("outbox");
            var usersCollection = mongoClient.GetDatabase("attachment-api-local-dev").GetCollection<User>("users");

            using var session = await mongoClient.StartSessionAsync();

            try
            {
                session.StartTransaction();

                var newUser = new User { Name = "Random User" + Guid.NewGuid().ToString() };
                await usersCollection.InsertOneAsync(session, newUser);

                var outboxRecord = new OutboxRecord
                {
                    EventStatus = OutboxEventStatus.Stored,
                    EventType = "UserCreated",
                    EventData = JsonSerializer.Serialize(newUser),
                };

                await outboxCollection.InsertOneAsync(session, outboxRecord);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"Error occurred:{ex}");
                throw ex;
            }

            //this part should be done by debezium and mongo cdc
            //try
            //{
            //    session.StartTransaction();

            //    var filter = Builders<OutboxRecord>.Filter.Eq(e => e.EventStatus, OutboxEventStatus.Stored);
            //    var update = Builders<OutboxRecord>.Update.Set(e => e.EventStatus, OutboxEventStatus.InProcess);

            //    var options = new FindOneAndUpdateOptions<OutboxRecord>
            //    {
            //        ReturnDocument = ReturnDocument.After
            //    };

            //    var outboxEvent = outboxCollection.FindOneAndUpdate(session, filter, update, options);

            //    if (outboxEvent != null)
            //    {
            //        session.CommitTransaction();

            //        var result = await kafkaProducer.ProduceAsync("my_topic", new Message<string, string>
            //        { Value = outboxEvent.EventData });
            //        Console.WriteLine($"Produced message '{outboxEvent.EventData}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");
            //    }
            //    else
            //    {
            //        session.AbortTransaction();
            //        throw new Exception();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    session.AbortTransaction();
            //    Console.WriteLine($"Error occurred:{ex}");
            //    throw ex;
            //}


            try
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = "localhost:9092",
                    GroupId = "my_consumer_group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };


                using var consumer = new ConsumerBuilder<string, string>(config).Build();              
                consumer.Subscribe("my_topic");
                
                var message = consumer.Consume();
                if(message is not null)
                    Console.WriteLine($"Consumed message '{message.Message.Value}' from topic {message.Topic}, partition {message.Partition}, offset {message.Offset}");
                         
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred:{ex}");
            }        
        }
    }
}

public class User
{
    public string Name { get; set; }
}

public class OutboxRecord
{
    public ObjectId Id { get; set; }
    public OutboxEventStatus EventStatus { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
}