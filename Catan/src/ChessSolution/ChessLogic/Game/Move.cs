using ChessLogic.Utilities;

namespace ChessLogic.Game
{
    public class Move
    {
        public Position From { get; }
        public Position To { get; }

        public Move(Position from, Position to)
        {
            From = from;
            To = to;
        }
    }
}
