using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class King : Piece
    {
        public King(PieceColor color, Position position): base(color, position)
        {}

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            int[] dx = { -1, 0, 1 };
            int[] dy = { -1, 0, 1 };

            foreach (var xOffset in dx)
            {
                foreach (var yOffset in dy)
                {
                    if (xOffset == 0 && yOffset == 0)
                        continue;

                    int newX = Position.X + xOffset;
                    int newY = Position.Y + yOffset;

                    if (!IsInsideBoard(newX, newY))
                        continue;

                    var pos = new Position(newX, newY);

                    if (!board.IsPositionOccupied(pos) || board.IsPositionOccupiedByOpponent(pos, Color))
                        moves.Add(pos);
                }
            }

            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            int[] dx = { -1, 0, 1 };
            int[] dy = { -1, 0, 1 };

            foreach (var xOffset in dx)
            {
                foreach (var yOffset in dy)
                {
                    if (xOffset == 0 && yOffset == 0)
                        continue;

                    int newX = Position.X + xOffset;
                    int newY = Position.Y + yOffset;

                    if (IsInsideBoard(newX, newY))
                        attacks.Add(new Position(newX, newY));
                }
            }

            return attacks;
        }

        public bool CanCastleKingside()
        {
            return !HasMoved;
        }

        public bool CanCastleQueenside()
        {
            return !HasMoved;
        }

        private static bool IsInsideBoard(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }
    }
}
