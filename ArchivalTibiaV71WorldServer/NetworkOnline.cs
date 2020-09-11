using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ArchivalTibiaV71WorldServer.Extensions;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.PacketHandlers;
using ArchivalTibiaV71WorldServer.Scripting;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer
{
    public static class NetworkOnline
    {
        private static readonly Dictionary<Packets.Receive, IPacketHandler> PacketHandlers =
            new Dictionary<Packets.Receive, IPacketHandler>();

        public static readonly List<ServerScript> OnPacketReceive = new List<ServerScript>();
        public static readonly List<ServerScript> OnUnknownPacket = new List<ServerScript>();

        public static ManualResetEvent LoginWait = new ManualResetEvent(false);

        static NetworkOnline()
        {
            PacketHandlers[Packets.Receive.Logout] = new LogoutPacketHandler();
            PacketHandlers[Packets.Receive.MoveUp] = new MoveUpPacketHandler();
            PacketHandlers[Packets.Receive.MoveRight] = new MoveRightPacketHandler();
            PacketHandlers[Packets.Receive.MoveDown] = new MoveDownPacketHandler();
            PacketHandlers[Packets.Receive.MoveLeft] = new MoveLeftPacketHandler();
            PacketHandlers[Packets.Receive.LookAt] = new LookAtPacketHandler();
            PacketHandlers[Packets.Receive.Message] = new MessagePacketHandler();
            PacketHandlers[Packets.Receive.TurnUp] = new TurnUpPacketHandler();
            PacketHandlers[Packets.Receive.TurnRight] = new TurnRightPacketHandler();
            PacketHandlers[Packets.Receive.TurnDown] = new TurnDownPacketHandler();
            PacketHandlers[Packets.Receive.TurnLeft] = new TurnLeftPacketHandler();
            PacketHandlers[Packets.Receive.UseThing] = new UseThingPacketHandler();
            PacketHandlers[Packets.Receive.AttackChaseMode] = new UseThingPacketHandler();
            PacketHandlers[Packets.Receive.AutoWalkDirections] = new AutoWalkDirectionsPacketHandler();
            PacketHandlers[Packets.Receive.StopAutoWalk] = new StopAutoWalkPacketHandler();
            PacketHandlers[Packets.Receive.AttackTarget] = new AttackTargetPacketHandler();
            PacketHandlers[Packets.Receive.CloseContainer] = new CloseContainerPacketHandler();
        }

        public static void OnlinePlayersReceiveLoop()
        {
            // maybe move the new players thingy to another thread?
            // let this thread focus on processing data from players
            while (true)
            {
                // this is a busy loop if there are no players online!
                // add a manual reset event or something to indicate that the first player is online
                // and then start the loop
                // and if no one has been online for a while, let it sit until someone is on again

                // assign correct online players value (at least for this millisecond or so)
                HandleNewOnlinePlayers();
                if (Game.Instance.OnlinePlayersCount < 1)
                {
                    LoginWait.Reset();
                    LoginWait.WaitOne();
                }

                var c = Game.Instance.OnlinePlayers.Count;
                for (int i = 0; i < c; i++)
                {
                    var p = Game.Instance.OnlinePlayers[i];
                    if (!p.Connection.Connected)
                        continue;
                    ReceiveFromPlayer(p);

                    ProcessTargeting(p);
                    ProcessWalking(p);
                }

                ProcessDecaying();

                Thread.Sleep(1);
            }
            // I would love it if players could play
            // ReSharper disable once FunctionNeverReturns
        }

        private static void ProcessDecaying()
        {
            for (int i = Game.Instance.DecayingItems.Count - 1; i > -1; i--)
            {
                // TODO: implement
                Game.Instance.DecayingItems[i].Decay(i);
            }
        }

        private static void ProcessTargeting(Player p)
        {
            switch (p.TargetType)
            {
                case TargetType.NoTarget:
                    return;
                case TargetType.Attacking:
                    if (!p.CanAttack)
                        return;
                    var c = Game.Instance.GetCreatureById(p.TargetId);
                    if(!Game.Instance.AreAdjacent(p, c))
                        return;
                    p.Attack(c);
                    break;
                case TargetType.Following:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ProcessWalking(Player p)
        {
            if (!p.CanWalk) return;
            var step = p.TakeStep();
            switch (step)
            {
                case Directions.North:
                    p.Packets.MoveNorth();
                    break;
                case Directions.East:
                    p.Packets.MoveEast();
                    break;
                case Directions.South:
                    p.Packets.MoveSouth();
                    break;
                case Directions.West:
                    p.Packets.MoveWest();
                    break;
            }
        }

        private static void ReceiveFromPlayer(Player p)
        {
            try
            {
                // going to have to rewrite this to get all available bytes
                // otherwise some packets in packets are bound to be lost at some point
                if (p.Connection.Available <= 0) return;
                var buffer = new byte[1024 * 1];
                var received = p.Connection.Receive(buffer);
                if (received < 1) return;
                HandleMessage(buffer, p);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void HandleNewOnlinePlayers()
        {
            var online = Game.Instance.OnlinePlayers.Count(p => p.Connection.Connected);
            Game.Instance.OnlinePlayersCount = online;

            var newOnline = Game.Instance.NewOnlinePlayers.Count;

            // add new online players
            if (newOnline > 0)
            {
                // let all players that have been cached use the same player objects as before
                for (int nOn = newOnline - 1; nOn > -1; nOn--)
                {
                    for (int cached = 0; cached < Game.Instance.OnlinePlayers.Count; cached++)
                    {
                        if (Game.Instance.OnlinePlayers[cached].Id == Game.Instance.NewOnlinePlayers[nOn].Id)
                        {
                            if (Game.Instance.OnlinePlayers[cached].Connection.Connected)
                            {
                                // Game.OnlinePlayers[cached].Packets.MessageBoxes
                                //     .Sorry("Someone else logged in on this character.");
                                try
                                {
                                    Game.Instance.OnlinePlayers[cached].Connection.Close(1000);
                                    Game.Instance.OnlinePlayers[cached]
                                        .SetConnection(Game.Instance.NewOnlinePlayers[nOn].PreConnection);
                                    Game.Instance.OnlinePlayers[cached].Packets.LoginSuccess();
                                    Game.Instance.NewOnlinePlayers.RemoveAt(nOn);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                            else
                            {
                                try
                                {
                                    Game.Instance.OnlinePlayers[cached]
                                        .SetConnection(Game.Instance.NewOnlinePlayers[nOn].PreConnection);
                                    Game.Instance.OnlinePlayers[cached].Packets.LoginSuccess();
                                    Game.Instance.NewOnlinePlayers.RemoveAt(nOn);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }

                            break;
                        }
                    }
                }

                newOnline = Game.Instance.NewOnlinePlayers.Count;
                if (newOnline < 1)
                {
                    online = Game.Instance.OnlinePlayers.Count(p => p.Connection.Connected);
                    Game.Instance.OnlinePlayersCount = online;
                    return;
                }

                if (Game.Instance.OnlinePlayers.Count + newOnline > Game.MaxPlayers)
                {
                    var lastAdded = 0;
                    for (int i = 0; i < Game.Instance.OnlinePlayers.Count; i++)
                    {
                        if (lastAdded == newOnline) // yay, everyone could login
                            break;
                        if (Game.Instance.OnlinePlayers[i].Connection.Connected) continue;
                        try
                        {
                            Game.Instance.OnlinePlayers[i] = Game.Instance.NewOnlinePlayers[lastAdded];
                            Game.Instance.OnlinePlayers[i].SetConnection(Game.Instance.OnlinePlayers[i].PreConnection);
                            Game.Instance.OnlinePlayers[i].Packets.LoginSuccess();
                            lastAdded++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    for (int i = lastAdded; i < newOnline; i++)
                    {
                        try
                        {
                            Game.Instance.NewOnlinePlayers[i].Connection.SendSorryMessageBox(
                                "Too many players online.");
                            Game.Instance.NewOnlinePlayers[i].Connection.Close(1000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    Game.Instance.OnlinePlayers.AddRange(Game.Instance.NewOnlinePlayers);
                    foreach (var p in Game.Instance.NewOnlinePlayers)
                    {
                        try
                        {
                            p.SetConnection(p.PreConnection);
                            p.Packets.LoginSuccess();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                Game.Instance.NewOnlinePlayers.Clear();
                online = Game.Instance.OnlinePlayers.Count(p => p.Connection.Connected);
                Game.Instance.OnlinePlayersCount = online;
            }

            // clear out duplicates :/
        }

        private static void HandleMessage(byte[] buffer, Player player)
        {
            var reader = new PacketReader(buffer);
            var length = reader.ReadU16();

            // could probably use memory<T> or span<T> here for better performance
            var packetId = (Packets.Receive) reader.ReadU8();
            #if DEBUG
            Console.Write("Packet Id: ");
            Console.WriteLine(Enum.IsDefined(typeof(Packets.Receive), (byte) packetId)
                ? packetId.ToString()
                : $"0x{(byte) packetId:X2}");
            #endif
            if (PacketHandlers.TryGetValue(packetId, out var handler))
            {
                handler.Handle(player, reader);
            }
            else
            {
                Console.WriteLine(
                    $"Unknown packet from {player.Name}: {buffer.AsSpan(0, length + 2).ToHex()}");
                for (var i = 0; i < OnUnknownPacket.Count; i++)
                {
                    OnUnknownPacket[i]
                        .Execute(new ScriptGlobals(player, (byte) packetId, length, new PacketReader(buffer)));
                }
            }

            for (var i = 0;
                i < OnPacketReceive.Count;
                i++)
            {
                OnPacketReceive[i]
                    .Execute(new ScriptGlobals(player, (byte) packetId, length, new PacketReader(buffer)));
            }
        }

        public static void InitializeOnlinePlayersReceiveLoopThread()
        {
            var thread = new Thread(OnlinePlayersReceiveLoop);
            thread.Start();
        }
    }
}