using Avalonia.Controls;
using System;

namespace Catan.Client.UI;

public partial class MatchFoundWindow : Window
{
    public MatchFoundWindow()
    {
        InitializeComponent();

        var vm = new MatchFoundViewModel();
        DataContext = vm;

        vm.AcceptCommand.Subscribe(_ => Close(true));
        vm.DeclineCommand.Subscribe(_ => Close(false));
    }
}
