using Confluent.Kafka;
using MongoKafkaOutbox.Messaging;

namespace ConsoleApp2;

public class MyKafkaService : KafkaService
{
    private readonly string _bootstrapServers;

    public MyKafkaService()
    {
        _bootstrapServers = "localhost:9092";

        ProducerConfig = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers
        };
    }
}
