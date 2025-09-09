# Block Sumo FFA

A Block Sumo FFA Minecraft game mode written in C# using
[Minecraft Dotnet](https://github.com/CoPokBl/MinecraftDotnet).

You can run the game simply by executing the `Test` project. Alternatively
you can integrate it into another project by including it as a dependency
and doing something like this:

```csharp
ManagedMinecraftServer server = ManagedMinecraftServer.NewBasic();

ITerrain terrain = ...;  // Load or generate terrain here

BlockSumoFfaConfig config = new(terrain, spawn, respawnTime);
BlockSumoFfaGame game = new(config, server);

World world = game.Initialise();
server.Events.AddListener<PlayerPreLoginEvent>(e => {
    e.World = world;
});

server.Start();
await server.ListenTcp(25565);
```

See the `Test` project for a complete example.
