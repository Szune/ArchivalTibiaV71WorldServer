using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class MoveDownPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            if (!player.CanWalk)
            {
                player.AddOrReplaceAutoWalk(Directions.South);
                return;
            }
            player.Packets.MoveSouth();
        }
    }
}