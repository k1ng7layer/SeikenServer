using System.Net;
using SeikenServer;
using SeikenServer.Game;
using SeikenServer.Network;
using SeikenServer.Network.Messaging.MessageHandlers.Impl;
using SeikenServer.Network.Messaging.MessageHandlers.Serialization.Impl;
using SeikenServer.Network.Transport.Impl;


var transport = new LiteNetLibTransport(new IPEndPoint(IPAddress.Any, 5555));
var networkHandlersService = new MessageHandler(new BinarySerializer());

var server = new NetworkServer(transport, networkHandlersService);

var bootstrap = new Bootstrap(server, new GameModule(server));

bootstrap.Initialize();

await bootstrap.Run();