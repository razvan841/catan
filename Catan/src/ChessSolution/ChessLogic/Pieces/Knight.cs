using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class Knight : Piece
    {
        public Knight(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
