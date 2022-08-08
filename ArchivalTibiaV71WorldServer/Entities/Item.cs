using System;
using System.Collections.Generic;

namespace ArchivalTibiaV71WorldServer.Entities
{
    [Flags]
    public enum ItemTypeFlags : byte
    {
        None = 0,
        Decaying = 1,
        Filled = 2,
        State1 = 4,
        State2 = 8,
        State3 = 16,
        State4 = 32,
        State5 = 64,
        State6 = 128,
    }
    public class Item
    {
        public ItemTypeFlags Flags = ItemTypeFlags.None;
        public static readonly Item None = new Item(0, 0);
        public readonly ushort Id;
        /// <summary>
        /// Item count or Splash index or fluid type
        /// </summary>
        public byte Extra;

        public List<Item> Inside;

        public Item(ushort id)
        {
            Id = id;
        }

        public Item(ushort id, byte extra)
        {
            Id = id;
            Extra = extra;
        }

        public bool IsNone => Id == 0;

        public void AddInside(Item item)
        {
            Inside ??= new List<Item>();
            Inside.Add(item);
        }
        
        public void SetInside(List<Item> item)
        {
            Inside = item;
        }

        public Item GetInside(byte slotIndex)
        {
            if (Inside == null)
                return None;
            if (slotIndex >= Inside.Count)
                return None;
            return Inside[slotIndex];
        }
    }
}