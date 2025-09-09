using BlockSumoFFA;
using ManagedServer;
using ManagedServer.Events;
using ManagedServer.Worlds;
using Minecraft.Schemas.Vec;
using Test;

ManagedMinecraftServer server = ManagedMinecraftServer.NewBasic();

BlockSumoFfaConfig config = new(new BlockSumoMapProvider(12), new Vec3<double>(0, 6, 0), 5);
BlockSumoFfaGame game = new(config, server);

World world = game.Initialise();
server.Events.AddListener<PlayerPreLoginEvent>(e => {
    e.World = world;
});

server.Start();

Console.WriteLine("Started server.");
await server.ListenTcp(25565);
