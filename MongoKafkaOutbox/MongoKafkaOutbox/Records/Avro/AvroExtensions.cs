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



    //todo support all the next type:
    //Primitive Types:
    //null: Represents a null value.
    //boolean: Represents a boolean value.
    //int: Represents a 32-bit signed integer value.
    //long: Represents a 64-bit signed integer value.
    //float: Represents a single-precision 32-bit IEEE 754 floating-point value.
    //double: Represents a double-precision 64-bit IEEE 754 floating-point value.
    //bytes: Represents a sequence of binary bytes.
    //string: Represents a Unicode character sequence.

    //Complex Types:
    //record: Represents a complex type with named fields.
    //enum: Represents a symbolic enumeration.
    //array: Represents a homogeneous collection of items.
    //map: Represents a key-value map where keys are strings.
    //union: Represents a value that can be one of several types.

    //Fixed-Size Binary Data:
    //fixed: Represents a fixed-size binary data, with a specified number of bytes.
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
