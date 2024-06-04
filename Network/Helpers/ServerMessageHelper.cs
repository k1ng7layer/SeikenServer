using SeikenServer.Network.Utils;

namespace SeikenServer.Network.Helpers;

public static class ServerMessageHelper
{
    internal static ENetworkMessageType ReadMessageType(ArraySegment<byte> data)
    {
        var messageTypeSpan = data.Slice(0, 2);
        var flagsInt = BitConverter.ToUInt16(messageTypeSpan);

        var result = ENetworkMessageType.None;

        if (Enum.TryParse(flagsInt.ToString(), out ENetworkMessageType messageType))
            result = messageType;
            
        return result;
    }

    // public static IPEndPoint GetPlayerEndpoint(byte[] data)
    // {
    //     // var ipStringSpan = new Span<byte>(data);
    //     // var ipStringSlice = ipStringSpan.Slice(6, 15);
    //     // var ipStringSpan = new Span<byte>(data);
    //     // var ipStringSlice = ipStringSpan.Slice(6, 15);
    // }
}