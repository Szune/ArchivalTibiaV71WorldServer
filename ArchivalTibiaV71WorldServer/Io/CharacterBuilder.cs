using System.Collections.Generic;
using ArchivalTibiaV71WorldServer.Entities;

namespace ArchivalTibiaV71WorldServer.Io
{
    public class EquipmentBuilder
    {
        public Item Armor { get; set; } = Item.None;
        public Item Head { get; set; } = Item.None;
        public Item Necklace { get; set; } = Item.None;
        public Item Backpack { get; set; } = Item.None;
        public Item Right { get; set; } = Item.None;
        public Item Left { get; set; } = Item.None;
        public Item Legs { get; set; } = Item.None;
        public Item Feet { get; set; } = Item.None;
        public Item Ring { get; set; } = Item.None;
        public Item Ammunition { get; set; } = Item.None;

        public Equipment Build(List<Item> container)
        {
            Backpack.SetInside(container);
            return new Equipment(Head, Necklace, Backpack, Armor, Right, Left, Legs, Feet, Ring, Ammunition);
        }
    }

    public class CharacterBuilder
    {
        public Player Build()
        {
            // base speed = 110 + (level * 2)
            // if GM: base speed = 10 000
            var speed = IsGm ? 10_000 : 110 + (Level * 2);
            var skills = new Skills((byte) Fist, (uint) FistExp, (byte) Club, (uint) ClubExp, (byte) Sword,
                (uint) SwordExp, (byte) Axe, (uint) AxeExp, (byte) Dist, (uint) DistExp, (byte) Shield, (uint) ShieldExp,
                (byte) Fish, (uint) FishExp);

            return new Player((uint) Id, Name, Password, Outfit, Position, TemplePosition, (ushort) Hitpoints, (ushort) MaxHitpoints,
                (ushort) Mana,
                (ushort) MaxMana, (uint) Experience, (byte) Level, (byte) MagicLevel, (uint)MagicLevelExp, (ushort) Capacity, (ushort) speed,
                skills,
                Equipment.Build(Inventory)) { IsGm = IsGm };
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public Position Position { get; set; }
        public Position TemplePosition { get; set; }
        public Outfit Outfit { get; set; }
        public long Level { get; set; }
        public long Experience { get; set; }
        public long Hitpoints { get; set; }
        public long MaxHitpoints { get; set; }
        public long Mana { get; set; }
        public long MaxMana { get; set; }
        public long MagicLevel { get; set; }
        public long MagicLevelExp { get; set; }
        public long Capacity { get; set; }
        public long Fist { get; set; }
        public long FistExp { get; set; }
        public long Club { get; set; }
        public long ClubExp { get; set; }
        public long Sword { get; set; }
        public long SwordExp { get; set; }
        public long Axe { get; set; }
        public long AxeExp { get; set; }
        public long Dist { get; set; }
        public long DistExp { get; set; }
        public long Shield { get; set; }
        public long ShieldExp { get; set; }
        public long Fish { get; set; }
        public long FishExp { get; set; }
        public EquipmentBuilder Equipment { get; set; } = new EquipmentBuilder();
        public bool IsGm { get; set; }
        public List<Item> Inventory { get; set; }
    }
}