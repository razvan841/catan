using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class Bishop : Piece
    {
        public Bishop(PieceColor color, Position position): base(color, position)
        {}

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            AddDiagonalMoves(board, moves,  1,  1);
            AddDiagonalMoves(board, moves,  1, -1);
            AddDiagonalMoves(board, moves, -1,  1);
            AddDiagonalMoves(board, moves, -1, -1);

            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            AddDiagonalAttacks(board, attacks,  1,  1);
            AddDiagonalAttacks(board, attacks,  1, -1);
            AddDiagonalAttacks(board, attacks, -1,  1);
            AddDiagonalAttacks(board, attacks, -1, -1);

            return attacks;
        }

        private void AddDiagonalMoves(Board board, List<Position> moves, int dx, int dy)
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

        private void AddDiagonalAttacks(Board board, List<Position> attacks, int dx, int dy)
        {
            int x = Position.X + dx;
            int y = Position.Y + dy;

            while (IsInsideBoard(x, y))
            {
                attacks.Add(new Position(x, y));

                if (board.IsPositionOccupied(new Position(x, y)))
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
