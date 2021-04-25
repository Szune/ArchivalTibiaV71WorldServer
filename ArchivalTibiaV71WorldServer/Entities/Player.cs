using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.PacketHelpers;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer.Entities
{
    public class Player : Creature
    {
        public Player(uint id, string name, string password, Outfit outfit, Position position, Position spawnPosition,
            ushort hitpoints,
            ushort maxHitpoints, ushort mana, ushort maxMana, uint experience, byte level, byte magicLevel,
            uint magicLevelExp,
            ushort capacity, ushort speed, Skills skills, Equipment equipment)
            : base(id, name, outfit, position, spawnPosition, hitpoints, maxHitpoints, mana, maxMana, experience, level, magicLevel,
                capacity, speed)
        {
            Packets = new PacketHelper(this);
            Password = password;
            Skills = skills;
            Equipment = equipment;
            MagicLevel.SetExp(magicLevelExp);
        }

        public PacketHelper Packets;

        public Equipment Equipment;
        public bool IsGm = false;
        private List<Item> _openContainers;

        public void OpenContainer(Item item)
        {
            _openContainers ??= new List<Item>(2);
            _openContainers.Add(item);
        }

        public Item GetContainer(byte index)
        {
            if(_openContainers == null)
                return Item.None;
            
            if (index >= _openContainers.Count)
                return Item.None;
            return _openContainers[index];
        }

        public void CloseContainer(byte index)
        {
            if (_openContainers == null)
                return;
            if (index >= _openContainers.Count)
                return;
            _openContainers.RemoveAt(index);
            Packets.Containers.Close(index);
        }

        public AttackMode AttackMode { get; private set; }
        public ChaseMode ChaseMode { get; private set; }

        public Socket Connection { get; private set; }
        public Socket PreConnection { get; private set; }

        public void SetConnection(Socket connection)
        {
            Connection = connection;
            ClearKnownCreatures();
        }

        public void SetPreConnection(Socket connection) =>
            PreConnection = connection;


        public Skills Skills { get; }
        public string Password { get; set; }

        public void AddKnownCreature(uint id)
        {
            _knownCreatures.Add(id);
        }

        public bool IsCreatureKnown(uint id)
        {
            return _knownCreatures.Contains(id);
        }

        public uint GetKnownCreatureToRemove()
        {
            // 252 is the amount of creatures that would take up an entire screen
            // so 255 should be fine, the client allows for 400
            uint remove = 0;
            if (_knownCreatures.Count < 255) 
            {
                return remove;
            }

            var c = _knownCreatures.Count;
            for(int i = 0; i < c; i++)
            {
                var cr = IoC.Game.GetCreatureById(_knownCreatures[i]);
                if (cr.Id == 0)
                    return _knownCreatures[i];
                if (!Position.SameScreen(cr.Position, Position))
                    return _knownCreatures[i];
                RefreshKnownCreature(_knownCreatures[i]);
            }

            return 0;
        }
        public void RefreshKnownCreature(uint id)
        {
            _knownCreatures.Remove(id);
            _knownCreatures.Add(id);
        }

        private readonly KnownCreaturesContainer _knownCreatures = new KnownCreaturesContainer();

        public void ClearKnownCreatures()
        {
            _knownCreatures.Clear();
        }


        public void AddOrReplaceAutoWalk(Directions direction)
        {
            if (_autoWalkSteps.Count > 0)
            {
                _autoWalkSteps.Clear();
            }

            _autoWalkSteps.Enqueue(direction);
        }

        public void ClearAutoWalk()
        {
            _autoWalkSteps.Clear();
        }
        public void AddAutoWalk(Directions direction)
        {
            _autoWalkSteps.Enqueue(direction);
        }

        private Queue<Directions> _autoWalkSteps = new Queue<Directions>(16);

        public Directions TakeStep()
        {
            if (_autoWalkSteps.Count < 1)
                return Directions.None;
            return _autoWalkSteps.Dequeue();
        }

        public void SetAttackMode(AttackMode attackMode)
        {
            AttackMode = attackMode;
        }

        public void SetChaseMode(ChaseMode chaseMode)
        {
            ChaseMode = chaseMode;
        }

        public void StopTargeting()
        {
            TargetId = 0;
            TargetType = TargetType.NoTarget;
            Packets.StopAttack();
        }

        public void Attack(Creature creature)
        {
            AttackWait();
            // take weapon and skill into account
            creature.Damage(Level);
            Packets.Message.Animated(creature.Position, Colors.Red, Level.ToString());
            Packets.Creature.UpdateHealth(creature);
            
            if (creature.Hitpoints < 1)
            {
                StopTargeting();
                AddExperience(creature.Experience);
                Packets.Map.CreatureDisappear(creature);
                var pos = creature.Position;
                if(creature.Flags.HasFlag(CreatureFlags.Temporary))
                {
                    IoC.Game.TemporaryCreatures.Remove(creature);
                }
                var item = IoC.Items.CreateWithFlags(2170, ItemTypeFlags.Decaying);
                Packets.Map.ItemAppear(pos, item);
                IoC.Game.Map[pos].AddItem(item);
                IoC.Game.DecayingItems.Add(IoC.Game.Map[pos]);
            }
        }

        public void AddExperience(uint exp)
        {
            Experience += exp;
            var level = Formulae.Level(Level, Experience); // should probably cache exp to next level instead
            if (level > Level)
            {
                AddLevel((byte)(level - Level));
            }
            Packets.Message.Animated(Position, Colors.White, exp.ToString());
            Packets.Stats();
        }

        private void AddLevel(byte level)
        {
            var oldLevel = Level;
            Level += level;
            Packets.Message.AdvanceOrRaid($"You advanced from level {oldLevel} to {Level}.");
        }

        public void OnLogout()
        {
            try
            {
                _openContainers?.Clear();
                ClearKnownCreatures();
                Connection.Close();
                IoC.Game.CreatureDisappear(this, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" -- Exception during logout of {Name}: {ex}");
            }
        }
    }

    public class Skills
    {
        public Skill<byte> Fist;
        public Skill<byte> Club;
        public Skill<byte> Sword;
        public Skill<byte> Axe;
        public Skill<byte> Dist;
        public Skill<byte> Shield;
        public Skill<byte> Fish;

        public Skills(byte fist, byte club, byte sword, byte axe, byte dist, byte shield, byte fish)
        {
            Fist = new Skill<byte>(fist, 10);
            Club = new Skill<byte>(club, 10);
            Sword = new Skill<byte>(sword, 10);
            Axe = new Skill<byte>(axe, 10);
            Dist = new Skill<byte>(dist, 10);
            Shield = new Skill<byte>(shield, 10);
            Fish = new Skill<byte>(fish, 10);
        }
        public Skills(byte fist, uint fistExp, byte club, uint clubExp, byte sword, uint swordExp, byte axe, uint axeExp, byte dist, uint distExp, byte shield, uint shieldExp, byte fish, uint fishExp)
        {
            Fist = new Skill<byte>(fist, 10, fistExp);
            Club = new Skill<byte>(club, 10, clubExp);
            Sword = new Skill<byte>(sword, 10, swordExp);
            Axe = new Skill<byte>(axe, 10, axeExp);
            Dist = new Skill<byte>(dist, 10, distExp);
            Shield = new Skill<byte>(shield, 10, shieldExp);
            Fish = new Skill<byte>(fish, 10, fishExp);
        }
    }

    public class Equipment
    {
        public Item Armor;
        public Item Head;
        public Item Necklace;
        public Item Backpack;
        public Item Right;
        public Item Left;
        public Item Legs;
        public Item Feet;
        public Item Ring;
        public Item Ammunition;

        public Equipment(Item head, Item necklace, Item backpack, Item armor, Item right, Item left, Item legs, Item feet, Item ring, Item ammunition)
        {
            Armor = armor;
            Head = head;
            Necklace = necklace;
            Backpack = backpack;
            Right = right;
            Left = left;
            Legs = legs;
            Feet = feet;
            Ring = ring;
            Ammunition = ammunition;
        }

        public Item Get(EquipmentSlots slot)
        {
            switch (slot)
            {
                case EquipmentSlots.Head:
                    return Head;
                case EquipmentSlots.Necklace:
                    return Necklace;
                case EquipmentSlots.Backpack:
                    return Backpack;
                case EquipmentSlots.Armor:
                    return Armor;
                case EquipmentSlots.Right:
                    return Right;
                case EquipmentSlots.Left:
                    return Left;
                case EquipmentSlots.Legs:
                    return Legs;
                case EquipmentSlots.Feet:
                    return Feet;
                case EquipmentSlots.Ring:
                    return Ring;
                case EquipmentSlots.Ammunition:
                    return Ammunition;
                default:
                    return Item.None;
            }
        }
    }
}