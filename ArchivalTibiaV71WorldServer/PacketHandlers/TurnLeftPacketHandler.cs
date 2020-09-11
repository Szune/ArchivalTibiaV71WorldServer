using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class TurnLeftPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            player.TurnLeft();
            Game.Instance.CreatureTurned(player);
        }
    }
}