using ChessLogic.Pieces;

namespace ChessLogic.Game
{
    public class Player
    {
        public bool IsWhite { get; }
        public List<Piece> Pieces { get; }

        public Player(bool isWhite)
        {
            IsWhite = isWhite;
            Pieces = new List<Piece>();
        }
    }
}
