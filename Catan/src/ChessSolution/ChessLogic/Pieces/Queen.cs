using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class Queen : Piece
    {
        public Queen(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
