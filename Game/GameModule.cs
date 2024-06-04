using SeikenServer.Game.Models;
using SeikenServer.Network;
using SeikenServer.Network.Helpers;
using SeikenServer.Network.Messaging;
using SeikenServer.Network.Messaging.Helpers;
using SeikenServer.Network.Messaging.Messages.MessageTypes;
using SeikenServer.Network.Models;

namespace SeikenServer.Game;

public class GameModule : IDisposable
{
    private const int MaxInitiative = 15;
    
    private readonly NetworkServer _networkServer;
    private readonly Dictionary<int, Player> _players = new();

    public GameModule(NetworkServer networkServer)
    {
        _networkServer = networkServer;
    }
    
    public void Initialize()
    {
        _networkServer.ClientConnected += OnClientConnected;
        _networkServer.ClientDisconnected += OnClientDisconnected;
        _networkServer.SubscribeNetMessage<ChangeInitiativeBoostMessage>(OnInitiativeChangeRequest);
        _networkServer.SubscribeNetMessage<MakeTurnMessage>(OnMakeTurnRequest);
        _networkServer.UnSubscribeNetMessage<CharacterSelectMessage>(OnCharacterSelect);
    }
    
    public void Dispose()
    {
        _networkServer.ClientConnected -= OnClientConnected;
        _networkServer.ClientDisconnected -= OnClientDisconnected;
        _networkServer.UnSubscribeNetMessage<ChangeInitiativeBoostMessage>(OnInitiativeChangeRequest);
        _networkServer.UnSubscribeNetMessage<MakeTurnMessage>(OnMakeTurnRequest);
        _networkServer.UnSubscribeNetMessage<CharacterSelectMessage>(OnCharacterSelect);
    }

    private void OnClientConnected(NetClient client)
    {
        var player = new Player();
        player.InitiativeLeft = MaxInitiative;
        _players.Add(client.Id, player);
    }

    private void OnClientDisconnected(NetClient client)
    {
        _players.Remove(client.Id);
    }

    private void OnInitiativeChangeRequest(
        int playerId, 
        ChangeInitiativeBoostMessage msg)
    {
        var player = _players[playerId];
        
        var delta = msg.Up ? 1 : -1;
        
        player.SelectedInitiative += delta;
        player.SelectedInitiative = Math.Clamp(player.SelectedInitiative, 0, player.InitiativeLeft + player.SelectedInitiative);
        player.InitiativeLeft = MaxInitiative - player.SelectedInitiative;
        
        _networkServer.SendMessage(playerId, new SetInitiativeBoostMessage
        {
            Current = player.SelectedInitiative,
            Total = player.InitiativeLeft
        });

        Console.WriteLine($"Current:{player.SelectedInitiative}, total: {player.InitiativeLeft}, delta: {delta}");
    }

    private void OnMakeTurnRequest(int playerId, MakeTurnMessage msg)
    {
        var player = _players[playerId];
        
        player.Ready = true;
        player.SelectedCard = msg.CardId;
        
        if (!AllPlayersReady())
            return;

        CalculateTurnResult();
    }

    private void OnCharacterSelect(int playerId, CharacterSelectMessage msg)
    {
        var player = _players[playerId];
        
        player.CharacterId = msg.CharacterId;
    }
    
    private bool AllPlayersReady()
    {
        foreach (var playerKvp in _players)
        {
            if (!playerKvp.Value.Ready)
                return false;
        }

        return true;
    }

    private void CalculateTurnResult()
    {
        Console.WriteLine($"CalculateTurnResult");
    }
}