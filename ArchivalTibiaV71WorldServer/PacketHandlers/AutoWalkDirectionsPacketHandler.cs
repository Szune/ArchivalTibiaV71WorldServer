using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class AutoWalkDirectionsPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var count = reader.ReadU8();
            player.ClearAutoWalk();
            for (int i = 0; i < count; i++)
            {
                player.AddAutoWalk((Directions)reader.ReadU8());
            }
        }
    }
}