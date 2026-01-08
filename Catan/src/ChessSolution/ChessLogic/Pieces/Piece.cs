using ChessLogic.Utilities;
using ChessLogic.GameBoard;

namespace ChessLogic.Pieces
{
    public abstract class Piece
    {
        public PieceColor Color { get; }
        public Position Position { get; set; }
        public bool HasMoved { get; set; }

        protected Piece(PieceColor color, Position position)
        {
            Color = color;
            Position = position;
        }

        public abstract IEnumerable<Position> GetLegalMoves(Board board);
        public abstract IEnumerable<Position> GetAttackSquares(Board board);

        public virtual void OnMove(Position newPosition)
        {
            
        }
    }
}
