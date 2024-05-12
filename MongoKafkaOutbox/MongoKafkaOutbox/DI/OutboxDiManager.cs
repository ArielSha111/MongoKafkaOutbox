using Confluent.SchemaRegistry;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Outbox;
using MongoKafkaOutbox.Serialization;
using MongoKafkaOutbox.Serialization.Avro;

namespace MongoKafkaOutbox.DI;

public static class OutboxDiManager
{
    public static void SetOutboxServicesWithDefaults(this IServiceCollection services,
        OutboxConfigurationBlock outboxConfigurationBlock)
    {
        services.AddSingleton(outboxConfigurationBlock);     

        services.AddSingleton(sp => {
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = outboxConfigurationBlock.schemaRegistryConfigUrl
            };
            return schemaRegistryConfig;
        });

        services.AddSingleton<ISerializationManager, DefaultSerializationManager>();
        services.AddSingleton<IAvroSerializationManager, DefaultAvroSerializationManager>();
        services.AddSingleton<IOutboxManager, DefaultOutboxManager>();
    }
}