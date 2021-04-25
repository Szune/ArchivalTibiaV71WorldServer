using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class TurnRightPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            player.TurnRight();
            IoC.Game.CreatureTurned(player);
        }
    }
}