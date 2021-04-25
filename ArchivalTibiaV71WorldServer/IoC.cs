using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer
{
    public static class IoC
    {
        public static Game Game;
        public static Items Items;
        
        public static void InitializeForLive()
        {
            Game = Game.CreateInstance();
            Items = Items.CreateInstance();
        }
        
        public static void InitializeForTests()
        {
            
        }
    }
}