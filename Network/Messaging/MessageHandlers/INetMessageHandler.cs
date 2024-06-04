namespace SeikenServer.Network.Messaging.MessageHandlers;

public interface INetMessageHandler
{
    void Subscribe<T>(ReceivedMessageCallback<T> callback) where T : struct;
    void UnSubscribe<T>(ReceivedMessageCallback<T> callback) where T : struct;
    byte[] SerializeMessage<T>(T msg) where T : struct;
}