using System;

namespace ArchivalTibiaV71WorldServer.Constants
{
    public static class Formulae
    {
        private static uint ExperienceToLevel(byte level)
        {
            if (level < 2)
                return 0;
            return (uint) ((50 * Math.Pow(level - 1, 3) - 150 * Math.Pow(level - 1, 2) + 400 * (level - 1)) / 3);
        }
        
        public static ushort Magic(byte level, ushort magicStrength, int targetDefense)
        {
            // TODO: fix later, just a placeholder
            return (ushort)(level + magicStrength - targetDefense);
        }
        
        public static ushort Melee(byte level, ushort meleeStrength, byte weaponDamage, int targetDefense)
        {
            // TODO: fix later, just a placeholder
            return (ushort)(level + meleeStrength + weaponDamage - targetDefense);
        }

        public static byte Level(byte level, uint experience)
        {
            if (level == 0xFF) // can't level higher than 255, Tibia 7.1 stores lvl as U8
                return level;
            var expToLevel = ExperienceToLevel(level);
            if (experience <= expToLevel) // don't downgrade people
                return level;
            byte cLvl = level;
            for (byte i = 1; i <= 25; i++)
            {
                expToLevel = ExperienceToLevel((byte) (level + i));
                if (experience >= expToLevel && cLvl + 1 < 0xFF)
                    cLvl += 1;
                else
                    break;
            }

            return cLvl;
        }
    }
}