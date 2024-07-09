namespace MongoKafkaOutbox.Misc
{
    public interface IAvroSerializer<T>
    {
        Task<byte[]> Serialize(string topic, T data, bool isKey);
    }

}
