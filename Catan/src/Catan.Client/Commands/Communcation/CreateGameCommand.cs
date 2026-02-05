using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class CreateGameCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "newgame", "game" };

    public CreateGameCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args[0] != "Chess" && args[0] != "Catan")
        {
            _ui.AppendChatLine("Unsupported game!", "error");
            return Task.CompletedTask;
        }
        if ((args[0] == "Catan" && args.Length != 4) || (args[0] == "Chess" && args.Length != 2))
        {
            _ui.AppendChatLine("Usage: /game Catan <user1> <user2> <user3> or\n/game Chess <user1>", "error");
            return Task.CompletedTask;
        }
        if(args[0] == "Catan")
        {
            _ui.AppendChatLine($"You requested a {args[0]} game with {args[1]}, {args[2]}, {args[3]}", "system");
            return _sender.SendAsync(new ClientMessage
            {
                Type = MessageType.NewGameRequest,
                Payload = new NewGameRequestDto { Game = "Catan", User1 = args[1], User2 = args[2], User3 = args[3] }
            });
        } else
        {
            _ui.AppendChatLine($"You requested a {args[0]} game with {args[1]}", "system");
            return _sender.SendAsync(new ClientMessage
            {
                Type = MessageType.NewGameRequest,
                Payload = new NewGameRequestDto { Game = "Chess", User1 = args[1] }
            });
        }

        
    }
}
