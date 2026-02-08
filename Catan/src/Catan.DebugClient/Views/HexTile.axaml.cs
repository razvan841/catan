using Avalonia.Controls;

namespace Catan.DebugClient.Views
{
    public partial class HexTile : UserControl
    {
        public HexTileModel? HexModel { get; set; }

        public HexTile()
        {
            InitializeComponent();
        }
    }
}
