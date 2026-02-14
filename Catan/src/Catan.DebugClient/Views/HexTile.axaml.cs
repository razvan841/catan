using Avalonia.Controls;
using Avalonia.Input;
namespace Catan.DebugClient.Views
{
    public partial class HexTile : UserControl
    {
        public HexTileModel? HexModel { get; set; }

        public HexTile()
        {
            InitializeComponent();
        }

        private void OnTileClicked(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is HexTileModel tile &&
                this.VisualRoot is Window window &&
                window.DataContext is CatanViewModel vm)
            {
                vm.TryMoveRobber(tile);
            }
        }
    }
}
