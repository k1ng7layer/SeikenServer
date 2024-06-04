using SeikenServer.Network.Messaging.MessageHandlers.Serialization;

namespace SeikenServer.Network.Messaging.MessageHandlers;

public delegate void NetworkMessageHandler(int connectionId, IMessageSerializer serializer, ArraySegment<byte> payload);