﻿using Avro;
using Avro.Specific;

namespace AvroSchema;

public class SpecificAvroRecord<T> : ISpecificRecord where T : new()
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