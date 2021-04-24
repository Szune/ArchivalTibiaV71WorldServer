using System;
using ArchivalTibiaV71WorldServer.Constants;
using ArchivalTibiaV71WorldServer.Entities;
using ArchivalTibiaV71WorldServer.Utilities;

namespace ArchivalTibiaV71WorldServer.PacketHelpers
{
    public class MapHelper
    {
        private readonly Player _player;

        public MapHelper(Player player)
        {
            _player = player;
        }

        public void ItemAppear(Position pos, Item item)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.ItemOrCreatureAppearOnTile);
            builder.AddPosition(pos);
            builder.AddItem(item);
            builder.Send(_player);
        }

        public void CreatureTurn(Creature creature)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.CreatureTurn);
            builder.AddPosition(creature.Position);
            builder.AddU8(creature.ZIndex);
            builder.AddU16(0x63);
            builder.AddU32(creature.Id);
            builder.AddU8((byte) creature.Direction);
            builder.Send(_player);
        }

        public void CreatureDisappear(Creature creature)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.ItemOrCreatureDisappear);
            builder.AddPosition(creature.Position);
            builder.AddU8(creature.ZIndex);
            builder.Send(_player);
        }

        public void CreatureAppear(Creature creature)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.ItemOrCreatureAppearOnTile);
            builder.AddPosition(creature.Position);
            builder.AddCreature(_player, creature);
            builder.Send(_player);
        }

        public void AddFullMapToBuilder(PacketBuilder builder)
        {
            builder.AddPacketId(Packets.SendToClient.FullScreenMap);
            /* add player position */
            builder.AddPosition(_player.Position);
            /* add rest of map description */
            // AddMapDescriptionToBuilder(builder, 0, 0, GameClient.Width - 1, GameClient.Height - 1);
            AddMapDescriptionToBuilder(builder,
                (ushort) (_player.Position.X - 8),
                (ushort) (_player.Position.Y - 6),
                _player.Position.Z,
                GameClient.Width,
                GameClient.Height,false);
        }

        public void AddMapDescriptionToBuilder(PacketBuilder builder, ushort x, ushort y,
            byte z, int width, int height, bool isStep)
        {
            const int maxZ = 15;
            var skip = -1;
            byte startZ, endZ;
            short zStep;

            if (z > 7)
            {
                startZ = (byte) (_player.Position.Z - 2);
                endZ = (byte) (Math.Min(maxZ, z + 2));
                zStep = 1;
            }
            else
            {
                startZ = 7;
                endZ = 0;
                zStep = -1;
            }
            // if (_player.Position.Z > 7)
            // {
            //     startz = (byte) (_player.Position.Z - 2);
            //     endz = (byte) (Math.Min(maxZ, _player.Position.Z + 2));
            //     zstep = 1;
            // }
            // else
            // {
            //     startz = 7;
            //     endz = 0;
            //     zstep = -1;
            // }

            // short x = (short) (_player.Position.X - 8);
            // short y = (short) (_player.Position.Y - 6);
            // byte z = _player.Position.Z;
            for (short nz = startZ;
                nz != endZ + zStep;
                nz += zStep) // +/- 2 of player z (but no higher than max Z or lower than min Z)
            {
                AddFloorDescriptionToBuilder(builder, x, y, (byte) nz, width, height, (short) (z - nz), isStep, ref skip);
            }

            if (skip < 0) return;
            builder.AddU8((byte) skip);
            builder.AddU8(0xFF);
        }

        private void AddFloorDescriptionToBuilder(PacketBuilder builder, ushort x, ushort y, byte z, int width,
            int height, short offset, bool isStep, ref int skip)
        {
            /* add floor description */
            for (ushort nx = 0; nx < width; nx++)
            {
                for (ushort ny = 0; ny < height; ny++)
                {
                    // var currentTile = new Position((ushort) (x + nx + offset), (ushort) (y + ny + offset),
                    //     (byte) nz);
                    var tile = Game.Instance.GetTile(
                        (ushort) (x + nx + offset),
                        (ushort) (y + ny + offset),
                        z);
                    if (tile != null)
                    {
                        if (skip >= 0)
                        {
                            builder.AddU8((byte) skip);
                            builder.AddU8(0xFF);
                        }

                        skip = 0;

                        AddTileDescriptionToBuilder(builder, tile, isStep);
                    }
                    else
                    {
                        skip += 1;
                        if (skip == 0xFF)
                        {
                            builder.AddU8(0xFF);
                            builder.AddU8(0xFF);
                            skip = -1;
                        }
                    }
                }
            }
        }

        private void AddTileDescriptionToBuilder(PacketBuilder builder, Tile tile, bool isStep)
        {
            var count = 0;
            if (tile.Ground != null)
            {
                builder.AddItem(tile.Ground);
                count++;
            }

            if (tile.Items?.Count > 0)
            {
                for (int i = tile.Items.Count - 1; i > -1 && count < 10; i--)
                {
                    builder.AddItem(tile.Items[i]);
                    count++;
                }
            }

            var creatures = Game.Instance.GetCreaturesOnTile(tile.Position);
            if (creatures == null) return;
            for (int i = 0; i < creatures.Count && count < 10; i++)
            {
                if(creatures[i].Id == _player.Id && isStep)
                    builder.AddPlayerMove(_player);
                else
                    builder.AddCreature(_player, creatures[i]);
                count++;
            }
        }

        //     if (!tile.skip)
        //     {
        //         if (skip >= 0)
        //         {
        //             builder.AddU8((byte) skip);
        //             builder.AddU8(0xFF);
        //         }
        //
        //         skip = 0;
        //         var count = 0;
        //         if (tile.t.GroundId > 0)
        //         {
        //             builder.AddU16(tile.t.GroundId);
        //             count++;
        //         }
        //
        //         if (tile.t.Items?.Count > 0)
        //         {
        //             foreach (var t in tile.t.Items)
        //             {
        //                 if (count > 9)
        //                     break;
        //                 builder.AddItem(t);
        //                 count++;
        //             }
        //         }
        //
        //         var creatures = Game.Instance.GetCreaturesOnTile(currentTile);
        //
        //         if (creatures == null) continue;
        //         foreach (var creature in creatures)
        //         {
        //             if (count > 9)
        //                 break;
        //             builder.AddCreature(_player, creature);
        //             count++;
        //         }
        //     }
        //     else
        //     {
        //         skip++;
        //         if (skip != 0xFF) continue;
        //         builder.AddU8(0xFF);
        //         builder.AddU8(0xFF);
        //         skip = -1;
        //     }
        // }


        public void CreatureMoved(Position oldPos, Position newPos, byte newZIndex)
        {
            var builder = new PacketBuilder();
            builder.AddPacketId(Packets.SendToClient.CreaturePositionUpdate);
            builder.AddPosition(oldPos);
            builder.AddU8(newZIndex); // not 100% sure if it should be new or old z-index
            builder.AddPosition(newPos);
            builder.Send(_player);
        }
    }
}