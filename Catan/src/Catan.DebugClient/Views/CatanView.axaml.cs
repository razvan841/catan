using Avalonia.Controls;

namespace Catan.DebugClient;

public partial class CatanView : Window
{
    public CatanView()
    {
        InitializeComponent();

        // Temporary debug seed
        GameLogBox.Text =
            "DEBUG CLIENT STARTED...\n" +
            "You can hook this to Catan.Shared here.";
    }
}
