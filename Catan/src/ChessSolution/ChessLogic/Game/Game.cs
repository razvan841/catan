using ChessLogic.GameBoard;
using ChessLogic.Pieces;

namespace ChessLogic.Game
{
    public class Game
    {
        public Board Board { get; private set; }
        public Player WhitePlayer { get; }
        public Player BlackPlayer { get; }
        public PieceColor CurrentTurn { get; private set; }

        public Game()
        {
            Board = new Board();
            WhitePlayer = new Player(PieceColor.White);
            BlackPlayer = new Player(PieceColor.Black);
            InitializePieces();

            CurrentTurn = PieceColor.White;
        }

        private void InitializePieces()
        {

        }

        public bool MakeMove(Move move)
        {
            return true;
        }
        public bool IsCheck(PieceColor color)
        {
            return true;
        }
        public bool IsCheckmate(PieceColor color)
        {
            return true;
        }
        public bool IsStalemate(PieceColor color)
        {
            return true;
        }

        public IEnumerable<Move> GetLegalMoves(PieceColor color)
        {
            return null;
        }
    }
}
