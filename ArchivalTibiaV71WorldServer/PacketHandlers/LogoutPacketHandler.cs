using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class LogoutPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            // handle the case where the player cannot logout due to some condition (pz etc)
            player.OnLogout();
        }
    }
}