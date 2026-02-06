using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Catan.DebugClient
{
    public partial class Port : UserControl
    {
        // Circle color
        public static readonly StyledProperty<IBrush> FillProperty =
            AvaloniaProperty.Register<Port, IBrush>(nameof(Fill), Brushes.White);

        public IBrush Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        // Text label
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<Port, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public Port()
        {
            InitializeComponent();
            DataContext = this; // binding works for Fill and Label
        }
    }
}
