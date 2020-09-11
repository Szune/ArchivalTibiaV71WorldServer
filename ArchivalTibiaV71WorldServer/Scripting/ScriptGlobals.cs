using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.Scripting
{
    public class ScriptGlobals
    {
        public ScriptGlobals()
        {
            
        }
        public ScriptGlobals(Player player, byte packetId, ushort packetLength, PacketReader reader)
        {
            PacketId = packetId;
            PacketLength = packetLength;
            Reader = reader;
            Player = player;
            Game = Game.Instance;
        }
        
        public Game Game;
        public Player Player;
        public byte PacketId;
        public ushort PacketLength;
        public PacketReader Reader;
    }
}