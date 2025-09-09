using ManagedServer;
using ManagedServer.Entities.Types;
using ManagedServer.Events;
using ManagedServer.Features;
using ManagedServer.Features.Impl;
using ManagedServer.Viewables;
using ManagedServer.Worlds;
using Minecraft;
using Minecraft.Data.Blocks;
using Minecraft.Data.Components.Types;
using Minecraft.Data.Generated;
using Minecraft.Packets.Play.ClientBound;
using Minecraft.Schemas;
using Minecraft.Schemas.Items;
using Minecraft.Text;

namespace BlockSumoFFA;

public class BlockSumoFfaGame(BlockSumoFfaConfig config, ManagedMinecraftServer server) {
    public World World = null!;
    
    private static readonly IBlock[] Blocks = [
        Block.WhiteConcretePowder,
        Block.OrangeConcretePowder,
        Block.MagentaConcretePowder,
        Block.LightBlueConcretePowder,  
        Block.YellowConcretePowder,
        Block.LimeConcretePowder,
        Block.PinkConcretePowder,
        Block.GrayConcretePowder,
        Block.LightGrayConcretePowder,
        Block.CyanConcretePowder,
        Block.PurpleConcretePowder,
        Block.BlueConcretePowder,
        Block.BrownConcretePowder,
        Block.GreenConcretePowder,
        Block.RedConcretePowder,
        Block.BlackConcretePowder
    ];
    
    private static readonly ItemStack BlockItem = new ItemStack(Item.BambooBlock, 64)
        .With(DataComponent.CustomName, TextComponent.Text("Michael's Bamboo").WithColor(TextColor.Green).WithBold().WithItalic(false))
        .With(DataComponent.Rarity, ItemRarity.Epic)
        .With(DataComponent.TooltipDisplay, new TooltipDisplayComponent.Info(true));
    
    public World Initialise() {
        World = server.CreateWorld(config.Map);
        World.AddFeature(new SimpleCombatFeature(500));

        World.Events.AddListener<PlayerEnteringWorldEvent>(e => {
            Respawn(e.Player);
            e.Player.Inventory.AddItem(BlockItem);
        });

        World.Events.AddListener<EntityMoveEvent>(e => {
            if (e.Entity is not PlayerEntity player) {
                return;
            }

            if (player.Position.Y <= config.DeathY && player.GameMode != GameMode.Spectator) {
                Die(player);
            }
        });

        World.Events.AddListener<PlayerBreakBlockEvent>(e => {
            e.Cancelled = true;
        });

        World.Events.AddListener<PlayerPlaceBlockEvent>(e => {
            e.Cancelled = false;
            e.Block = Blocks[Random.Shared.Next(Blocks.Length)];
            e.ConsumeItem = false;

            if (e.Position.Y > config.HeightLimit) {
                e.Cancelled = true;
                return;
            }
            
            AtomicCounter count = new(-1);
            int breakingEntity = Random.Shared.Next();
            server.Scheduler.ScheduleRepeatingTask(TimeSpan.FromSeconds(config.BlockTime/9), () => {
                count.Increment();
                if (count.Value == 9) {
                    e.World.SendPacket(new ClientBoundSetBlockDestroyStagePacket {
                        EntityId = breakingEntity,
                        Block = e.Position,
                        Stage = 16
                    });
                    e.World.SetBlock(e.Position, Block.Air);
                    return false;
                }
                
                e.World.SendPacket(new ClientBoundSetBlockDestroyStagePacket {
                    EntityId = breakingEntity,
                    Block = e.Position,
                    Stage = (byte)count.Value
                });
                return true;
            });
        }, true);
        
        return World;
    }

    public void Die(PlayerEntity player) {
        player.GameMode = GameMode.Spectator;
        player.Connection.SendTitle(TextComponent.FromLegacyString("&c&lNoob"), TextComponent.Empty());

        server.Scheduler.ScheduleTask(TimeSpan.FromSeconds(2), () => {
            Respawn(player);
        });
                
        TextComponent msg = $"{player.Name} was killed";
        World.StrikeLightning(player.Position);
        World.SendMessage(msg);
    }

    public void Respawn(PlayerEntity player) {
        player.Teleport(config.Spawn);
        player.GameMode = GameMode.Survival;
    }
}
