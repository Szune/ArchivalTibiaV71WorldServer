﻿using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;
using ArchivalTibiaV71WorldServer.WorldData;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class LookAtPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var pos = reader.ReadPosition();
            var itemId = reader.ReadU16();
            var topItem = reader.ReadU8();

            if (pos.X == 0xFFFF)
            {
                //Console.WriteLine($"Looked at item {itemId} (top item: {topItem}) in slot {(EquipmentSlots)pos.Y}");

                var strucc = Items.Instance.GetById(itemId);
                if (strucc.Id == 0)
                    return;

                if (pos.Y == 64)
                {
                    LookAtItem(player, strucc);
                    return;
                }

                LookAtItem(player, strucc);
                //player.Packets.Message.LookAt($"You are looking at your {((EquipmentSlots)pos.Y).ToString().ToLower()}.");
                //player.Packets.Message.LookAt($"You are looking at your {((EquipmentSlots)pos.Y).ToString().ToLower()}.");
            }
            else if (Position.Equals(pos, player.Position))
            {
                player.Packets.Message.LookAt($"You are looking at yourself.");
            }
            else
            {
                var strucc = Items.Instance.GetById(itemId);
                if (strucc.Id == 0)
                    return;

                LookAtItem(player, strucc);
                //Console.WriteLine($"Looked at item {itemId} (top item: {topItem}) ({pos.X}, {pos.Y}, {pos.Z})");
                // player.Packets.Message.LookAt($"You are looking at {(topItem == 0 ? "the ground" : "some sort of item")}.");
            }
        }

        private void LookAtItem(Player player, ItemStructure strucc)
        {
            switch (strucc.Slot)
            {
                case EquipmentSlots.Head:
                case EquipmentSlots.Armor:
                case EquipmentSlots.Legs:
                case EquipmentSlots.Feet:
                    LookAt(player, $"{strucc.Name} (Arm:{strucc.Armor})", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.Necklace:
                    if (strucc.Armor > 0)
                    {
                        LookAt(player, $"{strucc.Name} (Arm:{strucc.Armor})", strucc.Weight, strucc.Description);
                    }
                    else
                    {
                        LookAt(player, $"{strucc.Name}", strucc.Weight, strucc.Description);
                    }

                    break;
                case EquipmentSlots.Backpack:
                    LookAt(player, $"{strucc.Name} (Vol:{strucc.ContainerSize})", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.Ring:
                    LookAt(player, $"{strucc.Name}", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.Ammunition:
                    LookAt(player, $"{strucc.Name}", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.Weapon:
                    LookAt(player, $"{strucc.Name} (Atk:{strucc.Attack}, Def:{strucc.Defense})", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.Shield:
                    LookAt(player, $"{strucc.Name} (Def:{strucc.Defense})", strucc.Weight, strucc.Description);
                    break;
                case EquipmentSlots.None:
                    if (string.IsNullOrWhiteSpace(strucc.Name))
                        LookAt(player, $"an item of id {strucc.Id}", 0);
                    else
                        LookAt(player, $"{strucc.Name}", strucc.Weight, strucc.Description);
                    break;
                default:
                    LookAt(player, $"an item of id {strucc.Id}", 0);
                    break;
            }
        }

        public void LookAt(Player player, string message, ushort weight, string description=default)
        {
            var desc = string.IsNullOrWhiteSpace(description) ? "" : "\n" + description;
            if (weight > 0)
            {
                player.Packets.Message.LookAt($"You see {message}.\nIt weighs {weight / 100}.{weight % 100:00} oz.{desc}");
            }
            else
            {
                player.Packets.Message.LookAt($"You see {message}.{desc}");
            }
        }
    }
}