using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class Effects
    {
        private readonly Player _player;

        public Effects(Player player)
        {
            _player = player;
        }
        public void Login(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.MagicEffect);
            builder.AddPosition(creature.Position);
            builder.AddU8(10); // effect id - 1
            builder.Send(_player);
        }

        public void Logout(Creature creature)
        {
            var builder = new PacketBuilder(Packets.SendToClient.MagicEffect);
            builder.AddPosition(creature.Position);
            builder.AddU8(2); // effect id - 1
            builder.Send(_player);
        }

        public void Projectile(Position source, Position destination)
        {
            var builder = new PacketBuilder(Packets.SendToClient.ProjectileEffect);
            builder.AddPosition(source);
            builder.AddPosition(destination);
            builder.AddU8(3); // projectile id - 1 because the client adds 1 to whatever we send
            builder.Send(_player);
            // 3 (4) == HMM projectile id
            // TODO: get correct projectile id
        }
    }
}