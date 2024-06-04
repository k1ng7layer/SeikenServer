namespace SeikenServer.Network.Models;

public struct IncomePendingMessage
{
    public readonly  ArraySegment<byte> Payload;
    public readonly int ConnId;

    public IncomePendingMessage(
        ArraySegment<byte> payload, 
        int connId)
    {
        Payload = payload;
        ConnId = connId;
    }
}