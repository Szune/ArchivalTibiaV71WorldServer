using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Scripting;
using ArchivalTibiaV71WorldServer.Utilities;
using ArchivalTibiaV71WorldServer.World;

namespace ArchivalTibiaV71WorldServer
{
    static class Program
    {

        static void Main(string[] args)
        {
            new Thread(() => { _ = CSharpScript.Create("return null;").RunAsync().Result; }).Start();
            Console.Title = "ArchivalTibiaV71WorldServer";
            Console.WriteLine(".-----------------------------------.");
            Console.WriteLine("| Archival Tibia 7.1 World Server: Online |");
            Console.WriteLine("'-----------------------------------'");
            if (!Items.Instance.Load())
            {
                Console.ReadLine();
                return;
            }
            
            if (!Game.Instance.LoadCharacters())
            {
                Console.ReadLine();
                return;
            }

            if (!Game.Instance.ReadMap())
            {
                Console.ReadLine();
                return;
            }

            Console.WriteLine("- Loading scripts...");
            ServerScript.LoadScript.Execute(new ScriptGlobals());

            NetworkOnline.InitializeOnlinePlayersReceiveLoopThread();
            NetworkCharacterLogin.InitializeWorldServerThread();
        }


        private static void MoveObject(Socket connection, PacketReader reader, Player player)
        {
            var source = reader.ReadPosition();
            var itemId = reader.ReadU16();
            var knownItem = reader.ReadU8();
            var dest = reader.ReadPosition();
            var count = reader.ReadU8();
            if (itemId == 0x63) // moving creature
            {
                Console.WriteLine(
                    $"Move creature on top of ({source.X}, {source.Y}, {source.Z}) to ({dest.X}, {dest.Y}, {dest.Z})");
            }
            else if (source.X == 0xFFFF)
            {
                Console.WriteLine(
                    $"Move {count} items (id: {itemId}, known item: {knownItem}) from {(EquipmentSlots) source.Y} to ({dest.X}, {dest.Y}, {dest.Z})");
            }
            else if (dest.X == 0xFFFF)
            {
                Console.WriteLine(
                    $"Move {count} items (id: {itemId}, known item: {knownItem}) from ({source.X}, {source.Y}, {source.Z}) to {(EquipmentSlots) dest.Y}");
            }
            else
            {
                Console.WriteLine(
                    $"Move {count} items (id: {itemId}, known item: {knownItem}) from ({source.X}, {source.Y}, {source.Z}) to ({dest.X}, {dest.Y}, {dest.Z})");
            }

            player.Packets.Message.Status("throw new NotImplementedException();");
        }


        /// <summary>
        /// Only valid before attempting to login a character
        /// </summary>
        public enum DialogPacket : byte
        {
            Info = 0x08,
            Error = 0x0A,
            /// <summary>
            /// Message of the day
            /// </summary>
            Motd = 0x14,
        }
    }
}