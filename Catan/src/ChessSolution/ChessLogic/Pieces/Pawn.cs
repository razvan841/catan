using ChessLogic.Utilities;

namespace ChessLogic.Pieces
{
    public class Pawn : Piece
    {
        public Pawn(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(Position newPosition)
        {
            return true;
        }
    }
}
