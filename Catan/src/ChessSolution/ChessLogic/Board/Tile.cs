using ChessLogic.Pieces;

namespace ChessLogic.Board
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public Piece? CurrentPiece { get; set; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            CurrentPiece = null;
        }
    }
}
