using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan.Client.Commands;

public class CommandDispatcher
{
    private readonly Dictionary<string, ICommandHandler> _handlers;

    public CommandDispatcher(IEnumerable<ICommandHandler> handlers)
    {
        _handlers = handlers
            .SelectMany(h => h.Aliases.Select(a => (a, h)))
            .ToDictionary(x => x.a, x => x.h);
    }

    public Task Dispatch(string command, string[] args)
    {
        command = command.ToLower();

        if (_handlers.TryGetValue(command, out var handler))
            return handler.Execute(args);

        return Task.CompletedTask;
    }
}
