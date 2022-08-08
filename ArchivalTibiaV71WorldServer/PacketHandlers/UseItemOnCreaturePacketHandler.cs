using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class UseItemOnCreaturePacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var pos = reader.ReadPosition();
            var itemId = reader.ReadU16();
            var item = IoC.Items.GetById(itemId);
            if (!item.Flags.HasFlag(ItemFlags.CanUse))
                return;

            var notSure = reader.ReadU8();

            var creatureId = reader.ReadU32();
            var creature = IoC.Game.GetCreatureById(creatureId);
            if (creature.IsNone)
                return;

            // TODO: separate attack and exhausted (only difference is a rune attack triggers AttackWait as well as RuneWait)
            if (!player.CanAttack)
            {
                IoC.Game.PlayerExhausted(player);
                return;
            }

            var projectileId = IoC.Items.GetProjectileId(itemId);
            var magicId = IoC.Items.GetMagicId(itemId);
            
            // TODO: check if AoE rune
            IoC.Game.RuneTargetAttack(player, creature, projectileId, magicId);
        }
    }
}