using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ArchivalTibiaV71WorldServer.Extensions;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer
{
    public static class NetworkCharacterLogin
    {
        private static void WorldServerSocketAccept()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Any, 7172);
            socket.Bind(endpoint);
            socket.Listen(0);
            while (true)
            {
                try
                {
                    Console.WriteLine("[Waiting for character logins]");
                    var connection = socket.Accept();
                    if (IoC.Game.OnlinePlayers.Count >= Game.MaxPlayers)
                    {
                        connection.SendSorryMessageBox("Too many players online.");
                        connection.Close(1000);
                        continue;
                    }

                    var buffer = new byte[1024 * 1];
                    int received = connection.Receive(buffer);
                    if (received > 0)
                    {
                        var reader = new PacketReader(buffer);
                        reader.ReadU16(); // skip length, lazy
                        if ((Packets.ReceiveFromClient) reader.ReadU8() == Packets.ReceiveFromClient.CharacterLogin)
                        {
                            if (!HandleCharacterLogin(connection, reader))
                            {
                                connection.Close();
                                Console.WriteLine(" -- Login failed");
                            }
                        }
                        else
                        {
                            connection.Close();
                            Console.WriteLine(" -- Login failed: bad packet id");
                        }
                    }
                }
                catch
                {
                    // accept new socket
                }
            }
            // I want people to be able to login
            // ReSharper disable once FunctionNeverReturns
        }

        private static bool HandleCharacterLogin(Socket connection, PacketReader reader)
        {
            // character login packet
            Console.WriteLine("[Received character login packet]");
            var os = reader.ReadU16();
            var client = reader.ReadU16();
            var gmArg = reader.ReadU8();
            var characterName = reader.ReadString();
            var password = reader.ReadString();
            #if DEBUG
            Console.WriteLine(
                $">Character information:\nName: {characterName}\nPassword: {password}\nClient: {client}\nOs: {os}\nGM arg: {gmArg}");
            #endif
            
            var character = IoC.Game.LoadedPlayers.SingleOrDefault(p => p.Name == characterName && p.Password == password);
            if (character == null)
            {
                Console.WriteLine($" -- Failed to login to character {characterName}");
                return false;
            }

            if (IoC.Game.OnlinePlayersCount >= Game.MaxPlayers)
            {
                connection.SendSorryMessageBox("Too many players online.");
                connection.Close(1000);
                return false;
            }

            Console.WriteLine($" > Character {characterName} logged in");
            character.SetPreConnection(connection);
            IoC.Game.NewOnlinePlayers.Add(character);
            NetworkOnline.LoginWait.Set();
            return true;
        }

        public static void InitializeWorldServerThread()
        {
            var thread = new Thread(WorldServerSocketAccept);
            thread.Start();
        }
    }
}