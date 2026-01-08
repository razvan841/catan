using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public abstract class Piece
    {
        public bool IsWhite { get; }
        public Position Position { get; set; }

        protected Piece(bool isWhite)
        {
            IsWhite = isWhite;
        }

        public abstract bool IsValidMove(Position newPosition);
    }
}
