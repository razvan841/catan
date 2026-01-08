using ChessLogic.Pieces;

namespace ChessLogic.Board
{
    public class Board
    {
        public Tile[,] Tiles { get; private set; }

        public Board()
        {
            Tiles = new Tile[8,8];
            InitializeTiles();
            InitializePieces();
        }

        private void InitializeTiles()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Tiles[x,y] = new Tile(x, y);
                }
            }
        }

        public void InitializePieces(Piece piece, int x, int y)
        {

        }
        public void PlacePiece(Piece piece, int x, int y)
        {
            Tiles[x, y].CurrentPiece = piece;
            piece.Position = new Utilities.Position(x, y);
        }
    }
}
