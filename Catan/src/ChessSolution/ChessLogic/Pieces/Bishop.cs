using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class Bishop : Piece
    {
        public Bishop(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
