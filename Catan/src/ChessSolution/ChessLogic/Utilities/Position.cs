namespace ChessLogic.Utilities
{
    public readonly struct Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
            => obj is Position other && X == other.X && Y == other.Y;

        public override int GetHashCode()
            => HashCode.Combine(X, Y);

        public static bool operator ==(Position a, Position b)
            => a.Equals(b);

        public static bool operator !=(Position a, Position b)
            => !a.Equals(b);

        public override string ToString()
            => $"({X},{Y})";
    }
}
