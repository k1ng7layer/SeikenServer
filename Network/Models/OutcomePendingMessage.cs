using SeikenServer.Network.Utils;

namespace SeikenServer.Network.Models;

public readonly struct OutcomePendingMessage
{
    public readonly byte[] Payload;
    public readonly int ConnectionId;
    public readonly ESendMode SendMode;

    public OutcomePendingMessage(
        byte[] payload, 
        int connectionId,
        ESendMode sendMode)
    {
        Payload = payload;
        SendMode = sendMode;
        ConnectionId = connectionId;
    }
}