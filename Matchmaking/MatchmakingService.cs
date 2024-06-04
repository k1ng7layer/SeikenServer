using SeikenServer.Network;

namespace SeikenServer.Matchmaking;      

public class MatchmakingService
{
    private readonly NetworkServer _networkServer;

    public MatchmakingService(NetworkServer networkServer)
    {
        _networkServer = networkServer;
    }

    public void Start()
    {
        //_networkServer.SubscribeNetMessage();
    }
}