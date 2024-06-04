namespace SeikenServer.Network.Messaging.Messages.MessageTypes;

[Serializable]
public struct SetInitiativeBoostMessage
{
    public int Current;
    public int Total;
}