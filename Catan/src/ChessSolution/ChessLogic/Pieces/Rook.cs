using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class Rook : Piece
    {
        public Rook(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
