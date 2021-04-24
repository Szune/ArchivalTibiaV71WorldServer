using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class CreatureHelper
    {
        private readonly Player _player;

        public CreatureHelper(Player player)
        {
            _player = player;
        }
        public void UpdateLight(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.UpdateCreatureLight);
            builder.AddU32(creature.Id);
            builder.AddU8(creature.LightColor);
            builder.AddU8(creature.LightLevel);
            builder.Send(_player);
        }

        public void UpdateOutfit(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.UpdateCreatureOutfit);
            builder.AddU32(creature.Id);
            builder.AddU8(creature.Outfit.Id);
            if (creature.Outfit.Id != 0)
            {
                builder.AddU8(creature.Outfit.Head);
                builder.AddU8(creature.Outfit.Body);
                builder.AddU8(creature.Outfit.Legs);
                builder.AddU8(creature.Outfit.Feet);
            }
            else
            {
                // invisible
                builder.AddU16(0);
            }
            builder.Send(_player);
        }
        
        public void UpdateHealth(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.UpdateCreatureHealthPercent);
            builder.AddU32(creature.Id);
            builder.AddU8(creature.PercentHitpoints);
            builder.Send(_player);
        }
        
        public void UpdateSpeed(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.UpdateCreatureSpeed);
            builder.AddU32(creature.Id);
            builder.AddU16(creature.GetSpeed());
            builder.Send(_player);
        }
    }
}