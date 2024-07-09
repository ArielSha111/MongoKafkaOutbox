using Avro;
using System.Reflection;


namespace AvroSchema;

public static class SpecificRecordExtensions
{
    public static string GetAvroSchema(this Type type)
    {
        if (!type.IsClass || type.IsArray || type.IsAbstract || type.IsInterface)
            throw new ArgumentException("Type must be a non-abstract class.");

        var properties = type.GetAvroProperties();

        var fields = properties.Where(p => p.PropertyType != typeof(Schema))
            .Select(p => $"{{\"name\":\"{p.Name}\",\"type\":{GetAvroType(p.PropertyType)}}}");

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
            var t when t == typeof(int) => "\"int\"",
            var t when t == typeof(long) => "\"long\"",
            var t when t == typeof(float) => "\"float\"",
            var t when t == typeof(double) => "\"double\"",
            var t when t == typeof(bool) => "\"boolean\"",
            var t when t == typeof(string) => "\"string\"",
            var t when t == typeof(byte[]) => "\"bytes\"",
            var t when t.IsEnum => $"{{\"type\":\"enum\",\"name\":\"{t.Name}\",\"symbols\":[{string.Join(",", t.GetEnumNames().Select(enumName => $"\"{enumName}\""))}]}}",
            var t when t.IsArray => $"{{\"type\":\"array\",\"items\":{GetAvroType(t.GetElementType())}}}",
            var t when t == typeof(Dictionary<string, object>) => "\"map\"",
            var t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>) => $"{{\"type\":\"map\",\"values\":{GetAvroType(t.GetGenericArguments()[1])}}}",
            var t when t.IsClass && t != typeof(string) => $"{{\"type\":\"record\",\"name\":\"{t.Name}\",\"fields\":{GetAvroRecordFields(t)}}}",
            _ => throw new ArgumentException($"Unsupported type: {type.Name}")
        };
    }

    private static string GetAvroRecordFields(Type type)
    {
        var properties = type.GetAvroProperties();

        var fields = properties.Where(p => p.PropertyType != typeof(Schema))
            .Select(p => $"{{\"name\":\"{p.Name}\",\"type\":{GetAvroType(p.PropertyType)}}}");

        return $"[{string.Join(",", fields)}]";
    }
}