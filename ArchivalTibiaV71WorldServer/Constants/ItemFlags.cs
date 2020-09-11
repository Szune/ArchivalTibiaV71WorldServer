using System;

namespace ArchivalTibiaV71WorldServer.Constants
{
    [Flags]
    public enum ItemFlags : uint
    {
        None = 0,
        Stackable = 0b1,
        Charges = 0b10,
        Resistance = 0b100,
        Skill = 0b1000,
        Regeneration = 0b1_0000,
        Speed = 0b10_0000,
        FloorUpClick = 0b100_0000,
        FloorDownClick = 0b1000_0000,
        RopeUp = 0b1_0000_0000,
        Lever = 0b10_0000_0000,
        /// <summary>
        /// Doors
        /// </summary>
        OpenClick = 0b100_0000_0000,
        /// <summary>
        /// Doors
        /// </summary>
        CloseClick = 0b1000_0000_0000,
        FloorUpWalk = 0b1_0000_0000_0000,
        FloorDownWalk = 0b10_0000_0000_0000,
        Mailbox = 0b100_0000_0000_0000,
        DepotOpenClick = 0b1000_0000_0000_0000,
        Dustbin = 0b1_0000_0000_0000_0000,
        OpenWithShovel = 0b10_0000_0000_0000_0000,
        /// <summary>
        /// Can use 'look' on or read text
        /// </summary>
        Readable = 0b100_0000_0000_0000_0000,
        Writable = 0b1000_0000_0000_0000_0000,
        GroundTile = 0b1_0000_0000_0000_0000_0000,
        OnTopTile = 0b10_0000_0000_0000_0000_0000,
        Container = 0b1000_0000_0000_0000_0000_0000,
        BlockMissile = 0b1_0000_0000_0000_0000_0000_0000,
        BlockCreatureMove = 0b10_0000_0000_0000_0000_0000_0000,
        HoldsFluid = 0b100_0000_0000_0000_0000_0000_0000,
        CanPickUp = 0b1000_0000_0000_0000_0000_0000_0000,
        Immovable = 0b1_0000_0000_0000_0000_0000_0000_0000,
        CanUse = 0b10_0000_0000_0000_0000_0000_0000_0000,
        Blocking = 0b100_0000_0000_0000_0000_0000_0000_0000,
        Illuminating = 0b1000_0000_0000_0000_0000_0000_0000_0000,
    }
}