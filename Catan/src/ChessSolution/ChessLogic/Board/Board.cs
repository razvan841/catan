using ChessLogic.Pieces;
using ChessLogic.Utilities;

namespace ChessLogic.GameBoard
{
    public class Board
    {
        public Tile[,] Tiles { get; private set; }

        public Board()
        {
            Tiles = new Tile[8,8];
            InitializeTiles();
        }

        private void InitializeTiles()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Tiles[x,y] = new Tile(new Position(x, y));
                }
            }
        }

        public Tile GetTile(Position position)
        {
            if (position.X < 0 || position.X > 7 || position.Y < 0 || position.Y > 7)
                throw new ArgumentOutOfRangeException(nameof(position));

            return Tiles[position.X, position.Y];
        }

        public Piece? GetPiece(Position position)
        {
            var tile = GetTile(position);
            return tile.CurrentPiece;
        }
        public void PlacePiece(Piece piece, Position position)
        {
            var tile = GetTile(position);
            tile.PlacePiece(piece);
            piece.Position = position;
        }

        public void RemovePiece(Position position)
        {
            var tile = GetTile(position);
            tile.RemovePiece();
        }
        public void MovePiece(Position from, Position to)
        {
            var fromTile = GetTile(from);
            var toTile = GetTile(to);

            var piece = fromTile.CurrentPiece;
            if (piece == null)
                throw new InvalidOperationException("No piece to move.");

            fromTile.RemovePiece();
            toTile.PlacePiece(piece);

            piece.Position = to;
            piece.HasMoved = true;
        }

        public bool IsPositionOccupied(Position position)
        {
            var tile = GetTile(position);
            return !tile.IsEmpty;
        }
        public bool IsPositionOccupiedByOpponent(Position position, PieceColor color)
        {
            var piece = GetPiece(position);
            return piece != null && piece.Color != color;
        }

    }
}
