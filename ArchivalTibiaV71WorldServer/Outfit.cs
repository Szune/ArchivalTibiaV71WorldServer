using ArchivalTibiaV71WorldServer.Constants;

namespace ArchivalTibiaV71WorldServer
{
    public class Outfit
    {
        public Outfit(Outfits id, byte head, byte body, byte legs, byte feet, byte lightLevel = 0, byte lightColor = 0)
        {
            Id = (byte)id;
            Head = head;
            Body = body;
            Legs = legs;
            Feet = feet;
            LightLevel = lightLevel;
            LightColor = lightColor;
        }

        public Outfit(Outfits id, byte lightLevel = 0, byte lightColor = 0x7D)
        {
            Id = (byte)id;
            LightLevel = lightLevel;
            LightColor = lightColor;
        }

        /// <summary>
        /// 75 for GM, check .dat for rest
        /// </summary>
        public byte Id { get; private set; }
        public byte Head { get; private set; }
        public byte Body { get; private set; }
        public byte Legs { get; private set; }
        public byte Feet { get; private set; }
        public byte LightLevel { get; private set; }
        public byte LightColor { get; private set; }

        public void Set(Outfits outfit)
        {
            Id = (byte) outfit;
        }

        public void Set(Outfits outfit, byte head, byte body, byte legs, byte feet)
        {
            Id = (byte) outfit;
            Head = head;
            Body = body;
            Legs = legs;
            Feet = feet;
        }
    }
}
