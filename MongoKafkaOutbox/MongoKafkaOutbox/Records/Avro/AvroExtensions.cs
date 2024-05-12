using System.Reflection;
using Avro;
using Avro.Generic;
using Avro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avro.Generic;
using Avro.Specific;

namespace MongoKafkaOutbox.Records.Avro;

public static class AvroExtensions
{
    public static string GetAvroSchema(this Type type)
    {
        if (!type.IsClass || type.IsArray || type.IsAbstract || type.IsInterface)
            throw new ArgumentException("Type must be a non-abstract class.");

        var properties = type.GetAvroProperties();

        var fields = properties.Where(p => p.PropertyType != typeof(Schema))
            .Select(p => $"{{\"name\":\"{p.Name}\",\"type\":\"{GetAvroType(p.PropertyType)}\"}}");

        var schema = $"{{\"type\":\"record\",\"name\":\"{type.Name}\",\"fields\":[{string.Join(",", fields)}]}}";
        return schema;
    }

    public static PropertyInfo[] GetAvroProperties(this Type type)
    {
        if (!type.IsClass || type.IsArray || type.IsAbstract || type.IsInterface)
            throw new ArgumentException("Type must be a non-abstract class.");

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public static string GetAvroSchema(this GenericRecord record)
    {
        return record.Schema.ToString();
    }

    private static string GetAvroType(Type type)
    {
        if (typeof(GenericRecord).IsAssignableFrom(type))
            return "record";

        return type switch
        {
            Type t when t == typeof(int) => "int",
            Type t when t == typeof(long) => "long",
            Type t when t == typeof(float) => "float",
            Type t when t == typeof(double) => "double",
            Type t when t == typeof(bool) => "boolean",
            Type t when t == typeof(string) => "string",
            Type t when t == typeof(byte[]) => "bytes",
            Type t when t.IsEnum => $"enum {{\"type\":\"enum\",\"name\":\"{t.Name}\",\"symbols\":[{string.Join(",", t.GetEnumNames().Select(enumName => $"\"{enumName}\""))}]}}",
            Type t when t.IsArray => $"{{\"type\":\"array\",\"items\":\"{GetAvroType(t.GetElementType())}\"}}",
            Type t when t == typeof(Dictionary<string, object>) => "map",
            Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>) => $"{{\"type\":\"map\",\"values\":\"{GetAvroType(t.GetGenericArguments()[1])}\"}}",
            Type t when t.IsClass && t != typeof(string) => $"{{\"type\":\"record\",\"name\":\"{t.Name}\",\"fields\":{GetAvroRecordFields(t)}}}",
            _ => throw new ArgumentException($"Unsupported type: {type.Name}")
        };
    }

    private static string GetAvroRecordFields(Type type)
    {
        var properties = type.GetAvroProperties();

        var fields = properties.Where(p => p.PropertyType != typeof(Schema))
            .Select(p => $"{{\"name\":\"{p.Name}\",\"type\":\"{GetAvroType(p.PropertyType)}\"}}");

        return $"[{string.Join(",", fields)}]";
    }
}
