using System.Collections.Generic;
using ArchivalTibiaV71WorldServer.Constants;

namespace ArchivalTibiaV71WorldServer.WorldData
{
    public class ItemStructure
    {
        public static readonly ItemStructure None = new ItemStructure();
        public EquipmentSlots Slot { get; set; }
        public ItemFlags Flags { get; set; }
        public bool AddsZIndex { get; set; }
        public ushort MaxTextLength { get; set; }
        public ushort LightLevel { get; set; }
        public ushort LightColor { get; set; }
        public ushort Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// If there is text in it
        /// </summary>
        public string String { get; set; }
        /// <summary>
        /// Who wrote the text
        /// </summary>
        public string Author { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte Armor { get; set; }
        /// <summary>
        /// Weight % 100 = after decimal
        /// Weight / 100 = before decimal
        /// </summary>
        public ushort Weight { get; set; }
        public byte Charges { get; set; }
        public byte ContainerSize { get; set; }
        public byte ResistancePhysical { get; set; }
        public byte ResistanceFire { get; set; }
        public byte ResistancePoison { get; set; }
        public byte ResistanceEnergy { get; set; }
        /// <summary>
        /// Seconds
        /// </summary>
        public ushort Duration { get; set; }
        public byte SkillAxe { get; set; }
        public byte SkillClub { get; set; }
        public byte SkillSword { get; set; }
        public byte RegenerationLife { get; set; }
        public byte RegenerationMana { get; set; }
        /// <summary>
        /// If ground tile, this is tile speed, otherwise it is speed change
        /// </summary>
        public ushort Speed { get; set; }
        public bool IsSplash { get; set; }
        public List<ushort> SpriteIds { get; set; }
    }
}