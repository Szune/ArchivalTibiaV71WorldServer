using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class AttackChaseModePacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var attackMode = (AttackMode) reader.ReadU8();
            var chaseMode = (ChaseMode) reader.ReadU8();
            player.SetAttackMode(attackMode);
            player.SetChaseMode(chaseMode);
        }
    }
}