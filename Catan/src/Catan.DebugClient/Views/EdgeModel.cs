using Avalonia;
using Avalonia.Media;
using Catan.Shared.Game;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Catan.DebugClient.Views
{
    public class EdgeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public Shared.Game.Edge GameEdge { get; }
        private readonly Dictionary<Player, IBrush> _playerColors;

        private bool _isHighlightable;
        public bool IsHighlightable
        {
            get => _isHighlightable;
            set
            {
                if (_isHighlightable != value)
                {
                    _isHighlightable = value;
                    OnPropertyChanged(nameof(IsHighlightable));
                    OnPropertyChanged(nameof(Stroke));
                }
            }
        }
        
        public EdgeModel(Shared.Game.Edge edge, Dictionary<Player, IBrush> playerColors, Action<EdgeModel>? onClicked = null)
        {
            GameEdge = edge;
            OnClicked = onClicked;
            _playerColors = playerColors;
        }

        public Action<EdgeModel>? OnClicked { get; set; }

        public IBrush Stroke
        {
            get
            {
                if (IsHighlightable) return Brushes.Yellow;
                if (GameEdge.Road != null && _playerColors.TryGetValue(GameEdge.Road.Owner, out var color))
                    return color;
                return Brushes.White;
            }
        }

        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
