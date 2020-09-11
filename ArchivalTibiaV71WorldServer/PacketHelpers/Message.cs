using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class Message
    {
        private readonly Player _player;

        public Message(Player player)
        {
            _player = player;
        }
        public void Animated(Position pos, Colors color, string message)
        {
            var builder = new PacketBuilder(Packets.Send.AnimatedText);
            builder.AddPosition(pos);
            builder.AddU8((byte)color);
            builder.AddString(message);
            builder.Send(_player);
            
        }
        public void PlayerBroadcast(string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Broadcast); // message type
            builder.AddString(message);
            builder.Send(_player);
        }
        
        public void PlayerBroadcast2(string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Broadcast2); // message type
            builder.AddString(message);
            builder.Send(_player);
        }

        public void Private(string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Private); // message type
            builder.AddString(message);
            builder.Send(_player);
        }
        
        public void LocalMessage(Position pos, string fromName, string message, byte speechType)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8(speechType); // message type
            builder.AddPosition(pos);
            builder.AddString(message);
            builder.Send(_player);
        }

        public void Say(Position pos, string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Say); // message type
            builder.AddPosition(pos);
            builder.AddString(message);
            builder.Send(_player);
        }

        public void Whisper(Position pos, string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Whisper); // message type
            builder.AddPosition(pos);
            builder.AddString(message.ToLower());
            builder.Send(_player);
        }

        public void Yell(Position pos, string fromName, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(fromName); // source player
            builder.AddU8((byte) MessageTypes.Yell); // message type
            builder.AddPosition(pos);
            builder.AddString(message.ToUpper());
            builder.Send(_player);
        }

        public void Creature(Position pos, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(""); // source player
            builder.AddU8((byte) MessageTypes.Creature); // message type
            builder.AddPosition(pos);
            builder.AddString(message);
            builder.Send(_player);
        }
        
        public void Creature2(Position pos, string message)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(""); // source player
            builder.AddU8((byte) MessageTypes.Creature2); // message type
            builder.AddPosition(pos);
            builder.AddString(message);
            builder.Send(_player);
        }

        /// <summary>
        /// Probably some sort of global channel message
        /// </summary>
        public void NotSure(string text, string something)
        {
            var builder = new PacketBuilder(Packets.Send.Message);
            builder.AddString(text); // source player
            builder.AddU8((byte) MessageTypes.NotSure); // message type
            builder.AddString(something); // source player
            builder.Send(_player);
        }

        public void StatusAndConsole(string message)
        {
            var builder = new PacketBuilder(Packets.Send.SpecialMessage);
            builder.AddU8(0x11);
            builder.AddString(message);
            builder.Send(_player);
        }

        public void Status(string message)
        {
            var builder = new PacketBuilder(Packets.Send.SpecialMessage);
            builder.AddU8(0x14);
            builder.AddString(message);
            builder.Send(_player);
        }

        public void ServerBroadcast(string message)
        {
            var builder = new PacketBuilder(Packets.Send.SpecialMessage);
            builder.AddU8(0xF);
            builder.AddString(message);
            builder.Send(_player);
        }
        
        public void LookAt(string message)
        {
            var builder = new PacketBuilder(Packets.Send.SpecialMessage);
            builder.AddU8(0x13);
            builder.AddString(message);
            builder.Send(_player);
        }
        
        public void AdvanceOrRaid(string message)
        {
            var builder = new PacketBuilder(Packets.Send.SpecialMessage);
            builder.AddU8(0x10);
            builder.AddString(message);
            builder.Send(_player);
        }
    }
}