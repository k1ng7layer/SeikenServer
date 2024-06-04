using SeikenServer.Network.Helpers;
using SeikenServer.Network.Messaging.MessageHandlers.Serialization;

namespace SeikenServer.Network.Messaging.MessageHandlers.Impl;

public class MessageHandler : INetMessageHandler
{
    private readonly IMessageSerializer _messageSerializer;
    private readonly Dictionary<string, List<NetworkMessageHandler>> _registeredHandlersTable = new();

    public MessageHandler(IMessageSerializer messageSerializer)
    {
        _messageSerializer = messageSerializer;
    }
    
    public bool TryGetHandlerId<T>(out string id)
    {
        id = typeof(T).FullName.ToString();

        var hasId = _registeredHandlersTable.TryGetValue(id, out var handlerId);

        return hasId;
    }

    public void Subscribe<T>(ReceivedMessageCallback<T> handler) where T : struct
    {
        var id = typeof(T).Name;

        if (!_registeredHandlersTable.ContainsKey(id))
        {
            _registeredHandlersTable.Add(id, new List<NetworkMessageHandler>());
        }
            
        var networkHandler = CreateHandler(handler);
        _registeredHandlersTable[id].Add(networkHandler);
    }

    public void UnSubscribe<T>(ReceivedMessageCallback<T> callback) where T : struct
    {
        
    }

    public byte[] SerializeMessage<T>(T msg) where T : struct
    {
        return _messageSerializer.Serialize(msg);
    }

    public void CallHandler(int connectionId, string id, ArraySegment<byte> payload)
    {
        //Debug.Log($"CallHandler {id}");
        var hasHandler = _registeredHandlersTable.TryGetValue(id, out var handlers);
            
        if(!hasHandler)
            return;
            
        foreach (var handler in handlers)
        {
            handler?.Invoke(connectionId, _messageSerializer, payload);
        }
    }
    
    public void CallHandler(int connectionId, ArraySegment<byte> data)
    {
        var byteReader = new SegmentByteReader(data);
        var id = byteReader.ReadString(out _);
        var payloadLength = byteReader.ReadInt32();
        var payload = byteReader.ReadBytes(payloadLength);
       
        var hasHandler = _registeredHandlersTable.TryGetValue(id, out var handlers);
            
        if(!hasHandler)
            return;
            
        foreach (var handler in handlers)
        {
            handler?.Invoke(connectionId, _messageSerializer, payload);
        }
    }

    private NetworkMessageHandler CreateHandler<T>(ReceivedMessageCallback<T> handler) where T : struct
        => (connectionId, deserializer, payload) =>
        {
            var data = deserializer.Deserialize<T>(payload);

            handler?.Invoke(connectionId, data);
        };
}