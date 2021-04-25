using System;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
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

            var projectileId = IoC.Items.GetProjectileId(itemId);
            var magicId = IoC.Items.GetMagicId(itemId);
            
            var c = IoC.Game.OnlinePlayers.Count;
            for(int i = 0; i < c; i++)
            {
                if (!IoC.Game.OnlinePlayers[i].Connection.Connected)
                    continue;
                if (Position.SameScreen(player.Position, IoC.Game.OnlinePlayers[i].Position))
                {
                    IoC.Game.OnlinePlayers[i].Packets.Effects.Projectile(sourcePos, destPos, projectileId);
                    IoC.Game.OnlinePlayers[i].Packets.Effects.Magic(destPos, magicId);
                    Console.WriteLine($"Sent projectile to {IoC.Game.OnlinePlayers[i].Name}");
                }
            }

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