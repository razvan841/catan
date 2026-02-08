using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Catan.DebugClient.Views
{
    public partial class Edge : UserControl
    {
        public Edge()
        {
            InitializeComponent();

            this.PointerPressed += Edge_PointerPressed;
        }

        private void Edge_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is EdgeModel model)
            {
                model.OnClicked?.Invoke(model); // tells the ViewModel that this edge was clicked
            }
        }
    }
}   
