using ChessLogic.GameBoard;
using ChessLogic.Pieces;
using ChessLogic.Utilities;

namespace ChessLogic.Game
{
    public class Game
    {
        public Board Board { get; }
        public Player WhitePlayer { get; }
        public Player BlackPlayer { get; }
        public PieceColor CurrentTurn { get; private set; }
        private Move? _lastMove;

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
            for (int x = 0; x < 8; x++)
            {
                var whitePawn = new Pawn(PieceColor.White, new Position(x, 6));
                var blackPawn = new Pawn(PieceColor.Black, new Position(x, 1));

                AddPiece(whitePawn, WhitePlayer);
                AddPiece(blackPawn, BlackPlayer);
            }

            AddPiece(new Rook(PieceColor.White, new Position(0, 7), false), WhitePlayer);
            AddPiece(new Rook(PieceColor.White, new Position(7, 7), true), WhitePlayer);
            AddPiece(new Rook(PieceColor.Black, new Position(0, 0), false), BlackPlayer);
            AddPiece(new Rook(PieceColor.Black, new Position(7, 0), true), BlackPlayer);

            AddPiece(new Knight(PieceColor.White, new Position(1, 7)), WhitePlayer);
            AddPiece(new Knight(PieceColor.White, new Position(6, 7)), WhitePlayer);
            AddPiece(new Knight(PieceColor.Black, new Position(1, 0)), BlackPlayer);
            AddPiece(new Knight(PieceColor.Black, new Position(6, 0)), BlackPlayer);

            AddPiece(new Bishop(PieceColor.White, new Position(2, 7)), WhitePlayer);
            AddPiece(new Bishop(PieceColor.White, new Position(5, 7)), WhitePlayer);
            AddPiece(new Bishop(PieceColor.Black, new Position(2, 0)), BlackPlayer);
            AddPiece(new Bishop(PieceColor.Black, new Position(5, 0)), BlackPlayer);

            AddPiece(new Queen(PieceColor.White, new Position(3, 7)), WhitePlayer);
            AddPiece(new Queen(PieceColor.Black, new Position(3, 0)), BlackPlayer);

            AddPiece(new King(PieceColor.White, new Position(4, 7)), WhitePlayer);
            AddPiece(new King(PieceColor.Black, new Position(4, 0)), BlackPlayer);
        }

        private void AddPiece(Piece piece, Player player)
        {
            Board.PlacePiece(piece, piece.Position);
            player.AddPiece(piece);
        }

        public bool MakeMove(Move move)
        {
            if (move.MovingPiece.Color != CurrentTurn)
                return false;

            var legalMoves = GetLegalMoves(CurrentTurn);
            if (!legalMoves.Any(m => m.From.Equals(move.From) && m.To.Equals(move.To)))
                return false;

            var capturedPiece = Board.GetPiece(move.To);
            if (capturedPiece != null)
                GetOpponent(CurrentTurn).RemovePiece(capturedPiece);

            if (move.MovingPiece is Pawn pawn && _lastMove != null)
            {
                if (IsEnPassant(pawn, move.To, _lastMove))
                {
                    var capturedPos = new Position(move.To.X, pawn.Color == PieceColor.White ? move.To.Y + 1 : move.To.Y - 1);
                    var capturedPawn = Board.GetPiece(capturedPos);
                    if (capturedPawn != null)
                        GetOpponent(pawn.Color).RemovePiece(capturedPawn);
                    Board.RemovePiece(capturedPos);
                }
            }

            Board.MovePiece(move.From, move.To);
            move.MovingPiece.OnMove(move.To);

            if (move.MovingPiece is Pawn p)
            {
                if ((p.Color == PieceColor.White && p.Position.Y == 0) ||
                    (p.Color == PieceColor.Black && p.Position.Y == 7))
                {
                    PromotePawn(p, new Queen(p.Color, p.Position));
                }
            }

            if (move.MovingPiece is King k)
            {
                int dx = move.To.X - move.From.X;
                if (dx == 2)
                {
                    var rook = Board.GetPiece(new Position(7, move.From.Y)) as Rook;
                    if (rook != null)
                        Board.MovePiece(new Position(7, move.From.Y), new Position(5, move.From.Y));
                }
                else if (dx == -2)
                {
                    var rook = Board.GetPiece(new Position(0, move.From.Y)) as Rook;
                    if (rook != null)
                        Board.MovePiece(new Position(0, move.From.Y), new Position(3, move.From.Y));
                }
            }

            _lastMove = move;
            CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return true;
        }

        public bool IsCheck(PieceColor color)
        {
            var king = GetPlayer(color).Pieces.OfType<King>().Single();
            var opponent = GetOpponent(color);

            foreach (var piece in opponent.Pieces)
            {
                if (piece.GetAttackSquares(Board).Contains(king.Position))
                    return true;
            }

            return false;
        }

        public bool IsCheckmate(PieceColor color)
        {
            if (!IsCheck(color))
                return false;

            return !GetLegalMoves(color).Any();
        }

        public bool IsStalemate(PieceColor color)
        {
            if (IsCheck(color))
                return false;

            return !GetLegalMoves(color).Any();
        }

        public IEnumerable<Move> GetLegalMoves(PieceColor color)
        {
            var player = GetPlayer(color);
            var moves = new List<Move>();

            foreach (var piece in player.Pieces)
            {
                foreach (var target in piece.GetLegalMoves(Board))
                {
                    var move = new Move(piece.Position, target, piece);

                    if (!WouldLeaveKingInCheck(move))
                        moves.Add(move);
                }

                if (piece is King king)
                    moves.AddRange(GetCastlingMoves(king));
            }

            return moves;
        }

        private bool WouldLeaveKingInCheck(Move move)
        {
            var from = move.From;
            var to = move.To;

            var captured = Board.GetPiece(to);

            Board.MovePiece(from, to);
            move.MovingPiece.OnMove(to);

            bool inCheck = IsCheck(move.MovingPiece.Color);

            Board.MovePiece(to, from);
            move.MovingPiece.OnMove(from);

            if (captured != null)
                Board.PlacePiece(captured, to);

            return inCheck;
        }

        private IEnumerable<Move> GetCastlingMoves(King king)
        {
            var moves = new List<Move>();
            if (king.HasMoved) return moves;

            var rookK = Board.GetPiece(new Position(7, king.Position.Y)) as Rook;
            if (rookK != null && !rookK.HasMoved)
            {
                if (IsPathClear(king.Position, new Position(6, king.Position.Y)) &&
                    !WouldKingPassThroughCheck(king, new Position(5, king.Position.Y)) &&
                    !IsCheck(king.Color))
                {
                    moves.Add(new Move(king.Position, new Position(6, king.Position.Y), king));
                }
            }
            var rookQ = Board.GetPiece(new Position(0, king.Position.Y)) as Rook;
            if (rookQ != null && !rookQ.HasMoved)
            {
                if (IsPathClear(king.Position, new Position(2, king.Position.Y)) &&
                    !WouldKingPassThroughCheck(king, new Position(3, king.Position.Y)) &&
                    !IsCheck(king.Color))
                {
                    moves.Add(new Move(king.Position, new Position(2, king.Position.Y), king));
                }
            }

            return moves;
        }

        private bool IsPathClear(Position from, Position to)
        {
            int minX = Math.Min(from.X, to.X) + 1;
            int maxX = Math.Max(from.X, to.X) - 1;
            for (int x = minX; x <= maxX; x++)
                if (Board.IsPositionOccupied(new Position(x, from.Y)))
                    return false;
            return true;
        }

        private bool WouldKingPassThroughCheck(King king, Position intermediate)
        {
            var from = king.Position;
            var to = intermediate;

            Board.MovePiece(from, to);
            king.OnMove(to);

            bool inCheck = IsCheck(king.Color);

            Board.MovePiece(to, from);
            king.OnMove(from);

            return inCheck;
        }
        private bool IsEnPassant(Pawn pawn, Position target, Move lastMove)
        {
            if (!(lastMove.MovingPiece is Pawn opponentPawn)) return false;
            if (Math.Abs(lastMove.From.Y - lastMove.To.Y) != 2) return false;
            if (lastMove.To.Y != pawn.Position.Y) return false;
            if (Math.Abs(lastMove.To.X - pawn.Position.X) != 1) return false;

            if (pawn.Color == PieceColor.White && target.Y != lastMove.To.Y - 1) return false;
            if (pawn.Color == PieceColor.Black && target.Y != lastMove.To.Y + 1) return false;
            if (target.X != lastMove.To.X) return false;

            return true;
        }

        private void PromotePawn(Pawn pawn, Piece newPiece)
        {
            Board.PlacePiece(newPiece, newPiece.Position);
            var player = GetPlayer(pawn.Color);
            player.RemovePiece(pawn);
            player.AddPiece(newPiece);
        }

        private Player GetPlayer(PieceColor color) => color == PieceColor.White ? WhitePlayer : BlackPlayer;
        private Player GetOpponent(PieceColor color) => color == PieceColor.White ? BlackPlayer : WhitePlayer;
    }
}
