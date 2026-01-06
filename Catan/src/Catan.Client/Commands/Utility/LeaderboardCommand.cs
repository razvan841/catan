using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Commands;

public class LeaderboardCommand : ICommandHandler
{
    private readonly ClientSender _sender;

    public string[] Aliases => new[] { "leaderboard" };

    public LeaderboardCommand(ClientSender sender)
    {
        _sender = sender;
    }

    public Task Execute(string[] args)
    {
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.LeaderboardRequest
        });
    }
}
