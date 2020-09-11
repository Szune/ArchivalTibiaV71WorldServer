using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class AttackTargetPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var targetId = reader.ReadU32();
            player.SetAttackTarget(targetId);
        }
    }
}