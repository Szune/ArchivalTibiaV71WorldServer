using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class MessageBoxes
    {
        private readonly Player _player;

        public MessageBoxes(Player player)
        {
            _player = player;
        }

        public void Sorry(string message)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.SorryMessageBox);
            builder.AddString(message);
            builder.Send(_player.Connection);
        }
    }
}