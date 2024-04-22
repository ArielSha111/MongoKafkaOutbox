using Confluent.Kafka;
using MongoKafkaOutbox.Messaging;

namespace ConsoleApp2;

public class MyKafkaProducer : KafkaOutbox
{
    private readonly string _bootstrapServers;

    public MyKafkaProducer()
    {
        _bootstrapServers = "localhost:9092";       
    }

    public override async Task Publish<T>(T eventData, string topic)
    {
        var message = "";

        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        try
        {             
            var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Produced message '{message}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");               
        }
        catch (ProduceException<Null, string> ex)
        {
            Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
        }
    }
}
