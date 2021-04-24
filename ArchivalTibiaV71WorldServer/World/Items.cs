using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.WorldData;

namespace ArchivalTibiaV71WorldServer.World
{
    public class Items
    {
        public static readonly Items Instance = new Items();
        private Dictionary<ushort, ItemStructure> _items = new Dictionary<ushort, ItemStructure>();

        private Items()
        {
        }

        public bool Load()
        {
            try
            {
                Console.Write("- Loading data/items.json...");
                var json = File.ReadAllText("data/items.json");
                _items = JsonSerializer
                    .Deserialize<List<ItemStructure>>(json)
                    .ToDictionary(i => i.Id);
                Console.WriteLine(" OK");
                return true;
            }
            catch (Exception ex)
            {
                _items = new Dictionary<ushort, ItemStructure>();
                Console.WriteLine($"Failed to load items.json: {ex}");
                return false;
            }
        }

        public ItemStructure GetById(ushort id)
        {
            if (_items.TryGetValue(id, out var item))
            {
                return item;
            }

            return ItemStructure.None;
        }

        public Item Create(ushort id)
        {
            return new Item(id);
        }

        public Item Create(ushort id, byte extra)
        {
            return new Item(id, extra);
        }

        public Item CreateWithFlags(ushort id, ItemTypeFlags flags)
        {
            var item = new Item(id) { Flags = flags };
            return item;
        }

        public Item CreateWithFlags(ushort id, byte extra, ItemTypeFlags flags)
        {
            var item = new Item(id, extra) { Flags = flags };
            return item;
        }
        
        public byte GetMagicId(ushort item)
        {
            return item switch
            {
                1618 => 18, // sudden death
                1661 => 12, // heavy magic missile rune
                _ => throw new ArgumentOutOfRangeException(nameof(item)), // note: bad idea to crash on malicious ids
            };
        }

        public byte GetProjectileId(ushort item)
        {
            return item switch
            {
                1618 => 11, // sudden death
                1661 => 4, // heavy magic missile rune
                _ => throw new ArgumentOutOfRangeException(nameof(item)), // note: bad idea to crash on malicious ids
            };
        }
    }
}