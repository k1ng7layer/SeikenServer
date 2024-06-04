using SeikenServer.Game;
using SeikenServer.Network;

namespace SeikenServer;

public class Bootstrap
{
    private readonly NetworkServer _server;
    private readonly GameModule _game;
    private bool _running;
    private DateTime _lastTime;
    private int _tickRate;

    public Bootstrap(NetworkServer server, GameModule game)
    {
        _server = server;
        _game = game;
    }
    
    public void Initialize()
    {
        _server.Start();
        _game.Initialize();
        _tickRate = 1000 / 20;
        _running = true;
    }
    
    public async Task Run()
    {
        try
        {
            while (_running)
            {
                _lastTime = DateTime.UtcNow;
            
                await Task.Delay(_tickRate);
            
                // var deltaTime = (float)(DateTime.UtcNow - _lastTime).TotalSeconds;
                //Console.WriteLine($"tick: {deltaTime}");
            
                _server.Tick();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
  
    }
}