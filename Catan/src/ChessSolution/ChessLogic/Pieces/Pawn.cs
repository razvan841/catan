using ChessLogic.Utilities;
using ChessLogic.GameBoard;
using System.Collections.Generic;

namespace ChessLogic.Pieces
{
    public class Pawn : Piece
    {
        public Pawn(PieceColor color, Position position)
            : base(color, position)
        {
        }

        public override IEnumerable<Position> GetLegalMoves(Board board)
        {
            var moves = new List<Position>();

            int direction = Color == PieceColor.White ? -1 : 1;

            var oneForward = new Position(Position.X, Position.Y + direction);
            if (IsInsideBoard(oneForward) && !board.IsPositionOccupied(oneForward))
            {
                moves.Add(oneForward);
                var twoForward = new Position(Position.X, Position.Y + 2 * direction);
                if (!HasMoved && IsInsideBoard(twoForward) && !board.IsPositionOccupied(twoForward))
                    moves.Add(twoForward);
            }

            var captureLeft = new Position(Position.X - 1, Position.Y + direction);
            if (IsInsideBoard(captureLeft) && board.IsPositionOccupiedByOpponent(captureLeft, Color))
                moves.Add(captureLeft);

            var captureRight = new Position(Position.X + 1, Position.Y + direction);
            if (IsInsideBoard(captureRight) && board.IsPositionOccupiedByOpponent(captureRight, Color))
                moves.Add(captureRight);
                
            return moves;
        }

        public override IEnumerable<Position> GetAttackSquares(Board board)
        {
            var attacks = new List<Position>();

            int direction = Color == PieceColor.White ? -1 : 1;

            var left = new Position(Position.X - 1, Position.Y + direction);
            if (IsInsideBoard(left))
                attacks.Add(left);

            var right = new Position(Position.X + 1, Position.Y + direction);
            if (IsInsideBoard(right))
                attacks.Add(right);

            return attacks;
        }

        public bool CanPromote()
        {
            return Color == PieceColor.White && Position.Y == 0
                || Color == PieceColor.Black && Position.Y == 7;
        }

        private static bool IsInsideBoard(Position position)
        {
            return position.X >= 0 && position.X < 8 &&
                   position.Y >= 0 && position.Y < 8;
        }
    }
}
