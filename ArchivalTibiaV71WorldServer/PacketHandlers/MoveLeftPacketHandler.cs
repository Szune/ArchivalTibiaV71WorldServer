using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class MoveLeftPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            if (!player.CanWalk)
            {
                player.AddOrReplaceAutoWalk(Directions.West);
                return;
            }
            player.Packets.MoveWest();
        }
    }
}