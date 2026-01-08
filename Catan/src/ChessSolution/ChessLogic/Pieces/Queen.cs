using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class Queen : Piece
    {
        public Queen(PieceColor color, Position position): base(color, position)
        {}

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            AddSlidingMoves(board, moves, 1, 1);
            AddSlidingMoves(board, moves, 1, -1);
            AddSlidingMoves(board, moves, -1, 1);
            AddSlidingMoves(board, moves, -1, -1);

            AddSlidingMoves(board, moves, 1, 0);
            AddSlidingMoves(board, moves, -1, 0);
            AddSlidingMoves(board, moves, 0, 1);
            AddSlidingMoves(board, moves, 0, -1);

            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            AddSlidingAttacks(board, attacks, 1, 1);
            AddSlidingAttacks(board, attacks, 1, -1);
            AddSlidingAttacks(board, attacks, -1, 1);
            AddSlidingAttacks(board, attacks, -1, -1);

            AddSlidingAttacks(board, attacks, 1, 0);
            AddSlidingAttacks(board, attacks, -1, 0);
            AddSlidingAttacks(board, attacks, 0, 1);
            AddSlidingAttacks(board, attacks, 0, -1);

            return attacks;
        }

        private void AddSlidingMoves(Board board, List<Position> moves, int dx, int dy)
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

        private void AddSlidingAttacks(Board board, List<Position> attacks, int dx, int dy)
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
