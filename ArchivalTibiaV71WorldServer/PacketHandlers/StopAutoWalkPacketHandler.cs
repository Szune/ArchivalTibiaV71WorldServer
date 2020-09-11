using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class StopAutoWalkPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            player.ClearAutoWalk();
            player.Packets.ResetAutoWalk();
        }
    }
}