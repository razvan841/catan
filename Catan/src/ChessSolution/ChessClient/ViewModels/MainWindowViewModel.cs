using System.Collections.ObjectModel;
using System.ComponentModel;
using ChessLogic.GameBoard;
using ChessLogic.Pieces;

namespace ChessClient.ViewModels
{
    public class TileViewModel : INotifyPropertyChanged
    {
        private string color = "White";
        private string pieceSymbol = "";

        public string Color
        {
            get => color;
            set { color = value; OnPropertyChanged(nameof(Color)); }
        }

        public string PieceSymbol
        {
            get => pieceSymbol;
            set { pieceSymbol = value; OnPropertyChanged(nameof(PieceSymbol)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MainWindowViewModel
    {
        public ObservableCollection<TileViewModel> Tiles { get; }
        private Board _board;

        public MainWindowViewModel()
        {
            _board = new Board();
            Tiles = new ObservableCollection<TileViewModel>();
            InitializeTiles();
            PlaceSamplePieces();
        }

        private void InitializeTiles()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Tiles.Add(new TileViewModel
                    {
                        Color = (x + y) % 2 == 0 ? "White" : "Gray"
                    });
                }
            }
        }

        private void PlaceSamplePieces()
        {
            // White pawn
            var whitePawn = new Pawn(true);
            _board.PlacePiece(whitePawn, 0, 6);
            UpdateTileSymbol(0, 6, "♙");

            // Black pawn
            var blackPawn = new Pawn(false);
            _board.PlacePiece(blackPawn, 0, 1);
            UpdateTileSymbol(0, 1, "♟");
        }

        private void UpdateTileSymbol(int x, int y, string symbol)
        {
            int index = y * 8 + x;
            Tiles[index].PieceSymbol = symbol;
        }
    }
}
