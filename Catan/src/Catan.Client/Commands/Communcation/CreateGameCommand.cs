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
        if (args.Length != 3)
        {
            _ui.AppendChatLine("Usage: /game <user1> <user2> <user3>", "error");
            return Task.CompletedTask;
        }
        _ui.AppendChatLine($"You requested a game with {args[0]}, {args[1]}, {args[2]}", "system");

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.NewGameRequest,
            Payload = new NewGameRequestDto { User1 = args[0], User2 = args[1], User3 = args[2] }
        });
    }
}
