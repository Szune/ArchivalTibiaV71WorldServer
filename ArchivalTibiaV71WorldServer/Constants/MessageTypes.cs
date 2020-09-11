namespace ArchivalTibiaV71WorldServer.Constants
{
    public enum MessageTypes : byte
    {
        Say = 0x1,
        Whisper = 0x2,
        Yell = 0x3,
        Private = 0x4,
        NotSure = 0x8,
        Broadcast = 0x9,
        Broadcast2 = 0xA,
        Creature2 = 0xD,
        Creature = 0xE,
    }
}