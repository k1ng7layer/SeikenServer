using SeikenServer.Network.Helpers;

namespace SeikenServer.Network.Messaging.Helpers;

public static class MessageHelper
{
    public static ECustomMessageType ReadMessageType(SegmentByteReader byteReader)
    {
        var type = byteReader.ReadByte();

        return (ECustomMessageType)type;
    }

    public static void ReadPlayerTurnInfo(SegmentByteReader segmentByteReader)
    {
        
    }
}

public struct PlayerTurnInfo
{
    public int CardId;
}