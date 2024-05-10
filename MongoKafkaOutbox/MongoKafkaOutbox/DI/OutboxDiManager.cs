using Confluent.SchemaRegistry;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoKafkaOutbox.Contracts;
using MongoKafkaOutbox.Outbox.Default;
using MongoKafkaOutbox.Serialization;
using MongoKafkaOutbox.Serialization.Avro;

namespace MongoKafkaOutbox.DI;

public static class OutboxDiManager
{
    public static void SetOutboxServicesWithDefaults<TCollectionType>(this IServiceCollection services,
        OutboxConfigurationBlock outboxConfigurationBlock)
    {
        services.AddSingleton(outboxConfigurationBlock);

        services.AddSingleton<IMongoClient>(sp =>
        {
            var client = new MongoClient(outboxConfigurationBlock.MongoConnectionString);
            return client;
        });

        services.AddSingleton(sp => {
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = outboxConfigurationBlock.schemaRegistryConfigUrl
            };
            return schemaRegistryConfig;
        });

        services.AddSingleton<ISerializationManager, DefaultSerializationManager>();
        services.AddSingleton<IAvroSerializationManager, DefaultAvroSerializationManager>();
        services.AddSingleton<IGenericOutboxManager<TCollectionType>, GenericOutboxManager<TCollectionType>>();
    }
}