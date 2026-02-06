using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Catan.DebugClient
{
    public partial class Vertex : UserControl
    {
        public static readonly StyledProperty<IBrush> FillProperty =
            AvaloniaProperty.Register<Vertex, IBrush>(nameof(Fill), Brushes.White);

        public IBrush Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Vertex()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
