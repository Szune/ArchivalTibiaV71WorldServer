using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class CloseContainerPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var slot = reader.ReadU8();
            player.CloseContainer(slot);
        }
    }
}