using ChessLogic.Utilities;
using ChessLogic.Pieces;

namespace ChessLogic.Game
{
    public class Move
    {
        public Position From { get; }
        public Position To { get; }
        public Piece MovingPiece { get; }
        public Piece? CapturedPiece { get; }
        public bool IsCapture { get; }

        public Move(Position from, Position to, Piece movingPiece, Piece? capturedPiece = null)
        {
            From = from;
            To = to;
            MovingPiece = movingPiece;
            CapturedPiece = capturedPiece;
            IsCapture = capturedPiece != null;
        }
    }
}
