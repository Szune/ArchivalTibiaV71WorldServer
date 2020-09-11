using System;
using System.Linq;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Scripting;

namespace ArchivalTibiaV71WorldServer
{
    public static class GmCommands
    {
        private static readonly string[] EmptyArgs = new string[0];
        public const string CommandBroadcast = "!bc";
        public const string CommandSpawnItem = "!spawnitem";
        public const string CommandSpawnMonster = "!spawnmob";
        public const string CommandReloadScripts = "!reload";
        public const string CommandSetSpeed = "!speed";
        public const string CommandSetOutfit = "!outfit";

        public static bool Parse(Player player, string message)
        {
            var command = message.Split(' ').FirstOrDefault();
            var msg = "";

            switch (command)
            {
                case CommandReloadScripts:
                    try
                    {
                        ServerScript.LoadScript.Reload();
                        ServerScript.LoadScript.Execute(new ScriptGlobals());
                        player.Packets.Message.ServerBroadcast("Reloaded scripts.");
                    }
                    catch
                    {
                        player.Packets.Message.ServerBroadcast("Failed to reload script.");
                    }

                    return true;
                case CommandBroadcast:
                    msg = GetArgument(CommandBroadcast, message);
                    Game.Instance.BroadcastFrom(player, msg);
                    return true;
                case CommandSetOutfit:
                    SetOutfit(player, message);
                    return true;
                case CommandSpawnItem:
                    SpawnItem(player, message);
                    return true;
                case CommandSetSpeed:
                    msg = GetArgument(CommandSetSpeed, message);
                    if (ushort.TryParse(msg, out var newSpeed))
                    {
                        player.Speed.Set(newSpeed);
                        player.Packets.Creature.UpdateSpeed(player);
                    }

                    return true;
                case CommandSpawnMonster:
                    msg = GetArgument(CommandSpawnMonster, message);
                    Game.Instance.GmSpawnMonster(player, msg);
                    return true;
                case "!ll":
                    msg = message.Substring("!ll ".Length);
                    if (byte.TryParse(msg, out var ll))
                    {
                        player.SetLightLevel(ll);
                        player.Packets.Creature.UpdateLight(player);
                    }
                    else
                    {
                        player.Packets.Message.ServerBroadcast(
                            $"Failed to set light level: '{msg}' is not a valid byte.");
                    }

                    return true;
                case "!lc":
                    msg = message.Substring("!lc ".Length);
                    if (byte.TryParse(msg, out var lc))
                    {
                        player.SetLightColor(lc);
                        player.Packets.Creature.UpdateLight(player);
                    }
                    else
                    {
                        player.Packets.Message.ServerBroadcast(
                            $"Failed to set light color: '{msg}' is not a valid byte.");
                    }

                    return true;
                case "!light":
                    player.SetLightLevel(140);
                    player.SetLightColor(15);
                    player.Packets.Creature.UpdateLight(player);
                    return true;
                case "!pos":
                    player.Packets.Message.LookAt(
                        $"Your position is ({player.Position.X}, {player.Position.Y}, {player.Position.Z}).");
                    return true;
                case "!nolight":
                    player.SetLightLevel(player.Outfit.LightLevel);
                    player.SetLightColor(player.Outfit.LightColor);
                    player.Packets.Creature.UpdateLight(player);
                    return true;
                default:
                    if (message.StartsWith("!"))
                    {
                        //player.Packets.Message.ServerBroadcast($"Command not found: '{msg}'");
                        return true;
                    }

                    return false;
            }
        }

        private static string[] GetArguments(string command, string message)
        {
            if (command.Length + 1 > message.Length)
                return EmptyArgs;
            return message.Substring(command.Length + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string GetArgument(string command, string message)
        {
            if (command.Length + 1 > message.Length)
                return "";
            else
                return message.Substring(command.Length + 1);
        }

        private static void SetOutfit(Player player, string message)
        {
            var arg = GetArgument(CommandSetOutfit, message);
            if (Enum.TryParse<Outfits>(arg, true, out var outfit))
            {
                player.Outfit.Set(outfit);
                player.Packets.Creature.UpdateOutfit(player);
            }
        }

        private static void SpawnItem(Player player, string msg)
        {
            var args = GetArguments(CommandSpawnItem, msg);
            switch (args.Length)
            {
                case 0:
                    player.Packets.Message.ServerBroadcast($"Failed to spawn item: no argument.");
                    break;
                case 1:
                {
                    if (ushort.TryParse(args[0], out var id))
                    {
                        Game.Instance.GmSpawnItem(player.Position, id);
                    }
                    else
                    {
                        player.Packets.Message.ServerBroadcast(
                            $"Failed to spawn item: {args[0]} is not a valid item id.");
                    }

                    break;
                }
                case 2:
                {
                    if (ushort.TryParse(args[0], out var id))
                    {
                        if (byte.TryParse(args[1], out var count))
                        {
                            Game.Instance.GmSpawnItemWithCount(player.Position, id, count);
                        }
                        else
                        {
                            player.Packets.Message.ServerBroadcast(
                                $"Failed to spawn item: {args[1]} is not a valid byte.");
                        }
                    }
                    else
                    {
                        player.Packets.Message.ServerBroadcast(
                            $"Failed to spawn item: {args[0]} is not a valid item id.");
                    }

                    break;
                }
                default:
                    player.Packets.Message.ServerBroadcast($"Failed to spawn item: too many arguments.");
                    break;
            }
        }
    }
}