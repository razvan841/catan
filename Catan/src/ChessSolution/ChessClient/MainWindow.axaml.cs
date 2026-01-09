using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia;
using System.Linq;
using System.Text;
using ChessLogic.Game;
using ChessLogic.Utilities;
using ChessLogic.Pieces;

namespace ChessClient;

public partial class MainWindow : Window
{
    private readonly Game _game;
    private readonly StringBuilder _log = new();

    private Position? _selectedPosition;

    public MainWindow()
    {
        InitializeComponent();

        _game = new Game();

        BuildBoard();
        RenderBoard();
        Log("Game started. White to move.");
    }

    // -------------------------
    // UI construction
    // -------------------------
    private void BuildBoard()
    {
        BoardGrid.Children.Clear();

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool light = (x + y) % 2 == 0;

                var border = new Border
                {
                    Background = light ? Brushes.Bisque : Brushes.SaddleBrown,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Tag = new Position(x, y),
                    Child = new TextBlock
                    {
                        FontSize = 40, // BIGGER PIECES
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }
                };

                border.PointerPressed += OnTileClicked;
                BoardGrid.Children.Add(border);
            }
        }
    }

    // -------------------------
    // Rendering
    // -------------------------
    private void RenderBoard()
    {
        foreach (var child in BoardGrid.Children)
        {
            if (child is not Border border ||
                border.Child is not TextBlock text ||
                border.Tag is not Position pos)
                continue;

            var piece = _game.Board.GetPiece(pos);
            text.Text = piece != null ? GetSymbol(piece) : "";

            ResetTileVisual(border);
        }
    }

    // -------------------------
    // Input
    // -------------------------
    private void OnTileClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not Position pos)
            return;

        var piece = _game.Board.GetPiece(pos);

        // -------- SELECT --------
        if (_selectedPosition == null)
        {
            if (piece == null || piece.Color != _game.CurrentTurn)
                return;

            _selectedPosition = pos;
            RenderBoard();
            HighlightSelection(border);
            HighlightLegalMoves(piece);
            LogLegalMoves(piece);
            return;
        }

        // -------- MOVE --------
        var from = _selectedPosition.Value;
        var movingPiece = _game.Board.GetPiece(from);

        if (movingPiece == null)
        {
            _selectedPosition = null;
            RenderBoard();
            return;
        }

        var move = new Move(from, pos, movingPiece);

        if (_game.GetLegalMoves(_game.CurrentTurn)
            .Any(m => m.From.Equals(move.From) && m.To.Equals(move.To)))
        {
            _game.MakeMove(move);
            Log($"{movingPiece.GetType().Name} {from} → {pos}");
        }

        _selectedPosition = null;
        RenderBoard();
    }

    // -------------------------
    // Highlighting
    // -------------------------
    private void HighlightSelection(Border border)
    {
        border.BorderBrush = Brushes.Red;
        border.BorderThickness = new Thickness(3);
    }

    private void HighlightLegalMoves(Piece piece)
    {
        foreach (var target in piece.GetLegalMoves(_game.Board))
        {
            int index = target.Y * 8 + target.X;

            if (BoardGrid.Children[index] is Border border)
            {
                border.BorderBrush = Brushes.Green;
                border.BorderThickness = new Thickness(3);
            }
        }
    }

    private static void ResetTileVisual(Border border)
    {
        border.BorderBrush = Brushes.Black;
        border.BorderThickness = new Thickness(1);
    }

    // -------------------------
    // Logging
    // -------------------------
    private void Log(string message)
    {
        _log.AppendLine(message);
        LogText.Text = _log.ToString();
    }
    private void LogLegalMoves(Piece piece)
    {
        var moves = piece.GetLegalMoves(_game.Board).ToList();

        Log($"{piece.GetType().Name} at {FormatPosition(piece.Position)}");

        if (moves.Count == 0)
        {
            Log("  No legal moves.");
            return;
        }

        Log("  Legal moves:");
        foreach (var pos in moves)
        {
            Log($"   - {FormatPosition(pos)}");
        }
    }

    private static string FormatPosition(Position p) => $"({p.X},{p.Y})";
    // -------------------------
    // Piece symbols
    // -------------------------
    private static string GetSymbol(Piece piece) =>
        piece switch
        {
            Pawn p when p.Color == PieceColor.White => "♙",
            Pawn p when p.Color == PieceColor.Black => "♟",

            Rook r when r.Color == PieceColor.White => "♖",
            Rook r when r.Color == PieceColor.Black => "♜",

            Knight n when n.Color == PieceColor.White => "♘",
            Knight n when n.Color == PieceColor.Black => "♞",

            Bishop b when b.Color == PieceColor.White => "♗",
            Bishop b when b.Color == PieceColor.Black => "♝",

            Queen q when q.Color == PieceColor.White => "♕",
            Queen q when q.Color == PieceColor.Black => "♛",

            King k when k.Color == PieceColor.White => "♔",
            King k when k.Color == PieceColor.Black => "♚",

            _ => ""
        };

    
}
