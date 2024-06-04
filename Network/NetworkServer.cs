using SeikenServer.Network.Helpers;
using SeikenServer.Network.Messaging;
using SeikenServer.Network.Messaging.MessageHandlers;
using SeikenServer.Network.Messaging.MessageHandlers.Impl;
using SeikenServer.Network.Models;
using SeikenServer.Network.Utils;

namespace SeikenServer.Network;

public class NetworkServer : IDisposable
{
    private readonly Queue<OutcomePendingMessage> _outcomePendingMessages = new();
    private readonly Queue<IncomePendingMessage> _incomePendingMessages = new();
    private readonly Transport.Transport _transport;
    private readonly Dictionary<int, NetClient> _netClients = new();
    private readonly MessageHandler _messageHandler;
    private bool _running;

    public NetworkServer(
        Transport.Transport transport, 
        MessageHandler messageHandler
    )
    {
        _transport = transport;
        _messageHandler = messageHandler;
    }

    public event Action<NetClient> ClientConnected; 
    public event Action<NetClient> ClientReconnected; 
    public event Action<NetClient> ClientDisconnected; 
    public event Action<int, ArraySegment<byte>> OnClientCustomMessageReceived;
    public event Action<int, SegmentByteReader> OnClientCustomMessageReceivedReader;

    public IReadOnlyDictionary<int, NetClient> NetClients => _netClients;

    public void Start()
    {
        _transport.Start();
        _running = true;
        _transport.PeerConnected += OnPeerConnected;
        _transport.DataReceived += OnRawData;
        _transport.PeerDisconnected += OnPeerDisconnected;
    }
    
    public void Dispose()
    {
        _transport.Stop();
        _transport.PeerConnected -= OnPeerConnected;
        _transport.DataReceived -= OnRawData;
        _transport.PeerDisconnected -= OnPeerDisconnected;
    }

    public void Stop()
    {
        _transport.Stop();
    }

    public void Tick()
    {
        if (!_running)
            return;
        
        _transport.Tick();
        
        SendQueue();
        ReceiveQueue();
    }
    
    public void SendRawMessage(int connectionId, byte[] data, ESendMode sendMode)
    {
        var message = new OutcomePendingMessage(data, connectionId, sendMode);
            
        _outcomePendingMessages.Enqueue(message);
    }
    
    public void SendMessage<T>(int clientId, T message) where T : struct
    {
        var handlerId = typeof(T).Name;

        var payload = _messageHandler.SerializeMessage(message);
            
        var byteWriter = new ByteWriter();
            
        byteWriter.AddUshort((ushort)ENetworkMessageType.NetworkMessage);
        byteWriter.AddString(handlerId);
        byteWriter.AddInt32(payload.Length);
        byteWriter.AddBytes(payload);

        SendRawMessage(clientId, byteWriter.Data, ESendMode.Reliable);
    }
    
    public void SendRawMessage(byte[] data, ESendMode sendMode)
    {
        foreach (var client in _netClients)
        {
            var message = new OutcomePendingMessage(data, client.Key, sendMode);
            
            _outcomePendingMessages.Enqueue(message);
        }
    }

    public void SubscribeNetMessage<T>(ReceivedMessageCallback<T> callBack) where T : struct
    {
        _messageHandler.Subscribe(callBack);
    }

    public void UnSubscribeNetMessage<T>(ReceivedMessageCallback<T> callBack) where T : struct
    {
        _messageHandler.UnSubscribe(callBack);
    }

    private void OnPeerConnected(int connectionId)
    {
        var id = IdGenerator.Next();
        var client = new NetClient(id);
        
        _netClients.Add(connectionId, client);
    }

    private void OnPeerDisconnected(int connectionId)
    {
        _netClients.Remove(connectionId);
    }
    
    private void OnRawData(int connId, ArraySegment<byte> data)
    {
        Console.WriteLine($"OnRawData connId: {connId}, length: {data.Count}");
        var msg = new IncomePendingMessage(data, connId);
            
        _incomePendingMessages.Enqueue(msg);
    }
    
    private void SendQueue()
    {
        while (_outcomePendingMessages.Count > 0)
        {
            var msg = _outcomePendingMessages.Dequeue();
                
            _transport.Send(msg.ConnectionId, msg.Payload, msg.SendMode);
        }
    }
    
    private void ReceiveQueue()
    {
        while (_incomePendingMessages.Count > 0)
        {
            var msg = _incomePendingMessages.Dequeue();

            HandleIncomeMsg(msg.ConnId, msg.Payload);
        }
    }
    
    private void HandleIncomeMsg(int connectionId, ArraySegment<byte> data)
    {
        var messageType = ServerMessageHelper.ReadMessageType(data);
        var byteReader = new SegmentByteReader(data, 2);
        switch (messageType)
        {
            case ENetworkMessageType.ConnectionRequest:
                HandleConnectionRequest(connectionId, data);
                break;
            case ENetworkMessageType.ClientDisconnected:
            {
                var clientId = byteReader.ReadInt32();
                var client = _netClients[clientId];
                ClientDisconnected?.Invoke(client);
            }
                break;
            case ENetworkMessageType.ClientConnected:
            {
                var clientId = byteReader.ReadInt32();
                var client = _netClients[clientId];
                ClientConnected?.Invoke(client);
            }
                break;
            case ENetworkMessageType.ClientReconnected:
            {
                var clientId = byteReader.ReadInt32();
                var client = _netClients[clientId];
                ClientReconnected?.Invoke(client);
            }
                break;
            case ENetworkMessageType.ClientReady:
                break;
            case ENetworkMessageType.AuthenticationResult:
                break;
            case ENetworkMessageType.NetworkMessage:
            {
                var id = byteReader.ReadString(out _);
                var payloadLength = byteReader.ReadInt32();
                var payload = byteReader.ReadBytes(payloadLength);
                _messageHandler.CallHandler(connectionId, id, payload);
            }
                break;
            case ENetworkMessageType.ClientAliveCheck:
                break;
            case ENetworkMessageType.ServerAliveCheck:
                break;
            case ENetworkMessageType.Ping:
                break;
            case ENetworkMessageType.Sync:
                break;
            case ENetworkMessageType.None:
                break;
            case ENetworkMessageType.Custom:
            {
                var payloadSize = byteReader.ReadInt32();
                var payload = byteReader.ReadBytes(payloadSize);
                
                OnClientCustomMessageReceived?.Invoke(connectionId, payload);
                
                //TODO: create pool
                OnClientCustomMessageReceivedReader?.Invoke(connectionId, new SegmentByteReader(payload));
            }
                break;
        }
    }

    private void HandleConnectionRequest(int connId, ArraySegment<byte> data)
    {
        //var connectResult = ConnectionApproveCallback(id, data.Slice(2, data.Count - 2));
        var connectResult = EConnectionResult.Success;
        
        var byteWriter = new ByteWriter();
        byteWriter.AddUshort((ushort)ENetworkMessageType.AuthenticationResult);
        byteWriter.AddUshort((ushort)connectResult);
        byteWriter.AddInt32(connId);
        byteWriter.AddString("Success");

        SendRawMessage(connId, byteWriter.Data, ESendMode.Reliable);
        
        //_netClients.Add(connId, _transport.);
        ClientConnected?.Invoke(NetClients[connId]);
    }
    
}