namespace SeikenServer.Network.Messaging;

public enum ECustomMessageType : ushort
{
    CharacterInfo,
    PlayerTurn,
    InitiativeAdd,
    InitiativeRemove,
    PlayerReady
}