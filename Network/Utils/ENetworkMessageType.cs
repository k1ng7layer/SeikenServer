namespace SeikenServer.Network.Utils;

public enum ENetworkMessageType
{
    ConnectionRequest,
    ClientDisconnected,
    ClientConnected,
    ClientReconnected,
    ClientReady,
    AuthenticationResult,
    NetworkMessage,
    ClientAliveCheck,
    ServerAliveCheck,
    Ping,
    Sync,
    Custom,
    None
}