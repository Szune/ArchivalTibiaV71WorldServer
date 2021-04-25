using System;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHandlers
{
    public class MessagePacketHandler : IPacketHandler
    {
        public void Handle(Player player, PacketReader reader)
        {
            var messageType = (MessageTypes) reader.ReadU8();
            switch (messageType)
            {
                case MessageTypes.Say:
                case MessageTypes.Whisper:
                case MessageTypes.Yell:
                    var message = reader.ReadString();
                    SendLocalMessageFromPlayer(player, messageType, message);
                    break;
                case MessageTypes.Private:
                    break;
                case MessageTypes.NotSure:
                    break;
                case MessageTypes.Broadcast:
                    break;
                case MessageTypes.Broadcast2:
                    break;
                case MessageTypes.Creature2:
                    break;
                case MessageTypes.Creature:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendLocalMessageFromPlayer(Player player, MessageTypes messageType, string message)
        {
            if (player.IsGm)
            {
                if (GmCommands.Parse(player, message))
                    return;
            }
            player.Packets.Message.LocalMessage(player.Position, player.Name, message, (byte)messageType);
            var c = IoC.Game.OnlinePlayers.Count;
            for(int i = 0; i < c; i++)
            {
                if (!IoC.Game.OnlinePlayers[i].Connection.Connected || ReferenceEquals(player, IoC.Game.OnlinePlayers[i]))
                    continue;
                if (Position.SameFloorAndScreen(player.Position, IoC.Game.OnlinePlayers[i].Position))
                {
                    IoC.Game.OnlinePlayers[i].Packets.Message.LocalMessage(player.Position, player.Name, message, (byte)messageType);
                }
            }
        }
    }
}