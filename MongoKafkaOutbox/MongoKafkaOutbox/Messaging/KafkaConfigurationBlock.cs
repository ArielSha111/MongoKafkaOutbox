namespace MongoKafkaOutbox.Messaging;

public class KafkaConfigurationBlock
{
    public string BootstrapServers {  get; set; }
    public string Topic { get; set; }
}
