namespace ArchivalTibiaV71WorldServer.Constants
{
    public class OutfitDefinition
    {
        public Outfits Outfit { get; }
        public byte LightLevel { get; }
        public byte LightColor { get; }

        public OutfitDefinition(Outfits outfit, byte lightLevel, byte lightColor)
        {
            Outfit = outfit;
            LightLevel = lightLevel;
            LightColor = lightColor;
        }
    }

    public static class OutfitDefinitions
    {
        public static readonly OutfitDefinition FireElemental = new OutfitDefinition(Outfits.FireElemental, 4, 208);
    }

    public enum Outfits : byte
    {
        Invisible = 0,
        Sheep = 14,
        Minotaur = 25,
        Skeleton = 33,
        Dragon = 34,
        Demon = 35,
        DragonLord = 39,
        FireDevil = 40,
        FireElemental = 49,
        Behemoth = 55,
        Vampire = 68,
        MaleCitizen = 72,
        Hero = 73,
        Gm = 75,
        MaleKnight = 131,
    }
}