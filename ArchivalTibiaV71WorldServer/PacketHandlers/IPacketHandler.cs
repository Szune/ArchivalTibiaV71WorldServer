using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public interface IPacketHandler
    {
        void Handle(Player player, PacketReader reader);
    }
}