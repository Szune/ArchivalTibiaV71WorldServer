using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.WorldData;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class Containers
    {
        private readonly Player _player;

        public Containers(Player player)
        {
            _player = player;
        }

        public void Close(byte index)
        {
            var builder = new PacketBuilder(Packets.SendToClient.CloseContainer);
            builder.AddU8(index);
            builder.Send(_player);
        }

        public void Open(byte newContainerIndex, Item container, ItemStructure strucc, bool childContainer = true)
        {
            var builder = new PacketBuilder(Packets.SendToClient.OpenContainer);
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

            _player.OpenContainer(container);

            builder.Send(_player);
        }
    }
}