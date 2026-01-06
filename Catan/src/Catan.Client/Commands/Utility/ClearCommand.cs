using System.Threading.Tasks;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class ClearCommand : ICommandHandler
{
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "clear", "c" };

    public ClearCommand(MainWindow ui)
    {
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        _ui.ClearMessages();
        return Task.CompletedTask;
    }
}
