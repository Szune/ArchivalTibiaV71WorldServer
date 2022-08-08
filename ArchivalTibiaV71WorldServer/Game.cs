using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Io;
using ArchivalTibiaV71WorldServer.Map;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer
{
    public class Game
    {
        private Game()
        {
        }

        public static Game CreateInstance()
        {
            return new();
        }

        public List<Player> LoadedPlayers = new List<Player>();

        public Dictionary<Position, Tile> Map = new Dictionary<Position, Tile>();

        public Position TemplePosition = (13, 13, 7);
        public const int MaxPlayers = 100;
        public int OnlinePlayersCount = 0;
        public List<Player> OnlinePlayers = new List<Player>(MaxPlayers);
        public List<Player> NewOnlinePlayers = new List<Player>();
        public readonly CreatureContainer Creatures = new CreatureContainer();
        public readonly CreatureContainer TemporaryCreatures = new CreatureContainer();
        private uint _newCreatureId = 1_000_001;
        private uint _newTemporaryCreatureId = 1_800_001;
        
        public readonly List<Tile> DecayingItems = new List<Tile>();

        public bool ReadMap()
        {
            const string mapName = "map/world.map";
            try
            {
                Console.Write($"- Loading {mapName}...");
                using var fs = File.OpenRead(mapName);
                var reader = new MapReader(fs);
                Tile tile;
                while ((tile = reader.Read())!= null)
                {
                    Map[tile.Position] = tile;
                }

                // when reading creatures here, start ids from _newCreatureId
                Console.WriteLine(" OK");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" -- Failed to load map {mapName}: {ex}");
                return false;
            }
        }

        public Tile GetTile(ushort x, ushort y, byte z)
        {
            if (Map.TryGetValue((x, y, z), out var tile))
            {
                //Console.WriteLine($"Found tile {tile.GroundId} at ({x},{y},{z})");
                return tile;
            }

            return null;
        }

        public void BroadcastFrom(Player player, string msg)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                OnlinePlayers[i].Packets.Message.PlayerBroadcast(player.Name, msg);
            }
        }

        public void GmSpawnItem(Position pos, ushort id)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, pos)) continue;
                OnlinePlayers[i].Packets.Map.ItemAppear(pos, IoC.Items.Create(id));
            }
        }

        public void GmSpawnMonster(Player player, string monsterName)
        {
            Creature creature;
            if (monsterName.ToLower() == "vampire")
            {
                // monster ids start at 1_000_000
                var creaturePos = player.Position.Offset(PositionOffset.Down);
                if (GetCreaturesOnTile(creaturePos) != null)
                    return;
                creature = new Creature(_newTemporaryCreatureId++, "vampire",
                    new Outfit(Outfits.Vampire), creaturePos,
                    creaturePos, 150, 150, 0, 0, 1_000_000, 30, 0, 0, 150, CreatureFlags.Temporary);
                creature.ZIndex = GetCreatureZIndex(creature, creaturePos);
                TemporaryCreatures.Add(creature);
            }
            else
            {
                player.Packets.Message.ServerBroadcast($"Failed to spawn monster: '{monsterName}' could not be found.");
                return;
            }

            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, player.Position)) continue;
                OnlinePlayers[i].Packets.Map.CreatureAppear(creature);
            }
        }

        public Creature GetFirstPlayerOnTile(Position position)
        {
            var c = OnlinePlayers.Count;
            for (var i = 0; i < c; i++)
            {
                var op = OnlinePlayers[i];
                if (!op.Connection.Connected) continue;
                if (op.Hitpoints < 1) continue;
                if (!Position.Equals(op.Position, position)) continue;
                return op;
            }
            return Creature.None;
        }

        public void PlayerExhausted(Player player)
        {
            player.Packets.Message.Status("You are exhausted.");
            var c = IoC.Game.OnlinePlayers.Count;
            for(int i = 0; i < c; i++)
            {
                var playerOnScreen = IoC.Game.OnlinePlayers[i];
                if (!playerOnScreen.Connection.Connected)
                    continue;
                if (Position.SameScreen(player.Position, IoC.Game.OnlinePlayers[i].Position))
                {
                    playerOnScreen.Packets.Effects.Exhausted(playerOnScreen);
                }
            }
        }

        public void RuneTargetAttack(Creature attacker, Creature target, byte projectileId, byte magicId)
        {
            if (target.IsDead || attacker.IsDead || !attacker.CanAttack)
            {
                return;
            }

            var damage = Formulae.Magic(attacker.Level, attacker.GetMagicStrength(), target.GetMagicDefense());
            
            damage = target.Damage(damage);

            if (target is Player p)
            {
                p.Packets.Stats();
            }
            
            attacker.AttackWait();
            
            Item corpse = Item.None;
            
            if (target.IsDead)
            {
                corpse = CreatureKilled(attacker, target);
            }
            
            var c = IoC.Game.OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                var playerOnScreen = IoC.Game.OnlinePlayers[i];
                if (!playerOnScreen.Connection.Connected)
                    continue;
                if (Position.SameScreen(target.Position, IoC.Game.OnlinePlayers[i].Position))
                {
                    playerOnScreen.Packets.Effects.Projectile(attacker.Position, target.Position, projectileId);
                    playerOnScreen.Packets.Effects.Magic(target.Position, magicId);
                    playerOnScreen.Packets.Message.Animated(target.Position, Colors.Red, damage.ToString());
                    playerOnScreen.Packets.Creature.UpdateHealth(target);
                    if (target.IsDead)
                    {
                        playerOnScreen.Packets.Message.Animated(attacker.Position, Colors.White, target.Experience.ToString());
                        playerOnScreen.Packets.Map.CreatureDisappear(target);
                        if (!corpse.IsNone)
                        {
                            playerOnScreen.Packets.Map.ItemAppear(target.Position, corpse);
                        }
                    }
                }
            }
        }

        public void MeleeAttack(Creature attacker, Creature target)
        {
            if (target.IsDead || attacker.IsDead || !attacker.CanAttack || !IoC.Game.AreAdjacent(attacker, target))
            {
                return;
            }

            var damage = Formulae.Melee(attacker.Level,
                attacker.GetMeleeStrength(),
                attacker.GetWeaponDamage(),
                target.GetMeleeDefense());
            
            damage = target.Damage(damage);
            
            if (target is Player p)
            {
                p.Packets.Stats();
            }
            
            attacker.AttackWait();

            Item corpse = Item.None;

            if (target.IsDead)
            {
                corpse = CreatureKilled(attacker, target);
            }
            
            var c = IoC.Game.OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                var playerOnScreen = IoC.Game.OnlinePlayers[i];
                if (!playerOnScreen.Connection.Connected)
                    continue;
                if (Position.SameScreen(target.Position, IoC.Game.OnlinePlayers[i].Position))
                {
                    playerOnScreen.Packets.Effects.MeleeSplash(target);
                    playerOnScreen.Packets.Message.Animated(target.Position, Colors.Red, damage.ToString());
                    playerOnScreen.Packets.Creature.UpdateHealth(target);
                    if (target.IsDead)
                    {
                        playerOnScreen.Packets.Message.Animated(attacker.Position, Colors.White, target.Experience.ToString());
                        playerOnScreen.Packets.Map.CreatureDisappear(target);
                        if (!corpse.IsNone)
                        {
                            playerOnScreen.Packets.Map.ItemAppear(target.Position, corpse);
                        }
                    }
                }
            }
        }

        private static Item CreatureKilled(Creature attacker, Creature target)
        {
            if (target.Flags.HasFlag(CreatureFlags.Temporary))
            {
                IoC.Game.TemporaryCreatures.Remove(target);
            }

            attacker.StopTargeting();
            if (attacker is Player p && target is not Player)
            {
                p.AddExperience(target.Experience);
            }

            // TODO: get corpse + loot stuff 
            var corpse = IoC.Items.CreateWithFlags(2170, ItemTypeFlags.Decaying);
            IoC.Game.Map[target.Position].AddItem(corpse);
            IoC.Game.DecayingItems.Add(IoC.Game.Map[target.Position]);
            return corpse;
        }

        public List<Creature> GetCreaturesOnTile(Position position, Creature skip = default)
        {
            List<Creature> creatures = null; // lazy initialize because this is going to be run often
            var c = OnlinePlayers.Count;
            for (var i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (OnlinePlayers[i].Hitpoints < 1) continue;
                if (OnlinePlayers[i].Id.Equals(skip?.Id)) continue;
                if (!Position.Equals(OnlinePlayers[i].Position, position)) continue;
                creatures ??= new List<Creature>(9);
                creatures.Add(OnlinePlayers[i]);
            }

            for (var i = 0; i < Creatures.Count; i++)
            {
                if (Creatures[i].Hitpoints < 1) continue;
                if (!Position.Equals(Creatures[i].Position, position)) continue;
                creatures ??= new List<Creature>(9);
                creatures.Add(Creatures[i]);
            }
            
            for (var i = 0; i < TemporaryCreatures.Count; i++)
            {
                if (TemporaryCreatures[i].Hitpoints < 1) continue;
                if (!Position.Equals(TemporaryCreatures[i].Position, position)) continue;
                creatures ??= new List<Creature>(9);
                creatures.Add(TemporaryCreatures[i]);
            }

            return creatures;
        }

        public void CreatureTurned(Player player)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, player.Position)) continue;
                OnlinePlayers[i].Packets.Map.CreatureTurn(player);
            }
        }

        public void CreatureMoved(Player player, Position oldPos, Position newPos, byte newZIndex)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (OnlinePlayers[i].Id == player.Id) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, player.Position)) continue;
                OnlinePlayers[i].Packets.Map.CreatureMoved(oldPos, newPos, newZIndex);
            }
        }

        public void CreatureDisappear(Player player, bool isLogout)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (OnlinePlayers[i].Id == player.Id) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, player.Position)) continue;
                OnlinePlayers[i].Packets.Map.CreatureDisappear(player);
                if (isLogout)
                    OnlinePlayers[i].Packets.Effects.Logout(player);
            }
        }

        public void GmSpawnItemWithCount(Position pos, ushort id, byte count)
        {
            var c = OnlinePlayers.Count;
            for (int i = 0; i < c; i++)
            {
                if (!OnlinePlayers[i].Connection.Connected) continue;
                if (!Position.SameScreen(OnlinePlayers[i].Position, pos)) continue;
                OnlinePlayers[i].Packets.Map.ItemAppear(pos, IoC.Items.Create(id, count));
            }
        }

        /// <summary>
        /// Gets creature whether they are dead or alive
        /// </summary>
        public Creature GetCreatureById(uint id)
        {
            if (Creatures.TryGetValue(id, out var cr))
                return cr;
            if (TemporaryCreatures.TryGetValue(id, out cr))
                return cr;
            var player = OnlinePlayers.SingleOrDefault(p => p.Id == id);
            return player ?? Creature.None;
        }
        
        public byte GetCreatureZIndexLogin(Creature creature, Position pos)
        {
            if (Map.TryGetValue(pos, out var tile))
            {
                var count = 0;
                if (tile.Ground != null) count += 1;
                var creatures = GetCreaturesOnTile(pos, creature);
                if (creatures != null) count += creatures.Count;
                if (tile.Items != null) count += tile.Items.Count;
                return (byte) count;
            }

            return 0;
        }

        public byte GetCreatureZIndex(Creature creature, Position pos)
        {
            if (Map.TryGetValue(pos, out var tile))
            {
                var count = 0;
                if (tile.Ground != null)
                {
                    count += 1;
                    count += tile.Items?.Count(it => IoC.Items.GetById(it.Id).AddsZIndex) ?? 0;
                }
                var creatures = GetCreaturesOnTile(pos, creature);
                if (creatures != null) count += creatures.Count;
                return (byte) count;
            }

            return 0;
        }

        public bool AreAdjacent(Creature player, Creature creature)
        {
            if (creature.IsNone || creature.Hitpoints < 1)
                return false;
            if (player.Position.Z != creature.Position.Z)
                return false;
            var xDiff = Math.Abs(player.Position.X - creature.Position.X);
            var yDiff = Math.Abs(player.Position.Y - creature.Position.Y);
            return xDiff < 2 && yDiff < 2;
        }

        public bool LoadCharacters()
        {
            Console.WriteLine("- Loading players...");
            var files = new DirectoryInfo("players").GetFiles("*.player");
            var reader = new CharacterReader();
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var player = reader.Read(files[i].FullName);
                    if (player == null)
                        return false;
                    IoC.Game.LoadedPlayers.Add(player);
                    
                    Console.WriteLine($" > Loaded players/{files[i].Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" -- Failed to load players/{files[i].Name}: {ex}");
                    return false;
                }
            }

            return true;
        }
    }
}