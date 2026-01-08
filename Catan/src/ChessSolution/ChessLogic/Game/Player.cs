using ChessLogic.Pieces;

namespace ChessLogic.Game
{
    public class Player
    {
        public PieceColor Color { get; }

        private readonly List<Piece> _pieces;
        public IReadOnlyCollection<Piece> Pieces => _pieces;

        public Player(PieceColor color)
        {
            Color = color;
            _pieces = new List<Piece>();
        }

        public void AddPiece(Piece piece)
        {
            _pieces.Add(piece);
        }

        public void RemovePiece(Piece piece)
        {
            _pieces.Remove(piece);
        }
        public King GetKing()
        {
            foreach (var p in _pieces)
                if (p is King king) return king;

            throw new InvalidOperationException("Player has no King!");
        }

    }
}
