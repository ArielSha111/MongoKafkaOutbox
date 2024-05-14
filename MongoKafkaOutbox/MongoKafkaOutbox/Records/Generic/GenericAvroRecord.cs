using Avro;
using Avro.Specific;

namespace MongoKafkaOutbox.Records;

public class GenericAvroRecord<T>
{
    private static readonly Schema _SCHEMA = Schema.Parse(typeof(T).GetAvroSchema());

    public virtual Schema Schema => _SCHEMA;

    public virtual object Get(int fieldPos)
    {
        var propertyInfo = typeof(T).GetAvroProperties()[fieldPos];
        return propertyInfo.GetValue(this);
    }

    public virtual void Put(int fieldPos, object fieldValue)
    {
        var propertyInfo = typeof(T).GetAvroProperties()[fieldPos];
        propertyInfo.SetValue(this, fieldValue);
    }
}