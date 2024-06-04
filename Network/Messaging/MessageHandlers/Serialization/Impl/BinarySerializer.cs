using System.Runtime.Serialization.Formatters.Binary;
using SeikenServer.Game;

namespace SeikenServer.Network.Messaging.MessageHandlers.Serialization.Impl;

public class BinarySerializer : IMessageSerializer
{
    public T Deserialize<T>(ArraySegment<byte> data)
    {
        var stream = new MemoryStream(data.ToArray());
        var formatter = new BinaryFormatter();
        formatter.Binder = new CustomizedBinder<T>();
#pragma warning disable SYSLIB0011
        return (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
    }
    
    public byte[] Serialize<T>(T data)
    {
        var formatter = new BinaryFormatter();
        var stream = new MemoryStream();
#pragma warning disable SYSLIB0011
        formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011
            
        return stream.ToArray();
    }
}