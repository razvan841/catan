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
                "Commands: /help, /ping, /queue, /elo, /leaderboard, /playerinfo, /clear, /m, /friend, /group, /game");
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
                _ui.AppendChatLine("/message [username] - Send a private message.");
                break;
            case "friend":
                _ui.AppendChatLine("/friend [username] - Send a friend request.");
                break;
            case "group":
            case "g":
                _ui.AppendChatLine("/group - Send a message to all online friends.");
                break;
            case "newgame":
            case "game":
                _ui.AppendChatLine("/game [username1] [username2] [username3] - Send a game invite to three users.");
                break;
            case "block":
                _ui.AppendChatLine("/block [username] - Block a specific user.");
                break;
            case "unblock":
                _ui.AppendChatLine("/unblock [username1] - Unblock a specific user.");
                break;
            case "friendlist":
                _ui.AppendChatLine("/friendlist - Get a list of your friends and their status (online/offline).");
                break;
            default:
                _ui.AppendChatLine($"No help available for '{args[0]}'.");
                break;
        }

        return Task.CompletedTask;
    }
}
