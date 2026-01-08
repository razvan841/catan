using ChessLogic.Board;
using ChessLogic.Pieces;

namespace ChessLogic.Game
{
    public class Game
    {
        public Board.Board Board { get; private set; }
        public Player WhitePlayer { get; }
        public Player BlackPlayer { get; }
        public bool IsWhiteTurn { get; private set; }

        public Game()
        {
            Board = new Board.Board();
            WhitePlayer = new Player(true);
            BlackPlayer = new Player(false);
            IsWhiteTurn = true;
        }

        public bool MakeMove(Move move)
        {
            return true;
        }
    }
}
