using System.Collections.Generic;
using ArchivalTibiaV71WorldServer.Entities;

namespace ArchivalTibiaV71WorldServer
{
    public class Tile
    {
        public static readonly Tile Empty = new Tile(null, null);
        public Tile(Item ground, Position position)
        {
            Ground = ground;
            Position = position;
        }
        
        public Tile(Item ground, ushort speed, Position position)
        {
            Ground = ground;
            Speed = speed;
            Position = position;
        }
        
        public Tile(Item ground, ushort speed, List<ushort> items, Position position)
        {
            Ground = ground;
            Speed = speed;
            Position = position;
            Items = new List<Item>(items.Count);
            for(var i = 0; i < items.Count; i++)
                Items.Add(IoC.Items.Create(items[i]));
        }


        public Item Ground { get; }
        public Position Position { get; }
        public ushort Speed { get; }
        public List<Item> Items { get; private set; }

        public void AddItem(Item item)
        {
            Items ??= new List<Item>();
            Items.Add(item);
        }

        public void AddOrReplaceFirstItem(Item item)
        {
            Items ??= new List<Item>();
            if (Items.Count < 1)
            {
                Items.Add(item);
            }
            else
            {
                Items[0] = item;
            }
        }

        public void SetOnlyItem(Item item)
        {
            Items ??= new List<Item>();

            if (Items.Count > 0)
            {
                Items.Clear();
            }

            Items.Add(item);
        }

        public void Decay(int decayIndex)
        {
        }
    }
}