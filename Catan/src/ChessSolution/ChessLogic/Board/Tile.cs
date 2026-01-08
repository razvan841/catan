using ChessLogic.Pieces;
using ChessLogic.Utilities;

namespace ChessLogic.GameBoard
{
    public class Tile
    {
        public Position Position { get; }
        public Piece? CurrentPiece { get; set; }
        public bool IsEmpty => CurrentPiece == null;

        public Tile(Position position)
        {
            Position = position;
            CurrentPiece = null;
        }
        public void PlacePiece(Piece piece)
        {
            CurrentPiece = piece;
        }
        public void RemovePiece()
        {
            CurrentPiece = null;
        }
    }
}
