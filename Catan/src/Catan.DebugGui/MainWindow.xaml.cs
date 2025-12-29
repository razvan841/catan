using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Catan.Shared.Game;

namespace Catan.DebugGui;

public partial class MainWindow : Window
{
    private readonly GameSession _game;

    // Predefined positions (DO NOT CHANGE)
    private readonly Dictionary<HexTile, Point> _tileCenters = new();
    private readonly Dictionary<Vertex, Point> _vertexPositions = new();
    private readonly Dictionary<Edge, Tuple<Vertex, Vertex>> _edges = new();

    public MainWindow()
    {
        InitializeComponent();
        ConsoleHelper.AttachConsole();

        _game = new GameSession(new List<Player>
        {
            new Player("A"),
            new Player("B"),
            new Player("C"),
            new Player("D")
        });

        _game.StartGame();

        DefineBoardPositions();
        DrawBoard();
        StartConsoleLoop();
    }

    // =========================
    // Board geometry (UNCHANGED)
    // =========================

    private void DefineBoardPositions()
    {
        double startY = 150;
        int[] rowCountsTiles = { 3, 4, 5, 4, 3 };
        int[] rowCountsVertices = { 3, 4, 4, 5, 5, 6, 6, 5, 5, 4, 4, 3 };

        double horizontalSpacing = 103.92;
        double verticalSpacing = 90;
        double vertexHorizontalSpacing = 51.96 * 2;

        int tileIndex = 0;
        int vertexIndex = 0;

        for (int row = 0; row < rowCountsTiles.Length; row++)
        {
            int count = rowCountsTiles[row];
            double rowStartX = 100 + 51.96 * (5 - count);
            double y = startY + row * verticalSpacing;

            for (int col = 0; col < count; col++)
            {
                double x = rowStartX + col * horizontalSpacing;
                _tileCenters[_game.Board.Tiles[tileIndex]] = new Point(x, y);
                tileIndex++;
            }
        }

        for (int row = 0; row < rowCountsVertices.Length; row++)
        {
            int count = rowCountsVertices[row];
            double rowStartX = 48.04 + 51.96 * (6 - count);

            double y = row switch
            {
                0 => 40,
                1 => 70,
                2 => 130,
                3 => 160,
                4 => 220,
                5 => 250,
                6 => 310,
                7 => 340,
                8 => 400,
                9 => 430,
                10 => 490,
                11 => 520,
                _ => throw new IndexOutOfRangeException()
            };

            y += 51.96;

            for (int col = 0; col < count; col++)
            {
                double x = rowStartX + col * vertexHorizontalSpacing;
                _vertexPositions[_game.Board.Vertices[vertexIndex]] = new Point(x, y);
                vertexIndex++;
            }
        }

        foreach (var edge in _game.Board.Edges)
            _edges[edge] = Tuple.Create(edge.VertexA, edge.VertexB);
    }

    // =========================
    // Drawing
    // =========================

    private void DrawBoard()
    {
        BoardCanvas.Children.Clear();

        DrawTiles();
        DrawEdges();
        DrawVertices();
        DrawPorts();

        Title = $"Current Player: {_game.GetCurrentPlayer().Username} | Phase: {_game.Phase}";
    }

    private void DrawTiles()
    {
        foreach (var kv in _tileCenters)
        {
            var tile = kv.Key;
            var center = kv.Value;

            var hex = CreateHexagon(center.X, center.Y, 60);
            hex.Fill = GetTileBrush(tile);
            BoardCanvas.Children.Add(hex);

            if (tile.Resource != null)
            {
                var token = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(20),
                    Padding = new Thickness(6),
                    Child = new TextBlock
                    {
                        Text = tile.Resource != null 
                            ? $"{tile.NumberToken} \n {tile.Resource}"
                            : tile.NumberToken.ToString(),
                        FontWeight = FontWeights.Bold,
                        Foreground = (tile.NumberToken == 6 || tile.NumberToken == 8)
                            ? Brushes.Red
                            : Brushes.Black,
                        TextAlignment = TextAlignment.Center
                    }

                };

                Canvas.SetLeft(token, center.X - 25);
                Canvas.SetTop(token, center.Y - 25);
                BoardCanvas.Children.Add(token);
            }

            if (_game.Board.RobberTile == tile)
            {
                var robber = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(robber, center.X - 7);
                Canvas.SetTop(robber, center.Y - 7);
                BoardCanvas.Children.Add(robber);
            }
        }
    }

    private void DrawEdges()
    {
        foreach (var kv in _edges)
        {
            var edge = kv.Key;
            var (v1, v2) = kv.Value;

            var p1 = _vertexPositions[v1];
            var p2 = _vertexPositions[v2];

            var line = new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = edge.Road == null
                    ? Brushes.Black
                    : Brushes.DarkRed,
                StrokeThickness = 3
            };

            line.MouseLeftButtonDown += (_, _) =>
            {
                var result = _game.BuildRoad(_game.GetCurrentPlayer(), edge, false);
                Console.WriteLine(result);
                DrawBoard();
            };

            BoardCanvas.Children.Add(line);
        }
    }

    private void DrawVertices()
    {
        foreach (var kv in _vertexPositions)
        {
            var vertex = kv.Key;
            var pos = kv.Value;

            if (_game.Board.Ports.Any(p => p.Vertex == vertex))
                continue;

            var circle = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = vertex.IsCity
                    ? Brushes.DarkBlue
                    : vertex.IsSettlement
                        ? Brushes.DarkGreen
                        : Brushes.White,
                Stroke = Brushes.Black
            };

            Canvas.SetLeft(circle, pos.X - 5);
            Canvas.SetTop(circle, pos.Y - 5);
            BoardCanvas.Children.Add(circle);
        }
    }
    private void RollDiceButton_Click(object sender, RoutedEventArgs e)
    {
        _game.RollDice();
        DrawBoard();
    }

    private void FinishTurnButton_Click(object sender, RoutedEventArgs e)
    {
        _game.NextTurn();
        DrawBoard();
    }

    private void ReshuffleButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Board.ReshuffleBoard();
        DrawBoard();
    }
    private void DrawPorts()
    {
        foreach (var port in _game.Board.Ports)
        {
            var pos = _vertexPositions[port.Vertex];
            Console.WriteLine($"{port.Vertex.Index} -- {port.Type}");
            var marker = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = GetPortBrush(port.Type),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(marker, pos.X - 7);
            Canvas.SetTop(marker, pos.Y - 7);
            BoardCanvas.Children.Add(marker);

            string textString = port.Type == PortType.Generic 
                ? "3:1"
                : $"2:1 {port.Type}";

            var text = new TextBlock
            {
                Text = textString,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center
            };
            switch(port.Vertex.Index)
            {
                case 1:
                case 2:
                case 4:
                    Canvas.SetLeft(text, pos.X - 5);
                    Canvas.SetTop(text, pos.Y - 20);
                    break;
                case 7:
                    Canvas.SetLeft(text, pos.X - 60);
                    Canvas.SetTop(text, pos.Y - 10);
                    break;
                case 27:
                    Canvas.SetLeft(text, pos.X - 20);
                    Canvas.SetTop(text, pos.Y + 10);
                    break;
                case 11:
                case 38:
                    Canvas.SetLeft(text, pos.X - 50);
                    Canvas.SetTop(text, pos.Y);
                    break;
                case 20:
                    Canvas.SetLeft(text, pos.X + 15);
                    Canvas.SetTop(text, pos.Y - 10);
                    break;
                case 21:
                    Canvas.SetLeft(text, pos.X - 25);
                    Canvas.SetTop(text, pos.Y - 10);
                    break;
                case 43:
                    Canvas.SetLeft(text, pos.X - 60);
                    Canvas.SetTop(text, pos.Y);
                    break;
                case 48:
                    Canvas.SetLeft(text, pos.X - 25);
                    Canvas.SetTop(text, pos.Y + 15);
                    break;
                default:
                    Canvas.SetLeft(text, pos.X + 5);
                    Canvas.SetTop(text, pos.Y + 5);
                    break;
            }

            BoardCanvas.Children.Add(text);

        }
    }

    // =========================
    // UI helpers
    // =========================

    private Polygon CreateHexagon(double cx, double cy, double size)
    {
        var hex = new Polygon { Stroke = Brushes.Black, StrokeThickness = 2 };

        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 180 * (60 * i - 30);
            hex.Points.Add(new Point(cx + size * Math.Cos(angle), cy + size * Math.Sin(angle)));
        }

        return hex;
    }

    private Brush GetTileBrush(HexTile tile) =>
        tile.Resource switch
        {
            null => Brushes.SandyBrown,
            ResourceType.Wood => Brushes.ForestGreen,
            ResourceType.Brick => Brushes.IndianRed,
            ResourceType.Stone => Brushes.LightGray,
            ResourceType.Sheep => Brushes.LightGreen,
            ResourceType.Wheat => Brushes.Gold,
            _ => Brushes.White
        };

    private Brush GetPortBrush(PortType type) =>
        type switch
        {
            PortType.Generic => Brushes.DarkBlue,
            PortType.Wood => Brushes.ForestGreen,
            PortType.Brick => Brushes.IndianRed,
            PortType.Stone => Brushes.Gray,
            PortType.Sheep => Brushes.LightGreen,
            PortType.Wheat => Brushes.Goldenrod,
            _ => Brushes.Black
        };

    // =========================
    // Console
    // =========================

    private void StartConsoleLoop()
    {
        Task.Run(() =>
        {
            while (true)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();
                Dispatcher.Invoke(() => HandleCommand(cmd));
            }
        });
    }

    private void HandleCommand(string cmd)
    {
        if (cmd == null) return;

        switch (cmd.Trim().ToLower())
        {
            case "roll":
                _game.RollDice();
                break;
            case "end":
                _game.NextTurn();
                break;
        }

        DrawBoard();
    }
}
