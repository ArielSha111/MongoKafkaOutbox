using Confluent.SchemaRegistry;
using Microsoft.Extensions.DependencyInjection;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Outbox;
using MongoKafkaOutbox.Serialization;

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

        services.AddSingleton<IAvroSerializationManager, AvroSerializationManager>();
        services.AddSingleton<IAvroOutboxManager, AvroOutboxManager>();
    }
}