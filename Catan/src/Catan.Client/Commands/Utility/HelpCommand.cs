using System.Threading.Tasks;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class HelpCommand : ICommandHandler
{
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "help" };

    public HelpCommand(MainWindow ui)
    {
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length == 0)
        {
            _ui.AppendChatLine(
                "Commands: /help, /ping, /queue, /elo, /leaderboard, /playerinfo, /clear, /m");
            return Task.CompletedTask;
        }

        switch (args[0].ToLower())
        {
            case "ping":
            case "pulse":
                _ui.AppendChatLine("/ping or /pulse - Checks server connectivity.");
                break;
            case "queue":
                _ui.AppendChatLine("/queue - Joins matchmaking.");
                break;
            case "elo":
                _ui.AppendChatLine("/elo [usernames] - Shows Elo ratings.");
                break;
            case "leaderboard":
                _ui.AppendChatLine("/leaderboard - Shows top players.");
                break;
            case "playerinfo":
                _ui.AppendChatLine("/playerinfo <username> - Shows player info.");
                break;
            case "clear":
                _ui.AppendChatLine("/clear - Clears the chat.");
                break;
            case "message":
            case "m":
            case "whisper":
            case "w":
                _ui.AppendChatLine("/message [usernames] - Send a private message.");
                break;
            default:
                _ui.AppendChatLine($"No help available for '{args[0]}'.");
                break;
        }

        return Task.CompletedTask;
    }
}
