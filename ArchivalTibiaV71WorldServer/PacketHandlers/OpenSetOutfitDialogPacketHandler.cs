using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class OpenSetOutfitDialogPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            player.Packets.SetOutfitDialog();
        }
    }
}
