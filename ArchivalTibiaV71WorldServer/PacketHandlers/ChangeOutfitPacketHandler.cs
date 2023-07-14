using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class ChangeOutfitPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var outfitId = reader.ReadU8();
            var head = reader.ReadU8();
            var body = reader.ReadU8();
            var legs = reader.ReadU8();
            var feet = reader.ReadU8();

            player.Outfit.Set((Outfits)outfitId, head, body, legs, feet);
            player.Packets.Creature.UpdateOutfit(player);
        }
    }
}
