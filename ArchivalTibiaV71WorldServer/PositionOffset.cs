namespace ArchivalTibiaV71WorldServer
{
    public class PositionOffset
    {
        public static readonly PositionOffset Up = (0, -1, 0);
        public static readonly PositionOffset Right = (1, 0, 0);
        public static readonly PositionOffset Down = (0, 1, 0);
        public static readonly PositionOffset Left = (-1, 0, 0);
        public short X { get; }
        public short Y { get; }
        public short Z { get; }

        public PositionOffset(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public static implicit operator PositionOffset((short x, short y, short z) pos)
        {
            return new PositionOffset(pos.x, pos.y, pos.z);
        }
        
    }
}