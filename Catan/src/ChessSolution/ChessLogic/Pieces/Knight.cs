using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class Knight : Piece
    {
        public Knight(PieceColor color, Position position): base(color, position)
        {}

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            foreach (var pos in GetKnightTargets())
            {
                if (!IsInsideBoard(pos))
                    continue;

                if (!board.IsPositionOccupied(pos) ||
                    board.IsPositionOccupiedByOpponent(pos, Color))
                {
                    moves.Add(pos);
                }
            }

            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            foreach (var pos in GetKnightTargets())
            {
                if (IsInsideBoard(pos))
                    attacks.Add(pos);
            }

            return attacks;
        }

        private IEnumerable<Position> GetKnightTargets()
        {
            int x = Position.X;
            int y = Position.Y;

            yield return new Position(x + 1, y + 2);
            yield return new Position(x + 2, y + 1);
            yield return new Position(x + 2, y - 1);
            yield return new Position(x + 1, y - 2);
            yield return new Position(x - 1, y - 2);
            yield return new Position(x - 2, y - 1);
            yield return new Position(x - 2, y + 1);
            yield return new Position(x - 1, y + 2);
        }

        private static bool IsInsideBoard(Position pos)
        {
            return pos.X >= 0 && pos.X < 8 &&
                   pos.Y >= 0 && pos.Y < 8;
        }
    }
}
