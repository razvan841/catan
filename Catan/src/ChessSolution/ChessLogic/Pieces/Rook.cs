using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class Rook : Piece
    {
        public bool KingRook { get;}
        public Rook(PieceColor color, Position position, bool kingRook): base(color, position)
        {
            KingRook = kingRook;
        }

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            AddMovesInDirection(board, moves, 1, 0);
            AddMovesInDirection(board, moves, -1, 0);
            AddMovesInDirection(board, moves, 0, 1);
            AddMovesInDirection(board, moves, 0, -1);

            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            AddAttackSquaresInDirection(board, attacks, 1, 0);
            AddAttackSquaresInDirection(board, attacks, -1, 0);
            AddAttackSquaresInDirection(board, attacks, 0, 1);
            AddAttackSquaresInDirection(board, attacks, 0, -1);

            return attacks;
        }

        private void AddMovesInDirection(Board board, List<Position> moves, int dx, int dy)
        {
            int x = Position.X + dx;
            int y = Position.Y + dy;

            while (IsInsideBoard(x, y))
            {
                var pos = new Position(x, y);

                if (!board.IsPositionOccupied(pos))
                {
                    moves.Add(pos);
                }
                else
                {
                    if (board.IsPositionOccupiedByOpponent(pos, Color))
                        moves.Add(pos);
                    break;
                }

                x += dx;
                y += dy;
            }
        }

        private void AddAttackSquaresInDirection(Board board, List<Position> attacks, int dx, int dy)
        {
            int x = Position.X + dx;
            int y = Position.Y + dy;

            while (IsInsideBoard(x, y))
            {
                var pos = new Position(x, y);
                attacks.Add(pos);

                if (board.IsPositionOccupied(pos))
                    break;

                x += dx;
                y += dy;
            }
        }

        private static bool IsInsideBoard(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }
    }
}
