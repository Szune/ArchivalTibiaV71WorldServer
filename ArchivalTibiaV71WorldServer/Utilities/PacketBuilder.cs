using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using ArchivalTibiaV71WorldServer.Extensions;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.Utilities
{
    /// <summary>
    /// Little endian packet builder
    /// </summary>
    public class PacketBuilder
    {
        private readonly byte[] _buffer = new byte[1024 * 4];
        private int _pos = 2;


        public PacketBuilder()
        {
        }

        public PacketBuilder(Packets.Send id)
        {
            AddPacketId(id);
        }


        public void AddPacketId(Packets.Send packet) => AddU8((byte) packet);
        
        public void AddU8(byte value)
        {
            _buffer[_pos] = value;
            _pos += 1;
        }

        public void AddU16(ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_pos, 2), value);
            _pos += 2;
            // // low order byte
            // _buffer[_pos + 0] = (byte)(value & 0xFF);
            // // high order byte
            // _buffer[_pos + 1] = (byte)((value >> 8) & 0xFF);
            // _pos += 2;
        }

        public void AddU32(uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_pos, 4), value);
            _pos += 4;
            // _buffer[_pos + 0] = (byte)(value & 0xFF);
            // _buffer[_pos + 1] = (byte)((value >> 8) & 0xFF);
            // _buffer[_pos + 2] = (byte)((value >> 16) & 0xFF);
            // _buffer[_pos + 3] = (byte)((value >> 24) & 0xFF);
            // _pos += 4;
        }

        public void AddU64(ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_pos, 8), value);
            _pos += 8;
            // _buffer[_pos + 0] = (byte)(value & 0xFF);
            // _buffer[_pos + 1] = (byte)((value >> 8) & 0xFF);
            // _buffer[_pos + 2] = (byte)((value >> 16) & 0xFF);
            // _buffer[_pos + 3] = (byte)((value >> 24) & 0xFF);
            // _buffer[_pos + 4] = (byte)((value >> 32) & 0xFF);
            // _buffer[_pos + 5] = (byte)((value >> 40) & 0xFF);
            // _buffer[_pos + 6] = (byte)((value >> 48) & 0xFF);
            // _buffer[_pos + 7] = (byte)((value >> 56) & 0xFF);
            // _pos += 8;
        }


        public void AddPosition(Position p)
        {
            AddU16(p.X);
            AddU16(p.Y);
            AddU8(p.Z);
        }

        public void AddItem(Item item)
        {
            AddU16(item.Id);
            var strucc = Items.Instance.GetById(item.Id);
            if ((strucc.Flags & ItemFlags.Stackable) == ItemFlags.Stackable ||
                (strucc.Flags & ItemFlags.HoldsFluid) == ItemFlags.HoldsFluid ||
                strucc.IsSplash)
            {
                AddU8(item.Extra);
            }
        }

        public void AddString(string s)
        {
            var sBytes = Encoding.ASCII.GetBytes(s);
            var sLen = sBytes.Length;
            AddU16((ushort) sLen);
            Buffer.BlockCopy(sBytes, 0, _buffer, _pos, sLen);
            _pos += sLen;
        }


        public void AddPlayerMove(Player player)
        {
            AddU16(0x63);
            AddU32(player.Id);
            AddU8((byte) player.Direction);
        }

        public void AddCreature(Player player, Creature creature)
        {
            if (player.IsCreatureKnown(creature.Id))
            {
                AddU16(0x62); // creature is known
                AddU32(creature.Id);
                player.RefreshKnownCreature(creature.Id);
            }
            else
            {
                player.AddKnownCreature(creature.Id);
                AddU16(0x61); // creature is not known
                AddU32(player.GetKnownCreatureToRemove());
                AddU32(creature.Id);
                AddString(creature.Name);
            }

            AddU8(creature.PercentHitpoints);
            AddU8((byte) creature.Direction); // direction (2 == south)
            if (creature.Outfit.Id == 0)
            {
                AddU8(creature.Outfit.Id); // invisible
                AddU16(0);
            }
            else
            {
                AddU8(creature.Outfit.Id); // wearing an outfit, not invisible
                AddU8(creature.Outfit.Head); // head
                AddU8(creature.Outfit.Body); // body
                AddU8(creature.Outfit.Legs); // legs
                AddU8(creature.Outfit.Feet); // feet
            }

            AddU8(creature.Outfit.LightLevel); // possibly light level
            AddU8(creature.Outfit.LightColor); // possibly light color
            AddU16(creature.GetSpeed()); // speed
        }
        

        public void Send(Socket connection)
        {
            var packetSize = _pos - 2;
            _buffer[0] = (byte) (packetSize & 0xFF);
            _buffer[1] = (byte) ((packetSize >> 8) & 0xFF);
            try
            {
            var sent = connection.Send(_buffer, 0, _pos, SocketFlags.None);
#if DEBUG
            Console.Write("[");
            Console.Write(Enum.IsDefined(typeof(Packets.Send), _buffer[2])
                ? ((Packets.Send) _buffer[2]).ToString()
                : $"0x{_buffer[2]:X2}");
            Console.WriteLine($"] Sent {_buffer.AsSpan(0, sent).ToHex()}");
#endif
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Failed to send packet.");
            }
            Reset();
        }

        public void Send(Player player)
        {
            var packetSize = _pos - 2;
            _buffer[0] = (byte) (packetSize & 0xFF);
            _buffer[1] = (byte) ((packetSize >> 8) & 0xFF);
            try
            {
                var sent = player.Connection.Send(_buffer, 0, _pos, SocketFlags.None);
#if DEBUG
                Console.Write("[");
                Console.Write(Enum.IsDefined(typeof(Packets.Send), _buffer[2])
                    ? ((Packets.Send) _buffer[2]).ToString()
                    : $"0x{_buffer[2]:X2}");
                Console.WriteLine($"] Sent to {player.Name}: {_buffer.AsSpan(0, sent).ToHex()}");
#endif
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"Failed to send packet: {player.Name} disconnected.");
            }

            Reset();
        }
        
        public void Reset()
        {
            _pos = 2;
        }

    }
}