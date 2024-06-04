using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using SeikenServer.Network.Utils;

namespace SeikenServer.Network.Transport.Impl;

public class LiteNetLibTransport : Transport, INetEventListener
{
    private readonly NetDataWriter _dataWriter;
    private readonly IPEndPoint _localEp;
    private readonly NetManager _server;
    private readonly Dictionary<int, NetPeer> _connectedPeers = new();
    
    public LiteNetLibTransport(IPEndPoint localEp)
    {
        _localEp = localEp;
        _server = new NetManager(this);
        _dataWriter = new NetDataWriter();
    }
    
    public override event Action<int, ArraySegment<byte>> DataReceived;
    public override event Action<int> PeerConnected;
    public override event Action<int> PeerDisconnected;

    public override void Start()
    {
        _server.Start(_localEp.Port);
    }

    public override void Stop()
    {
        _server.Stop();
    }

    public override void Tick()
    {
        _server.PollEvents();
    }

    public override void Send(IPEndPoint endPoint, ArraySegment<byte> data, ESendMode sendMode)
    {
        _connectedPeers.TryGetValue(endPoint.GetHashCode(), out var peer);
        
        var msgLength = BitConverter.GetBytes(data.Count);
        var buffer = new byte[msgLength.Length + data.Count];
            
        Buffer.BlockCopy(msgLength, 0, buffer, 0, msgLength.Length);
        Buffer.BlockCopy(data.ToArray(), 0, buffer, 4, data.Count);
        
        peer.Send(_dataWriter, sendMode == ESendMode.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
    }

    public override void Send(int connectionHash, ArraySegment<byte> data, ESendMode sendMode)
    {
        _connectedPeers.TryGetValue(connectionHash, out var peer);
        
        var msgLength = BitConverter.GetBytes(data.Count);
        var buffer = new byte[msgLength.Length + data.Count];
            
        Buffer.BlockCopy(msgLength, 0, buffer, 0, msgLength.Length);
        Buffer.BlockCopy(data.ToArray(), 0, buffer, 4, data.Count);
        
        peer.Send(buffer, sendMode == ESendMode.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
    }

    public override void Dispose()
    {
        _server.Stop();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"OnPeerConnected, address: {peer.Address}, port: {peer.Port}");
       
        _connectedPeers.Add(peer.Id, peer);
        
        PeerConnected?.Invoke(peer.Id);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"OnPeerDisconnected, address: {peer.Address}, port: {peer.Port}");
       
        _connectedPeers.Remove(peer.Id);
        PeerDisconnected?.Invoke(peer.Id);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.WriteLine($"OnNetworkError, socketError: {socketError}");
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var mgLengthBytes = new Span<byte>(reader.RawData, 4, 4);
        var msgLength = BitConverter.ToUInt16(mgLengthBytes);
        var msg = new Span<byte>(reader.RawData, 8, msgLength);
        Console.WriteLine($"OnNetworkReceive, address: {peer.Address}, port: {peer.Port}, data length: {reader.RawData.Length}, msg length: {BitConverter.ToUInt16(mgLengthBytes)}");
    
        DataReceived?.Invoke(peer.Id, msg.ToArray());
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Console.WriteLine($"OnNetworkReceiveUnconnected, address: {remoteEndPoint}");
        
        DataReceived?.Invoke(-1, reader.RawData);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        //Console.WriteLine($"OnNetworkLatencyUpdate, Peer: {peer.Address}, port: {peer.Port}");
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Console.WriteLine($"OnConnectionRequest, address: {request.RemoteEndPoint}");
        
        request.Accept();
    }
}