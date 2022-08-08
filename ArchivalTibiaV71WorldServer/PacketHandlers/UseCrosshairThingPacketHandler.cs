using System;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    /// <summary>
    /// Handles using crosshair thing from player on:
    /// - other players
    /// - the ground
    /// - item in container
    /// </summary>
    public class UseCrosshairThingPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var sourcePos = reader.ReadPosition(); // X == u16.max if from self (player), Y = 64
            var fromPlayer = sourcePos.X == 0xFFFF && sourcePos.Y == 0x40;
            sourcePos = fromPlayer ? player.Position : sourcePos;

            var itemId = reader.ReadU16();
            var item = IoC.Items.GetById(itemId);
            if (!item.Flags.HasFlag(ItemFlags.CanUse))
                return;

            //var sourceZIndex = reader.ReadU8(); // not sure if actually zindex
            var containerSlot = reader.ReadU8(); // depends on source type (ground, backpack etc)

            var destPos = reader.ReadPosition(); // screen pos
            var destinationItemOnTopOrPlayer = reader.ReadU16(); // itemID on top or 99 if player
            var containerSlotAgain = reader.ReadU8(); // depends on source type (ground, backpack etc)

            // TODO: separate attack and exhausted (only difference is a rune attack triggers AttackWait as well as RuneWait)
            if (!player.CanAttack)
            {
                IoC.Game.PlayerExhausted(player);
                return;
            }

            var projectileId = IoC.Items.GetProjectileId(itemId);
            var magicId = IoC.Items.GetMagicId(itemId);

            if (destinationItemOnTopOrPlayer == 99)
            {
                // TODO: check if AoE rune
                var targetPlayer = IoC.Game.GetFirstPlayerOnTile(destPos);
                if (!targetPlayer.Equals(player))
                {
                    // only deal damage if not attacking self
                    IoC.Game.RuneTargetAttack(player, targetPlayer, projectileId, magicId);
                }
                else
                {
                    var c = IoC.Game.OnlinePlayers.Count;
                    for (int i = 0; i < c; i++)
                    {
                        var playerOnScreen = IoC.Game.OnlinePlayers[i];
                        if (!playerOnScreen.Connection.Connected)
                            continue;
                        if (Position.SameScreen(player.Position, IoC.Game.OnlinePlayers[i].Position))
                        {
                            playerOnScreen.Packets.Effects.Projectile(sourcePos, destPos, projectileId);
                            playerOnScreen.Packets.Effects.Magic(destPos, magicId);
                        }
                    }
                }
            }

            // TODO: handle other item uses (rope etc)


//             Console.WriteLine(
// $@"SourcePos: {sourcePos}
// ZIndex: {sourceZIndex}
// ItemId? {itemId}
// ItemId? {something}
//
// DestPos: {destPos}
// ZIndex: {destZIndex}");
        }
    }
}