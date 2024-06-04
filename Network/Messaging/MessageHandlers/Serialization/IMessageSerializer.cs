namespace SeikenServer.Network.Messaging.MessageHandlers.Serialization;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T data);
    T Deserialize<T>(ArraySegment<byte> data);
}