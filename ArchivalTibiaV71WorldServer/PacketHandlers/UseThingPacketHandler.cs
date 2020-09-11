using System;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;
using ArchivalTibiaV71WorldServer.WorldData;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class UseThingPacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var pos = reader.ReadPosition();
            var itemId = reader.ReadU16();
            var zIndex = reader.ReadU8();
            var newContainerIndex = reader.ReadU8(); // extra == 1 -> open in new window
            Console.WriteLine($"Item {itemId} on {pos} zindex {zIndex}, extra: {newContainerIndex}");
            if (pos.X == 0xFFFF)
            {
                if ((pos.Y & 64) != 64) // y has flag 64 means using thing in container
                {
                    Console.WriteLine("Using item on equipment slot");
                    var item = player.Equipment.Get((EquipmentSlots) pos.Y);
                    if (item.Id == 0)
                        return;

                    var strucc = Items.Instance.GetById(item.Id);
                    if ((strucc.Flags & ItemFlags.Container) == ItemFlags.Container)
                    {
                        player.Packets.Containers.Open(0, item, strucc, false);
                        //OpenContainer(player, 0, item, strucc, false);
                        //NewMethod1(player, item, strucc);
                    }
                    else
                    {
                        Console.WriteLine("Used something that wasn't a container");
                    }
                }
                else
                {
                    Console.WriteLine("Used something in a container slot");
                    var containerIndex = pos.Y - 64;
                    var item = player.GetContainer((byte)containerIndex);
                    if (item.Id == 0)
                        return;

                    var itemInSlot = item.GetInside(pos.Z);

                    var strucc = Items.Instance.GetById(itemInSlot.Id);
                    if ((strucc.Flags & ItemFlags.Container) == ItemFlags.Container)
                    {
                        player.Packets.Containers.Open(newContainerIndex, itemInSlot, strucc);
                        //OpenContainer(player, newContainerIndex, itemInSlot, strucc);
                        // use item in container slot
                        // zindex == slot in container
                    }
                }
            }
            else
            {
                // use item on ground
                Console.WriteLine($"Using item on ground");
            }
        }

        private static void NewMethod1(Player player, Item item, ItemStructure strucc)
        {
            var builder = new PacketBuilder(Packets.Send.OpenContainer);
            builder.AddU8(0); // container index
            builder.AddU16(item.Id); // container look id? the one identifying the container
            builder.AddString(strucc.Name);
            builder.AddU8(strucc.ContainerSize);
            builder.AddU8(0); // root container, 1 == child container
            var inside = player.Equipment.Backpack.Inside;
            if (inside == null)
            {
                builder.AddU8(0);
            }
            else
            {
                builder.AddU8((byte) inside.Count);
                for (int i = 0; i < inside.Count; i++)
                {
                    builder.AddItem(inside[i]);
                }
            }

            player.OpenContainer(player.Equipment.Backpack);

            builder.Send(player);
        }

        private static void OpenContainer(Player player, byte newContainerIndex, Item container, ItemStructure strucc, bool childContainer = true)
        {
            var builder = new PacketBuilder(Packets.Send.OpenContainer);
            builder.AddU8(newContainerIndex); // container index
            builder.AddU16(container.Id); // container look id? the one identifying the container
            builder.AddString(strucc.Name);
            builder.AddU8(strucc.ContainerSize);
            builder.AddU8((byte)(childContainer ? 1 : 0)); // 0 == root container, 1 == child container
            var inside = container.Inside;
            if (inside == null)
            {
                builder.AddU8(0);
            }
            else
            {
                builder.AddU8((byte) inside.Count);
                for (int i = 0; i < inside.Count; i++)
                {
                    builder.AddItem(inside[i]);
                }
            }

            player.CloseContainer(newContainerIndex);
            player.OpenContainer(container);

            builder.Send(player);
        }
    }
}