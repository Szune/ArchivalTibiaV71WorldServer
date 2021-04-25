using System;
using ArchivalTibiaV71WorldServer.Constants;

namespace ArchivalTibiaV71WorldServer.Entities
{
    [Flags]
    public enum CreatureFlags : byte
    {
        None = 0,
        Temporary = 0b1111_1111,
    }
    public class Creature
    {
        public Creature()
        {
            
        }

        public Creature(uint id)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0.", nameof(id));
            Id = id;
        }

        public Creature(uint id, string name, Outfit outfit, Position position, Position spawnPosition,
            ushort hitpoints, ushort maxHitpoints,
            ushort mana, ushort maxMana, uint experience, byte level, byte magicLevel, ushort capacity, ushort speed,
            CreatureFlags flags = CreatureFlags.None)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0.", nameof(id));
            Id = id;
            Position = position;
            SpawnPosition = spawnPosition;
            Name = name;
            Outfit = outfit;
            Hitpoints = hitpoints;
            MaxHitpoints = maxHitpoints;
            Mana = mana;
            MaxMana = maxMana;
            Experience = experience;
            Level = level;
            MagicLevel = new Skill<byte>(magicLevel, 0);
            Capacity = capacity;
            Flags = flags;
            Speed = new Skill<ushort>(speed, 110); // 110 is for level 1
            LightColor = Outfit.LightColor;
            LightLevel = Outfit.LightLevel;
            ZIndex = 1;
        }

        public uint Id { get; }
        public Position Position { get; private set; }
        public Position SpawnPosition { get; private set; }
        public Directions Direction = Directions.South;
        public string Name { get; set; }
        public Outfit Outfit { get; }
        public byte LightColor { get; private set; }
        public byte LightLevel { get; private set; }
        public byte PercentHitpoints => (byte) (MathF.Ceiling((Hitpoints / (float) MaxHitpoints) * 100f));
        public ushort Hitpoints { get; private set; }
        public ushort MaxHitpoints { get; }
        public ushort Mana { get; }
        public ushort MaxMana { get; }
        public uint Experience { get;protected set; }
        public byte Level { get; protected set; }
        public Skill<byte> MagicLevel { get; }
        public ushort Capacity { get; }
        public CreatureFlags Flags { get; }
        public Skill<ushort> Speed { get; }
        public byte ZIndex { get; set; }

        public long EarliestWalkTime { get; private set; }
        public long EarliestAttackTime { get; private set; }

        public bool CanWalk
        {
            get
            {
                if (_canWalk)
                    return true;
                _canWalk = EarliestWalkTime <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return _canWalk;
            }
        }

        public bool CanAttack
        {
            get
            {
                if (_canAttack)
                    return true;
                _canAttack = EarliestAttackTime <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return _canAttack;
            }
        }

        public static readonly Creature None = new Creature();

        private bool _canAttack = true;
        private bool _canWalk = true;


        public void SetLightLevel(byte level)
        {
            LightLevel = level;
        }

        public void SetLightColor(byte color)
        {
            LightColor = color;
        }

        public ushort GetSpeed()
        {
            return Speed.Get();
        }

        public void SetAttackTarget(uint targetId)
        {
            TargetId = targetId;
            TargetType = TargetType.Attacking;
        }

        public TargetType TargetType { get; set; } = TargetType.NoTarget;


        public uint TargetId { get; set; }

        /// <summary>
        /// Stops the creature from being able to walk until it has finished moving.
        /// </summary>
        /// <param name="moveOffset"></param>
        public void WalkWait(PositionOffset moveOffset)
        {
            // old:
            // var x = Speed < 1 ? 1000 : (tileSpeed * 1000) / Speed;
            // var y = (GameClient.BeatDuration - 1 + x) / GameClient.BeatDuration;
            // y *= GameClient.BeatDuration;
            // EarliestWalkTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + y;

            // new:

            // if (!floorChange)
            // {
            //     tileSpeed *= 3;
            // }
            // var currentTileSpeed = IoC.Game.Map[Position].Speed;
            // Position += moveOffset;
            // var destinationTileSpeed = IoC.Game.Map[Position].Speed;
            // //var speed = GetSpeed();
            // var x = currentTileSpeed < 1 ? 1000 : (destinationTileSpeed * 1000) / currentTileSpeed;
            // //var x = speed < 1 ? 1000 : (tileSpeed * 1000) / speed;
            // var y = (GameClient.Beat- 1 + x) / GameClient.Beat;
            // y *= GameClient.Beat;
            // EarliestWalkTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + y;

            // EarliestWalkTime =
            //     ((GameClient.Beat - 1 + (tileSpeed * 1000) / speed) / GameClient.Beat) * GameClient.Beat +
            //     DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // EarliestWalkTime =
            //     ((GameClient.Beat - 1 + (tileSpeed * 1000) / speed) / GameClient.Beat) * GameClient.Beat +
            //     DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var tileSpeed = IoC.Game.Map[Position].Speed;
            Position += moveOffset;
            ZIndex = IoC.Game.GetCreatureZIndex(this, Position);

            var speed = GetSpeed();
            EarliestWalkTime =
                ((GameClient.Beat - 1 + (tileSpeed * 1000) / speed) / GameClient.Beat) * GameClient.Beat +
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


            _canWalk = false;
        }

        public void AttackWait()
        {
            EarliestAttackTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + GameConstants.AttackTimeMs;
            _canAttack = false;
        }

        public void MoveUp()
        {
            Direction = Directions.North;
            WalkWait(PositionOffset.Up);
        }

        public void MoveRight()
        {
            Direction = Directions.East;
            WalkWait(PositionOffset.Right);
        }

        public void MoveDown()
        {
            Direction = Directions.South;
            WalkWait(PositionOffset.Down);
        }

        public void MoveLeft()
        {
            Direction = Directions.West;
            WalkWait(PositionOffset.Left);
        }

        public void TurnUp()
        {
            Direction = Directions.North;
        }

        public void TurnRight()
        {
            Direction = Directions.East;
        }

        public void TurnDown()
        {
            Direction = Directions.South;
        }

        public void TurnLeft()
        {
            Direction = Directions.West;
        }

        public override int GetHashCode()
        {
            return (int) Id;
        }

        /// <summary>
        /// Looks exclusively at id.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Creature c))
                return false;
            return Id == c.Id;
        }

        public void Damage(ushort damage)
        {
            // take armor and defense into account
            var aftermath = Hitpoints - damage;
            Hitpoints = (ushort)(aftermath >= 0 ? aftermath : 0);
        }
    }
}