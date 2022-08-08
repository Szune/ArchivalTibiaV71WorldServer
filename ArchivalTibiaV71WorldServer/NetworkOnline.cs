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
        private static readonly Dictionary<Packets.ReceiveFromClient, IPacketHandler> PacketHandlers = new();

        public static readonly List<ServerScript> OnPacketReceive = new();
        public static readonly List<ServerScript> OnUnknownPacket = new();

        public static ManualResetEvent LoginWait = new(false);

        static NetworkOnline()
        {
            PacketHandlers[Packets.ReceiveFromClient.Logout] = new LogoutPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.MoveUp] = new MoveUpPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.MoveRight] = new MoveRightPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.MoveDown] = new MoveDownPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.MoveLeft] = new MoveLeftPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.LookAt] = new LookAtPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.Message] = new MessagePacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.TurnUp] = new TurnUpPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.TurnRight] = new TurnRightPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.TurnDown] = new TurnDownPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.TurnLeft] = new TurnLeftPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.UseThing] = new UseThingPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.AttackChaseMode] = new UseThingPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.AutoWalkDirections] = new AutoWalkDirectionsPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.StopAutoWalk] = new StopAutoWalkPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.AttackTarget] = new AttackTargetPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.CloseContainer] = new CloseContainerPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.UseCrosshairThing] = new UseCrosshairThingPacketHandler();
            PacketHandlers[Packets.ReceiveFromClient.UseCrosshairThingOnCreature] = new UseItemOnCreaturePacketHandler();
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
                if (IoC.Game.OnlinePlayersCount < 1)
                {
                    LoginWait.Reset();
                    LoginWait.WaitOne();
                }

                var c = IoC.Game.OnlinePlayers.Count;
                for (int i = 0; i < c; i++)
                {
                    var p = IoC.Game.OnlinePlayers[i];
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
            for (int i = IoC.Game.DecayingItems.Count - 1; i > -1; i--)
            {
                // TODO: implement
                IoC.Game.DecayingItems[i].Decay(i);
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
                    var c = IoC.Game.GetCreatureById(p.TargetId);
                    if (!IoC.Game.AreAdjacent(p, c))
                        return;
                    IoC.Game.MeleeAttack(p, c);
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
            var online = IoC.Game.OnlinePlayers.Count(p => p.Connection.Connected);
            IoC.Game.OnlinePlayersCount = online;

            var newOnline = IoC.Game.NewOnlinePlayers.Count;

            // TODO: rewrite this huge method to be readable
            // add new online players
            if (newOnline > 0)
            {
                // let all players that have been cached use the same player objects as before
                for (int nOn = newOnline - 1; nOn > -1; nOn--)
                {
                    for (int cached = 0; cached < IoC.Game.OnlinePlayers.Count; cached++)
                    {
                        if (IoC.Game.OnlinePlayers[cached].Id == IoC.Game.NewOnlinePlayers[nOn].Id)
                        {
                            if (IoC.Game.OnlinePlayers[cached].Connection.Connected)
                            {
                                // Game.OnlinePlayers[cached].Packets.MessageBoxes
                                //     .Sorry("Someone else logged in on this character.");
                                try
                                {
                                    IoC.Game.OnlinePlayers[cached].Connection.Close(1000);
                                    IoC.Game.OnlinePlayers[cached]
                                        .SetConnection(IoC.Game.NewOnlinePlayers[nOn].PreConnection);
                                    IoC.Game.OnlinePlayers[cached].Packets.LoginSuccess();
                                    IoC.Game.NewOnlinePlayers.RemoveAt(nOn);
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
                                    IoC.Game.OnlinePlayers[cached]
                                        .SetConnection(IoC.Game.NewOnlinePlayers[nOn].PreConnection);
                                    IoC.Game.OnlinePlayers[cached].Packets.LoginSuccess();
                                    IoC.Game.NewOnlinePlayers.RemoveAt(nOn);
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

                newOnline = IoC.Game.NewOnlinePlayers.Count;
                if (newOnline < 1)
                {
                    online = IoC.Game.OnlinePlayers.Count(p => p.Connection.Connected);
                    IoC.Game.OnlinePlayersCount = online;
                    return;
                }

                if (IoC.Game.OnlinePlayers.Count + newOnline > Game.MaxPlayers)
                {
                    var lastAdded = 0;
                    for (int i = 0; i < IoC.Game.OnlinePlayers.Count; i++)
                    {
                        if (lastAdded == newOnline) // yay, everyone could login
                            break;
                        if (IoC.Game.OnlinePlayers[i].Connection.Connected) continue;
                        try
                        {
                            IoC.Game.OnlinePlayers[i] = IoC.Game.NewOnlinePlayers[lastAdded];
                            IoC.Game.OnlinePlayers[i].SetConnection(IoC.Game.OnlinePlayers[i].PreConnection);
                            IoC.Game.OnlinePlayers[i].Packets.LoginSuccess();
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
                            IoC.Game.NewOnlinePlayers[i].Connection.SendSorryMessageBox(
                                "Too many players online.");
                            IoC.Game.NewOnlinePlayers[i].Connection.Close(1000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    IoC.Game.OnlinePlayers.AddRange(IoC.Game.NewOnlinePlayers);
                    foreach (var p in IoC.Game.NewOnlinePlayers)
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

                IoC.Game.NewOnlinePlayers.Clear();
                online = IoC.Game.OnlinePlayers.Count(p => p.Connection.Connected);
                IoC.Game.OnlinePlayersCount = online;
            }

            // clear out duplicates :/
        }

        private static void HandleMessage(byte[] buffer, Player player)
        {
            var reader = new PacketReader(buffer);
            var length = reader.ReadU16();

            // could probably use memory<T> or span<T> here for better performance
            var packetId = (Packets.ReceiveFromClient) reader.ReadU8();
#if DEBUG
            Console.Write("Packet Id: ");
            Console.WriteLine(Enum.IsDefined(typeof(Packets.ReceiveFromClient), (byte) packetId)
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