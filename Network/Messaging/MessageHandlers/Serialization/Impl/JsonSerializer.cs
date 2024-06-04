using System.Runtime.Serialization.Formatters.Binary;

namespace SeikenServer.Network.Messaging.MessageHandlers.Serialization.Impl;

public class JsonSerializer : IMessageSerializer
{
    public byte[] Serialize<T>(T data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
#pragma warning disable SYSLIB0011
            bf.Serialize(ms, data);
#pragma warning restore SYSLIB0011
            return ms.ToArray();
        }
    }

    public T Deserialize<T>(ArraySegment<byte> data)
    {
        return (T)System.Text.Json.JsonSerializer.Deserialize<T>(data);
    }
}