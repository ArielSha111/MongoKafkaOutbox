using Avro.Generic;
using System.Reflection;

public static class GenericRecordExtensions//todo check performances and error handling
{
    public static T ConvertTo<T>(this GenericRecord genericRecord) where T : new()
    {
        T result = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (genericRecord.TryGetFieldValue(property.Name, out var value))
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                property.SetValue(result, value != null ? Convert.ChangeType(value, targetType) : null);
            }
        }

        return result;
    }

    private static bool TryGetFieldValue(this GenericRecord genericRecord, string fieldName, out object value)
    {
        if (genericRecord.Schema.Fields.Any(x => x.Name == fieldName))
        {
            value = genericRecord[fieldName];
            return true;
        }

        value = null;
        return false;
    }
}
