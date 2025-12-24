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
    private readonly GameRunner _game;

    // Predefined positions
    private readonly Dictionary<HexTile, Point> _tileCenters = new();
    private readonly Dictionary<Vertex, Point> _vertexPositions = new();
    private readonly Dictionary<Edge, Tuple<Vertex, Vertex>> _edges = new();

    public MainWindow()
    {
        InitializeComponent();
        ConsoleHelper.AttachConsole();
        _game = new GameRunner();

        DefineBoardPositions();
        DrawBoard();
        StartConsoleLoop();
    }

    private void DefineBoardPositions()
    {
        double hexSize = 60;
        double hexWidth = hexSize * 2;
        double hexHeight = Math.Sqrt(3) * hexSize;
        double startY = 150;
        int[] rowCountsTiles = { 3, 4, 5, 4, 3 };
        int[] rowCountsVertices = { 3, 4, 4, 5, 5, 6, 6, 5, 5, 4, 4, 3 };
        double vertexVerticalSpacing = hexHeight / 2; // 51.96
        double vertexHorizontalSpacing = hexHeight;
        int tileIndex = 0;
        int vertexIndex = 0;

        double horizontalSpacing = 103.92; 
        double verticalSpacing = 90; 

        for (int row = 0; row < rowCountsTiles.Length; row++)
        {
            int count = rowCountsTiles[row];
            double rowStartX = 100 + 51.96 * (5 - count);

            double y = startY + row * verticalSpacing; // hexHeight * 0.84

            for (int col = 0; col < count; col++)
            {
                double x = rowStartX + col * horizontalSpacing;
                _tileCenters[_game.Board.Tiles[tileIndex]] = new Point(x, y);
                tileIndex++;
            }
        }

        // Define vertices
        for (int row = 0; row < rowCountsVertices.Length; row++)
        {
            int count = rowCountsVertices[row];
            double rowStartX = 48.04 + 51.96 * (6 - count);
            double y;

            switch (row)
            {
                case 0:
                    y = 40;
                    break;
                case 1:
                    y = 70;
                    break;
                case 2:
                    y = 130;
                    break;
                case 3:
                    y = 160;
                    break;
                case 4:
                    y = 220;
                    break;
                case 5:
                    y = 250;
                    break;
                case 6:
                    y = 310;
                    break;
                case 7:
                    y = 340;
                    break;
                case 8:
                    y = 400;
                    break;
                case 9:
                    y = 430;
                    break;
                case 10:
                    y = 490;
                    break;
                case 11:
                    y = 520;
                    break;
                default:
                    throw new IndexOutOfRangeException("Impossible number of vertices in a row!");
            }
            y+= 51.96;

            for (int col = 0; col < count; col++)
            {
                double x = rowStartX + col * vertexHorizontalSpacing;
                _vertexPositions[_game.Board.Vertices[vertexIndex]] = new Point(x, y);
                vertexIndex++;
            }
        }

        // Define edges (reference game logic vertices)
        foreach (var edge in _game.Board.Edges)
        {
            _edges[edge] = Tuple.Create(edge.VertexA, edge.VertexB);
        }
    }

    private void DrawBoard()
    {
        BoardCanvas.Children.Clear();

        // Draw hex tiles
        foreach (var kv in _tileCenters)
        {
            var tile = kv.Key;
            var center = kv.Value;

            var hex = CreateHexagon(center.X, center.Y, 60);
            hex.Fill = GetTileBrush(tile);
            BoardCanvas.Children.Add(hex);

            // Number token
            if (tile.Resource != null)
            {
                var stack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var tokenText = new TextBlock
                {
                    Text = tile.NumberToken.ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = (tile.NumberToken == 6 || tile.NumberToken == 8)
                        ? Brushes.Red
                        : Brushes.Black
                };

                var resourceText = new TextBlock
                {
                    Text = tile.Resource.ToString(),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.Black
                };

                stack.Children.Add(tokenText);
                stack.Children.Add(resourceText);

                var border = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(20),
                    Padding = new Thickness(6),
                    Child = stack
                };

                Canvas.SetLeft(border, center.X - 22);
                Canvas.SetTop(border, center.Y - 22);

                BoardCanvas.Children.Add(border);
            }


            // Robber
            if (_game.Board.RobberTile == tile)
            {
                var robberCircle = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Stroke = Brushes.Black,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(robberCircle, center.X - 7);
                Canvas.SetTop(robberCircle, center.Y - 7);
                Canvas.SetZIndex(robberCircle, 3);
                BoardCanvas.Children.Add(robberCircle);
            }
        }

        // Draw edges
        foreach (var kv in _edges)
        {
            var edge = kv.Key;
            var v1 = kv.Value.Item1;
            var v2 = kv.Value.Item2;

            if (!_vertexPositions.TryGetValue(v1, out var p1)) continue;
            if (!_vertexPositions.TryGetValue(v2, out var p2)) continue;

            var line = new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            BoardCanvas.Children.Add(line);
        }

        // Draw vertices
        foreach (var kv in _vertexPositions)
        {
            var vertex = kv.Key;

            // If vertex has a port, skip here
            if (_game.Board.Ports.Any(p => p.Vertex == vertex))
                continue;

            var pos = kv.Value;
            var circle = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                Fill = Brushes.White
            };
            Canvas.SetLeft(circle, pos.X - 5);
            Canvas.SetTop(circle, pos.Y - 5);
            Canvas.SetZIndex(circle, 2);
            BoardCanvas.Children.Add(circle);
        }

        // Draw ports
        foreach (var port in _game.Board.Ports)
        {
            if (!_vertexPositions.TryGetValue(port.Vertex, out var pos))
                continue;

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

            var label = new TextBlock
            {
                Text = port.Type == PortType.Generic
                    ? "3:1"
                    : $"2:1 {port.Type}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(label, pos.X + 6);
            Canvas.SetTop(label, pos.Y - 6);
            BoardCanvas.Children.Add(label);
        }

        Title = $"Current Player: {_game.CurrentPlayer.Username}";
    }

    private Polygon CreateHexagon(double centerX, double centerY, double size)
    {
        var hex = new Polygon
        {
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i - 30;
            double angleRad = Math.PI / 180 * angleDeg;
            hex.Points.Add(new Point(centerX + size * Math.Cos(angleRad), centerY + size * Math.Sin(angleRad)));
        }

        return hex;
    }

    private Brush GetTileBrush(HexTile tile)
    {
        if (tile.Resource == null) return Brushes.SandyBrown;
        return tile.Resource switch
        {
            ResourceType.Wood => Brushes.ForestGreen,
            ResourceType.Brick => Brushes.IndianRed,
            ResourceType.Stone => Brushes.LightGray,
            ResourceType.Sheep => Brushes.LightGreen,
            ResourceType.Wheat => Brushes.Gold,
            _ => Brushes.White
        };
    }

    private Brush GetPortBrush(PortType type)
    {
        return type switch
        {
            PortType.Generic => Brushes.DarkBlue,
            PortType.Wood => Brushes.ForestGreen,
            PortType.Brick => Brushes.IndianRed,
            PortType.Stone => Brushes.Gray,
            PortType.Sheep => Brushes.LightGreen,
            PortType.Wheat => Brushes.Goldenrod,
            _ => Brushes.Black
        };
    }

    private void RollDiceButton_Click(object sender, RoutedEventArgs e)
    {
        // int diceRoll = _game.RollDice(); 
        MessageBox.Show($"Dice rolled: {0}");
    }

    private void FinishTurnButton_Click(object sender, RoutedEventArgs e)
    {
        DrawBoard();
    }

    private void ReshuffleButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Board.ReshuffleBoard();
        _game.Board.InitializeRobber();
        DefineBoardPositions();
        DrawBoard();
    }

    private void StartConsoleLoop()
    {
        Task.Run(() =>
        {
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                if (command == null) continue;

                HandleCommand(command.Trim().ToLower());
            }
        });
    }

    private void HandleCommand(string command)
    {
        switch (command)
        {
            case "next":
                _game.NextTurn();
                break;
            case "reshuffle":
                _game.Board.ReshuffleBoard();
                break;
            case "exit":
                Environment.Exit(0);
                return;
            default:
                Console.WriteLine("Commands:");
                Console.WriteLine(" next       -> next player");
                Console.WriteLine(" reshuffle  -> reshuffle board");
                Console.WriteLine(" exit");
                break;
        }

        Dispatcher.Invoke(DrawBoard);
    }
}
