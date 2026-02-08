using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Catan.DebugClient.Views
{
    public partial class Edge : UserControl
    {
        public static readonly StyledProperty<IBrush> FillProperty =
            AvaloniaProperty.Register<Edge, IBrush>(nameof(Fill), Brushes.White);

        public static readonly StyledProperty<double> LengthProperty =
            AvaloniaProperty.Register<Edge, double>(nameof(Length), 50.0);

        public static readonly StyledProperty<double> AngleProperty =
            AvaloniaProperty.Register<Edge, double>(nameof(Angle), 0.0);

        public IBrush Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public double Length
        {
            get => GetValue(LengthProperty);
            set => SetValue(LengthProperty, value);
        }

        public double Angle
        {
            get => GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        public Edge()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
