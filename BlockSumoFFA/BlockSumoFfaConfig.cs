using Minecraft.Implementations.Server.Terrain;
using Minecraft.Schemas.Vec;

namespace BlockSumoFFA;

public record BlockSumoFfaConfig(ITerrainProvider Map, Vec3<double> Spawn, int RespawnTime, float BlockTime = 5, int DeathY = -10, int HeightLimit = 1);
