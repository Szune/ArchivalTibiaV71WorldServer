using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class PacketHelper
    {
        private readonly Player _player;

        public CreatureHelper Creature;
        public Effects Effects;
        public Equip Equip;
        public Message Message;
        public MapHelper Map;
        public MessageBoxes MessageBoxes;
        public Containers Containers;

        public PacketHelper(Player player)
        {
            _player = player;
            Creature = new CreatureHelper(player);
            Effects = new Effects(player);
            Equip = new Equip(player);
            Message = new Message(player);
            Map = new MapHelper(player);
            MessageBoxes = new MessageBoxes(player);
            Containers = new Containers(player);
        }


        public void LoginSuccess()
        {
            _player.ZIndex = Game.Instance.GetCreatureZIndexLogin(_player, _player.Position);
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.LoginSuccess);
            builder.AddU32(_player.Id);
            builder.AddU16(GameClient.Beat);
            Map.AddFullMapToBuilder(builder);
            builder.Send(_player);

            WorldLight();
            Stats();
            Skills();
            StatusIcons();
            Equipment();
            Effects.Login(_player);

            PlayerAppear();
        }

        private void PlayerAppear()
        {
            var c = Game.Instance.OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (Game.Instance.OnlinePlayers[i].Id == _player.Id) continue;
                if (!Game.Instance.OnlinePlayers[i].Connection.Connected) continue;
                if (!Position.SameScreen(_player.Position, Game.Instance.OnlinePlayers[i].Position)) continue;
                Game.Instance.OnlinePlayers[i].Packets.Map.CreatureAppear(_player);
                Game.Instance.OnlinePlayers[i].Packets.Effects.Login(_player);
            }
        }

        public void Skills()
        {
            var builder = new PacketBuilder(Packets.SendToClient.PlayerSkills);
            builder.AddU8(_player.Skills.Fist.Get());
            builder.AddU8(_player.Skills.Club.Get());
            builder.AddU8(_player.Skills.Sword.Get());
            builder.AddU8(_player.Skills.Axe.Get());
            builder.AddU8(_player.Skills.Dist.Get());
            builder.AddU8(_player.Skills.Shield.Get());
            builder.AddU8(_player.Skills.Fish.Get());
            builder.Send(_player);
        }

        public void StatusIcons()
        {
            var builder = new PacketBuilder(Packets.SendToClient.PlayerStatusIcons);
            builder.AddU8((byte) Constants.StatusIcons.None);
            builder.Send(_player);
        }


        public void WorldLight()
        {
            var builder = new PacketBuilder(Packets.SendToClient.WorldLight);
            // 40 = night
            // 250 = day
            builder.AddU8(40);
            builder.AddU8(215);
            builder.Send(_player);
        }

        public void Stats()
        {
            var builder = new PacketBuilder(Packets.SendToClient.PlayerStats);
            builder.AddU16(_player.Hitpoints);
            builder.AddU16(_player.MaxHitpoints);
            builder.AddU16(_player.Capacity);
            builder.AddU32(_player.Experience);
            builder.AddU8(_player.Level);
            builder.AddU16(_player.Mana);
            builder.AddU16(_player.MaxMana);
            builder.AddU8(_player.MagicLevel.Get());
            builder.Send(_player);
        }

        public void Equipment()
        {
            Equip.Head(_player.Equipment.Head);
            Equip.Necklace(_player.Equipment.Necklace);
            Equip.Backpack(_player.Equipment.Backpack);
            Equip.Armor(_player.Equipment.Armor);
            Equip.Right(_player.Equipment.Right);
            Equip.Left(_player.Equipment.Left);
            Equip.Legs(_player.Equipment.Legs);
            Equip.Feet(_player.Equipment.Feet);
            Equip.Ring(_player.Equipment.Ring);
            Equip.Ammunition(_player.Equipment.Ammunition);
        }

        public void MoveSouth()
        {
            var builder = new PacketBuilder(Packets.SendToClient.CreaturePositionUpdate);
            var oldPos = _player.Position;
            var oldZIndex = _player.ZIndex;
            builder.AddPosition(oldPos);
            builder.AddU8(oldZIndex); // old stack position (z-index)
            _player.MoveDown();
            var newPos = _player.Position;
            builder.AddPosition(newPos);
            builder.Send(_player);

            builder.AddPacketId(Packets.SendToClient.MoveDown);
            /* add rest of map description */
            _player.Packets.Map.AddMapDescriptionToBuilder(builder,
                (ushort) (oldPos.X - 8),
                (ushort) (newPos.Y + 7),
                newPos.Z,
                GameClient.Width, 1, true);

            builder.Send(_player);

            var newZIndex = Game.Instance.GetCreatureZIndex(_player, _player.Position);
            Game.Instance.CreatureMoved(_player, oldPos, newPos, newZIndex);
        }

        public void MoveNorth()
        {
            var builder = new PacketBuilder(Packets.SendToClient.CreaturePositionUpdate);
            var oldPos = _player.Position;
            var oldZIndex = _player.ZIndex;
            builder.AddPosition(oldPos);
            builder.AddU8(_player.ZIndex); // old stack position (z-index)
            _player.MoveUp();
            var newPos = _player.Position;
            builder.AddPosition(newPos);
            builder.AddPacketId(Packets.SendToClient.MoveUp);
            /* add rest of map description */
            _player.Packets.Map.AddMapDescriptionToBuilder(builder, (ushort) (oldPos.X - 8), (ushort) (newPos.Y - 6),
                newPos.Z, GameClient.Width, 1, true);

            builder.Send(_player);
            Game.Instance.CreatureMoved(_player, oldPos, newPos, oldZIndex);
        }

        public void MoveWest()
        {
            var builder = new PacketBuilder(Packets.SendToClient.CreaturePositionUpdate);
            var oldPos = _player.Position;
            var oldZIndex = _player.ZIndex;
            builder.AddPosition(oldPos);
            builder.AddU8(oldZIndex); // old stack position (z-index)
            _player.MoveLeft();
            var newPos = _player.Position;
            builder.AddPosition(newPos);
            builder.AddPacketId(Packets.SendToClient.MoveLeft);
            /* add rest of map description */
            _player.Packets.Map.AddMapDescriptionToBuilder(builder,
                (ushort) (newPos.X - 8),
                (ushort) (newPos.Y - 6),
                newPos.Z, 1, GameClient.Height, true);

            builder.Send(_player);
            Game.Instance.CreatureMoved(_player, oldPos, newPos, oldZIndex);
        }

        public void MoveEast()
        {
            var builder = new PacketBuilder(Packets.SendToClient.CreaturePositionUpdate);
            var oldPos = _player.Position;
            var oldZIndex = _player.ZIndex;
            builder.AddPosition(oldPos);
            builder.AddU8(oldZIndex); // old stack position (z-index)
            _player.MoveRight();
            var newPos = _player.Position;
            builder.AddPosition(newPos);
            builder.AddPacketId(Packets.SendToClient.MoveRight);
            /* add rest of map description */
            _player.Packets.Map.AddMapDescriptionToBuilder(builder, 
                (ushort) (newPos.X + 9),
                (ushort) (newPos.Y - 6),
                newPos.Z, 1, GameClient.Height, true);

            builder.Send(_player);
            Game.Instance.CreatureMoved(_player, oldPos, newPos, oldZIndex);
        }

        public void ResetAutoWalk()
        {
            var builder = new PacketBuilder(Packets.SendToClient.ResetAutoWalk);
            builder.Send(_player);
        }

        public void StopAttack()
        {
            var builder = new PacketBuilder(Packets.SendToClient.StopAttack);
            builder.Send(_player);
        }
    }
}