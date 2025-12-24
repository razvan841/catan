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
    private readonly Dictionary<Vertex, Point> _vertexPositions = new();

    public MainWindow()
    {
        InitializeComponent();
        ConsoleHelper.AttachConsole();
        _game = new GameRunner();

        DrawBoard();
        StartConsoleLoop();
    }

    private void DrawBoard()
    {
        BoardCanvas.Children.Clear();
        _vertexPositions.Clear();

        double hexSize = 60;
        double hexWidth = hexSize * 2;
        double hexHeight = Math.Sqrt(3) * hexSize;

        double startX = 100;
        double startY = 150;

        int[] rowCounts = { 3, 4, 5, 4, 3 };
        int tileIndex = 0;

        for (int row = 0; row < rowCounts.Length; row++)
        {
            int count = rowCounts[row];
            double rowOffsetX = (5 - count) * hexWidth * 0.50;
            double y = startY + row * (hexHeight * 0.84);

            for (int col = 0; col < count; col++)
            {
                double x = startX + rowOffsetX + col * hexWidth * 0.88;
                var tile = _game.Board.Tiles[tileIndex];

                // Hex
                var hex = CreateHexagon(x, y, hexSize);
                hex.Fill = GetTileBrush(tile);
                BoardCanvas.Children.Add(hex);

                // Token
                if (tile.Resource != null)
                {
                    var text = new TextBlock
                    {
                        Text = tile.NumberToken.ToString(),
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = (tile.NumberToken == 6 || tile.NumberToken == 8)
                            ? Brushes.Red
                            : Brushes.Black
                    };

                    Canvas.SetLeft(text, x - 8);
                    Canvas.SetTop(text, y - 10);
                    BoardCanvas.Children.Add(text);
                }
                // Robber
                if (tile.HasRobber)
                {
                    var circle = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = Brushes.Black,
                        Fill = Brushes.Black
                    };

                    Canvas.SetLeft(circle, x - 5);
                    Canvas.SetTop(circle, y - 5);

                    BoardCanvas.Children.Add(circle);  
                    Canvas.SetZIndex(circle, 1); 
                }

                // Map vertices
                var hexVertices = GetHexVertices(x, y, hexSize);
                foreach (var v in hexVertices)
                {
                    var closestVertex = _game.Board.Vertices
                        .OrderBy(vx => vx.AdjacentTiles.Contains(tile) ? 0 : 1)
                        .FirstOrDefault();

                    if (closestVertex != null && !_vertexPositions.ContainsKey(closestVertex))
                        _vertexPositions[closestVertex] = v;
                }

                tileIndex++;
            }
        }

        DrawEdges();
        // DrawVertices();
        DrawPorts();

        Title = $"Current Player: {_game.CurrentPlayer.Username}";
    }


    private Brush GetTileBrush(HexTile tile)
    {
        if (tile.Resource == null)
            return Brushes.SandyBrown; // desert

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

    private List<Point> GetHexVertices(double centerX, double centerY, double size)
    {
        var points = new List<Point>();

        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i - 30;
            double angleRad = Math.PI / 180 * angleDeg;

            points.Add(new Point(
                centerX + size * Math.Cos(angleRad),
                centerY + size * Math.Sin(angleRad)
            ));
        }

        return points;
    }

    private void DrawVertices()
    {
        foreach (var kv in _vertexPositions)
        {
            var vertex = kv.Key;
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

            BoardCanvas.Children.Add(circle);
        }
    }

    private void drawVertex(double x, double y)
    {
        var circle = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                Fill = Brushes.White
            };

        Canvas.SetLeft(circle, x - 5);
        Canvas.SetTop(circle, y - 5);

        BoardCanvas.Children.Add(circle);  
        Canvas.SetZIndex(circle, 1);  
    }
    private void DrawEdges()
    {
        foreach (var edge in _game.Board.Edges)
        {
            if (!_vertexPositions.TryGetValue(edge.VertexA, out var a)) continue;
            if (!_vertexPositions.TryGetValue(edge.VertexB, out var b)) continue;

            var line = new Line
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };

            BoardCanvas.Children.Add(line);
        }
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
    private void DrawPorts()
    {
        foreach (var port in _game.Board.Ports)
        {
            var vertex = port.Vertex;
            if (!_vertexPositions.TryGetValue(vertex, out var pos))
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
                Text = port.Type == PortType.Generic ? "3:1" : "2:1",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };

            Canvas.SetLeft(label, pos.X + 6);
            Canvas.SetTop(label, pos.Y - 6);
            BoardCanvas.Children.Add(label);
        }
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
            double angleDeg = 60 * i - 30; // flat-top hex
            double angleRad = Math.PI / 180 * angleDeg;

            hex.Points.Add(new Point(
                centerX + size * Math.Cos(angleRad),
                centerY + size * Math.Sin(angleRad)
            ));
            drawVertex(centerX + size * Math.Cos(angleRad), centerY + size * Math.Sin(angleRad));
        }

        return hex;
    }


    private void StartConsoleLoop()
    {
        Task.Run(() =>
        {
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine();

                if (command == null)
                    continue;

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

        // GUI updates must run on the UI thread
        Dispatcher.Invoke(DrawBoard);
    }


}
