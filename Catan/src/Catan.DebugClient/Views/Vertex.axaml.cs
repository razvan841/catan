using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Catan.DebugClient.Views
{
    public partial class Vertex : UserControl
    {
        public VertexModel? VertexModel { get; set; }

        public Vertex()
        {
            InitializeComponent();
            this.PointerPressed += Vertex_PointerPressed;
        }

        private void Vertex_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (VertexModel == null) return;

            var parentVM = this.FindAncestorOfType<CatanView>()?.DataContext as CatanViewModel;
            if (parentVM != null)
            {
                parentVM.TryPlaceSettlement(VertexModel);
            }
        }
    }
}
