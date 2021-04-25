using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class TurnDownPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            player.TurnDown();
            IoC.Game.CreatureTurned(player);
        }
    }
}