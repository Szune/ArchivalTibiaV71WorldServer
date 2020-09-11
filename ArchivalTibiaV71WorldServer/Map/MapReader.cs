using System.Collections.Generic;
using System.IO;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.Map
{
    public class MapReader
    {
        private BinaryReader _reader;

        public MapReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public Tile Read()
        {
            if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
                return null;
            
            
            var x = _reader.ReadUInt16();
            var y = _reader.ReadUInt16();
            var flags = (TileFlags)_reader.ReadByte();
            var z = (byte)(flags & (TileFlags)15); // Z goes 0 to 15
            var itemId = _reader.ReadUInt16();
            var items = new List<ushort>(3);
            if (flags.HasFlag(TileFlags.HasItems))
            {
                var count = _reader.ReadByte();
                for (int a = 0; a < count; a++)
                {
                    items.Add(_reader.ReadUInt16());
                }
            }
            var speed = Items.Instance.GetById(itemId).Speed;
            var item = Items.Instance.Create(itemId);
            return new Tile(item, speed, items, new Position(x, y, z));
        }
    }
}