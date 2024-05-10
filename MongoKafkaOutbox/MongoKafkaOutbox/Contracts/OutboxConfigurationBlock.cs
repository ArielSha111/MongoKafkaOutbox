namespace MongoKafkaOutbox.Contracts
{
    public class OutboxConfigurationBlock
    {
        public string MongoConnectionString { get; set; }
        public string MongoDBName { get; set; }
        public IDictionary<string, string> MongoCollectionNames { get; set; }
        public string OutboxCollectionName { get; set; }

        public string kafkaTopicName { get; set; }
        public string schemaRegistryConfigUrl { get; set; }
    }
}