using Avro;
using Avro.Specific;

namespace MongoKafkaOutbox2.Records.Avro;

public class DynamicAvroRecord<T> : ISpecificRecord where T : new()
{
    private static readonly Schema _schema = Schema.Parse(typeof(T).GetAvroSchema());

    public virtual Schema Schema => _schema;

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
