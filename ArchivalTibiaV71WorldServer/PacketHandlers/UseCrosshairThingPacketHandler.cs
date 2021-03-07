﻿using System;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

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
            var sourceZIndex = reader.ReadU8(); // not sure if actually zindex

            var destPos = reader.ReadPosition(); // screen pos
            var destinationItemOnTopOrPlayer = reader.ReadU16(); // itemID on top or 99 if player
            var destZIndex = reader.ReadU8();
            destPos += sourcePos;
            
            var c = Game.Instance.OnlinePlayers.Count;
            for(int i = 0; i < c; i++)
            {
                if (!Game.Instance.OnlinePlayers[i].Connection.Connected)
                    continue;
                if (Position.SameScreen(player.Position, Game.Instance.OnlinePlayers[i].Position))
                {
                    Game.Instance.OnlinePlayers[i].Packets.Effects.Projectile(sourcePos, destPos);
                    Console.WriteLine($"Sent projectile to {Game.Instance.OnlinePlayers[i].Name}");
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