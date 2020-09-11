using System;
using ArchivalTibiaV71WorldServer.Constants;

namespace ArchivalTibiaV71WorldServer
{
    public class Position
    {
        public Position(ushort x, ushort y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public ushort X { get; }
        public ushort Y { get; }
        public byte Z { get; }

        public static bool Equals(Position a, Position b)
        {
            return a.Z == b.Z &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }

        public static implicit operator Position((ushort x, ushort y, byte z) pos)
        {
            return new Position(pos.x, pos.y, pos.z);
        }

        public static Position operator +(Position a, Position b)
        {
            return new Position((ushort) (a.X + b.X), (ushort) (a.Y + b.Y), (byte) (a.Z + b.Z));
        }

        public static Position operator +(Position a, PositionOffset b)
        {
            return new Position((ushort) (a.X + b.X), (ushort) (a.Y + b.Y), (byte) (a.Z + b.Z));
        }

        public Position Offset(PositionOffset offset)
        {
            return new Position((ushort) (X + offset.X), (ushort) (Y + offset.Y), (byte) (Z + offset.Z));
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public override int GetHashCode()
        {
            return (X, Y, Z).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position other))
            {
                return false;
            }

            return Equals(this, other);
        }

        public static bool SameFloorAndScreen(Position a, Position b)
        {
            return a.Z == b.Z && // if z are not the same, can't possibly on screen
                   Math.Abs(a.X - b.X) < GameClient.HalfWidth &&
                   Math.Abs(a.Y - b.Y) < GameClient.HalfHeight;
        }

        public static bool SameScreen(Position a, Position b)
        {
            return Math.Abs(a.X - b.X) < GameClient.HalfWidth &&
                   Math.Abs(a.Y - b.Y) < GameClient.HalfHeight;
        }
    }
}