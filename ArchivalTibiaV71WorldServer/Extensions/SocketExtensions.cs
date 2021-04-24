using System.Net.Sockets;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.Extensions
{
    public static class SocketExtensions
    {
        
        public static void SendSorryMessageBox(this Socket connection, string message)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.SorryMessageBox);
            builder.AddString(message);
            builder.Send(connection);
        }
    }
}