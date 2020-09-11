namespace ArchivalTibiaV71WorldServer.Constants
{
    public static class Packets
    {
        public enum Send : byte
        {
            LoginSuccess = 0xA,
            SorryMessageBox = 0x14,
            SocketKeepAlive = 0x1E,
            FullScreenMap = 0x64,
            MoveUp = 0x65,
            MoveRight = 0x66,
            MoveDown = 0x67,
            MoveLeft = 0x68,
            UpdateSingleTile = 0x69,
            ItemOrCreatureAppearOnTile = 0x6A,
            CreatureTurn = 0x6B,
            ItemOrCreatureDisappear = 0x6C,
            CreaturePositionUpdate = 0x6D,
            OpenContainer = 0x6E,
            CloseContainer = 0x6F,
            FillEquipmentSlot = 0x78,
            ClearEquipmentSlot = 0x79,
            WorldLight = 0x82,
            MagicEffect = 0x83,
            AnimatedText = 0x84,
            UpdateCreatureHealthPercent = 0x8C,
            UpdateCreatureLight = 0x8D,
            UpdateCreatureOutfit = 0x8E,
            UpdateCreatureSpeed = 0x8F,
            PlayerStats = 0xA0,
            PlayerSkills = 0xA1,
            PlayerStatusIcons = 0xA2,
            StopAttack = 0xA3,
            Message = 0xAA,
            SpecialMessage = 0xB4,
            ResetAutoWalk = 0xB5,
        }
        public enum Receive : byte
        {
            AccountLogin = 0x01,
            CharacterLogin = 0x0A,
            Logout = 0x14,
            SocketKeepAlive = 0x1E,
            AttackTarget = 0xA1,
            AutoWalkDirections = 0x64,
            MoveUp = 0x65,
            MoveRight = 0x66,
            MoveDown = 0x67,
            MoveLeft = 0x68,
            StopAutoWalk = 0x69,
            TurnUp = 0x6F,
            TurnRight = 0x70,
            TurnDown = 0x71,
            TurnLeft = 0x72,
            MoveObject = 0x78,
            UseThing = 0x82,
            /// <summary>
            /// FF FF unknown
            /// 40 00 00 7D unknown
            /// 06 00 unknown
            /// Position
            /// 63 == creature, 66 == ???
            /// 00 unknown
            /// zindex
            /// </summary>
            UseCrosshairThing = 0x83,
            CloseContainer = 0x87,
            LookAt = 0x8C,
            Message = 0x96,
            OpenNewChannel = 0x97,
            OpenPmWithPlayer = 0x9A,
            AttackChaseMode = 0xA0,
            RequestSingleTile = 0xC9,
            OpenSetOutfitDialog = 0xD2,
            AddVip = 0xDC,
        }
    }
}