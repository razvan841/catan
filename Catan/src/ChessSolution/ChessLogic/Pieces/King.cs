using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class King : Piece
    {
        public King(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
