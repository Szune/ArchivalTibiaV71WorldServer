using System;
using System.Buffers.Binary;
using System.Text;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Extensions;

namespace ArchivalTibiaV71WorldServer.Utilities
{
    /// <summary>
    /// Little endian packet reader
    /// </summary>
    public class PacketReader
    {
        private readonly byte[] _bytes;
        private int _pos;

        public PacketReader(byte[] bytes)
        {
            _bytes = bytes;
            _pos = 0;
        }

        public ushort ReadU16()
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(_bytes.AsSpan(_pos, 2));
            _pos += 2;
            return value;
            // ushort value = (ushort)(_bytes[_pos + 0] + (_bytes[_pos + 1] << 8));
            // _pos += 2;
            // return value;
        }

        public Position ReadPosition()
        {
            return new(ReadU16(), ReadU16(), ReadU8());
        }

        public uint ReadU32()
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(_pos, 4));
            _pos += 2;
            return value;
            // uint value = _bytes[_pos + 0] + ((uint)_bytes[_pos + 1] << 8) + ((uint)_bytes[_pos + 2] << 16) + ((uint)_bytes[_pos + 3] << 24);
            // _pos += 4;
            // return value;
        }

        public byte ReadU8()
        {
            byte value = _bytes[_pos];
            _pos += 1;
            return value;
        }

        public Packets.ReceiveFromClient ReadPacketId()
        {
            return (Packets.ReceiveFromClient) ReadU8();
        }

        private string ReadPackedAsciiString(int length)
        {
            var value = Encoding.ASCII.GetString(_bytes.AsSpan(_pos, length));
            _pos += length;
            return value;
        }

        public string ReadString()
        {
            var length = ReadU16();
            return ReadPackedAsciiString(length);
        }

        public void SkipHeader()
        {
            Skip(3);
        }

        public void Skip(int bytes)
        {
            _pos += bytes;
        }

        public string ToHex(int bytes)
        {
            return _bytes.AsSpan(_pos, bytes).ToHex();
        }
    }
}