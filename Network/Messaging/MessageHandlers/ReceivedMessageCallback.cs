namespace SeikenServer.Network.Messaging.MessageHandlers;

public delegate void ReceivedMessageCallback<T>(int connId, T payload) where T : struct;
