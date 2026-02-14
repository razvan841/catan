using Avalonia.Media;
using Catan.Shared.Game;
using System.ComponentModel;

namespace Catan.DebugClient.Views
{
    public class HexTileModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Shared.Game.HexTile GameHex { get; }

        public HexTileModel(Shared.Game.HexTile gameHex)
        {
            GameHex = gameHex;
        }

        public string Label => GameHex.Resource == ResourceType.Sand
                ? "Desert"
                : $"{GameHex.Resource}: {GameHex.NumberToken}";

        public IBrush Fill => GameHex.Resource switch
        {
            ResourceType.Wood => Brushes.ForestGreen,
            ResourceType.Wheat => Brushes.Goldenrod,
            ResourceType.Brick => Brushes.SandyBrown,
            ResourceType.Sheep => Brushes.LightGreen,
            ResourceType.Stone => Brushes.LightGray,
            ResourceType.Sand => Brushes.Gold,
            _ => Brushes.Gray
        };
        public bool HasRobber => GameHex.HasRobber;
        private bool _isRobberTargetHighlight;
        public bool IsRobberTargetHighlight
        {
            get => _isRobberTargetHighlight;
            set
            {
                _isRobberTargetHighlight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRobberTargetHighlight)));
            }
        }

        public void NotifyChanges()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fill)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasRobber)));
        }
    }
}
