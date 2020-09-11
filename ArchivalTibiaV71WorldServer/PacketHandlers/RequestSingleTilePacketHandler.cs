using System;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class RequestSingleTilePacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var pos = reader.ReadPosition();
            Console.WriteLine($"Player {player.Name} requested single tile: {pos}");
        }
    }
}