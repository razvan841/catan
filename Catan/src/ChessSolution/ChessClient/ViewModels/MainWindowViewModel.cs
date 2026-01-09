using Avalonia.Media;
using System.Collections.ObjectModel;

namespace ChessClient.ViewModels;

public class TileViewModel
{
    public IBrush Background { get; }
    public string Symbol { get; }

    public TileViewModel(IBrush background, string symbol = "")
    {
        Background = background;
        Symbol = symbol;
    }
}

public class MainWindowViewModel
{
    public ObservableCollection<TileViewModel> Tiles { get; } = new();

    public MainWindowViewModel()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool light = (x + y) % 2 == 0;
                Tiles.Add(new TileViewModel(
                    light ? Brushes.Bisque : Brushes.SaddleBrown
                ));
            }
        }

        // sample pieces
        Tiles[6 * 8 + 4] = new TileViewModel(Brushes.Bisque, "♙");
        Tiles[1 * 8 + 4] = new TileViewModel(Brushes.SaddleBrown, "♟");
    }
}
