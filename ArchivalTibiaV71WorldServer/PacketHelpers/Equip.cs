using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class Equip
    {
        private readonly Player _player;

        public Equip(Player player)
        {
            _player = player;
        }
        public void Head(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Head);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Head = item;
        }
        
        public void Necklace(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Necklace);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Necklace = item;
        }
        
        public void Backpack(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Backpack);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Backpack = item;
        }
        
        public void Armor(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Armor);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Armor = item;
        }
        
        public void Right(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Right);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Right = item;
        }
        
        public void Left(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Left);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Left = item;
        }
        
        public void Legs(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Legs);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Legs = item;
        }
        
        
        public void Feet(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Feet);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Feet = item;
        }
        
        public void Ring(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Ring);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Ring = item;
        }
        
        public void Ammunition(Item item)
        {
            if (item.Id == 0)
                return;
            var builder = new PacketBuilder(Packets.SendToClient.FillEquipmentSlot);
            builder.AddU8((byte) EquipmentSlots.Ammunition);
            builder.AddItem(item);
            builder.Send(_player);
            _player.Equipment.Ammunition = item;
        }
    }
}