using Avro;
using Avro.Specific;

namespace ConsoleApp3;

public partial class Person : ISpecificRecord
{
    public static Schema _SCHEMA = Schema.Parse(
        "{" +
            "\"type\":\"record\"," +
            "\"name\":\"Person\"," +
            "\"namespace\":\"AvroConsole.Entity\"," +
            "\"fields\":" +
            "[" +
                "{\"name\":\"Age\",\"type\":\"int\"}," +
                "{\"name\":\"Name\",\"type\":\"string\"}" +
            "]" +
        "}");

    public int Age { get; set; }
    public string Name { get; set; }

    public virtual Schema Schema
    {
        get
        {
            return _SCHEMA;
        }
    }


    public virtual object Get(int fieldPos)
    {
        switch (fieldPos)
        {
            case 0: return Age;
            case 1: return Name;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
        };
    }

    public virtual void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0: Age = (int)fieldValue; break;
            case 1: Name = (string)fieldValue; break;
            default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
        };
    }
}
