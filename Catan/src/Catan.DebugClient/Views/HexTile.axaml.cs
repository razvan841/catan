using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Catan.DebugClient.Views
{
    public partial class HexTile : UserControl
    {
        // Fill property (editable hex color)
        public static readonly StyledProperty<IBrush> FillProperty =
            AvaloniaProperty.Register<HexTile, IBrush>(nameof(Fill), Brushes.LightGray);

        // Label property (editable text)
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<HexTile, string>(nameof(Label), "Resource 0");

        public IBrush Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public HexTile()
        {
            InitializeComponent();
            DataContext = this; // Bind Fill and Label to XAML
        }
    }
}
