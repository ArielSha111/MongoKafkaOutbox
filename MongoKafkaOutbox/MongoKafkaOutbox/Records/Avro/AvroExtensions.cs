using System.Reflection;
using Avro;

namespace MongoKafkaOutbox.Records.Avro;

public static class AvroExtensions
{
    public static string GetAvroSchema(this Type type)
    {
        if (!type.IsClass || type.IsArray || type.IsAbstract || type.IsInterface)
            throw new ArgumentException("Type must be a non-abstract class.");

        var properties = type.GetAvroProperties();
        var fields = properties.Where(p => p.PropertyType != typeof(Schema)).Select(p => $"{{\"name\":\"{p.Name}\",\"type\":\"{GetAvroType(p.PropertyType)}\"}}");
        var schema = $"{{\"type\":\"record\",\"name\":\"{type.Name}\",\"fields\":[{string.Join(",", fields)}]}}";
        return schema;
    }

    public static PropertyInfo[] GetAvroProperties(this Type type)
    {
        if (!type.IsClass || type.IsArray || type.IsAbstract || type.IsInterface)
            throw new ArgumentException("Type must be a non-abstract class.");

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    private static string GetAvroType(Type type)
    {
        return type switch
        {
            Type t when t == typeof(int) => "int",
            Type t when t == typeof(long) => "long",
            Type t when t == typeof(float) => "float",
            Type t when t == typeof(double) => "double",
            Type t when t == typeof(bool) => "boolean",
            Type t when t == typeof(string) => "string",
            _ => throw new ArgumentException($"Unsupported type: {type.Name}")
        };
    }
}
