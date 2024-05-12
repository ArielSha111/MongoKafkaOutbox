namespace MongoKafkaOutbox.Contracts
{
    public class OutboxConfigurationBlock
    {
        public string OutboxDbName { get; set; }
        public string OutboxCollectionName { get; set; }
        public string kafkaTopicName { get; set; }
        public string schemaRegistryConfigUrl { get; set; }
    }
}