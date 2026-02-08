using Avalonia.Media;
using Catan.Shared.Game;
using System.ComponentModel;
using System.Collections.Generic;

namespace Catan.DebugClient.Views
{
    public class VertexModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Shared.Game.Vertex GameVertex { get; }
        private readonly Dictionary<Player, IBrush> _playerColors;

        private bool _isHighlightable = false;
        public bool IsHighlightable
        {
            get => _isHighlightable;
            set
            {
                if (_isHighlightable != value)
                {
                    _isHighlightable = value;
                    OnPropertyChanged(nameof(IsHighlightable));
                    OnPropertyChanged(nameof(Fill));
                }
            }
        }

        public VertexModel(Shared.Game.Vertex gameVertex, Dictionary<Player, IBrush> playerColors)
        {
            GameVertex = gameVertex;
            _playerColors = playerColors;
        }

        public IBrush Fill
        {
            get
            {
                if (GameVertex.Owner != null && _playerColors.TryGetValue(GameVertex.Owner, out var brush))
                    return brush;

                return IsHighlightable ? Brushes.LightGreen
                                    : (GameVertex.IsSettlement || GameVertex.IsCity) ? Brushes.Gray
                                                                                    : Brushes.White;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}