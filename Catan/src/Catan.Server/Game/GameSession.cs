using Catan.Server.Sessions;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos.Server;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;

namespace Catan.Server.Game;

public class GameSession
{
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyList<ClientSession> Players { get; }
    public GameState State { get; private set; }
    public GamePhase Phase { get; private set; }

    private readonly TurnManager _turnManager;

    public GameSession(List<ClientSession> players)
    {
        Players = players;
        State = GameState.Initializing;
        Phase = GamePhase.Setup;

        _turnManager = new TurnManager(players);

        foreach (var p in players)
        {
            p.GameSession = this;
            p.State = SessionState.InMatch;
        }
    }

    public async Task StartAsync(string game)
    {
        State = GameState.Running;

        await BroadcastAsync(new ServerMessage
        {
            Type = MessageType.MatchStart,
            Payload = new MatchStartDto
            {
                MatchId = Id,
                Players = Players.Select(p => p.Username!).ToArray(),
                Game = game
            }
        });

        await NotifyTurnAsync();
    }

    public async Task HandleMessageAsync(ClientSession sender, ClientMessage message)
    {
        if (State != GameState.Running)
            return;

        if (sender != _turnManager.CurrentPlayer)
            return;

        switch (message.Type)
        {
            case MessageType.EndTurn:
                _turnManager.NextTurn();
                await NotifyTurnAsync();
                break;
        }
    }

    private async Task NotifyTurnAsync()
    {
        await BroadcastAsync(new ServerMessage
        {
            Type = MessageType.TurnChanged,
            Payload = new
            {
                CurrentPlayer = _turnManager.CurrentPlayer.Username
            }
        });
    }

    private async Task BroadcastAsync(ServerMessage message)
    {
        var data = JsonMessageSerializer.Serialize(message);

        foreach (var p in Players)
            await p.Stream.WriteAsync(data);
    }
}