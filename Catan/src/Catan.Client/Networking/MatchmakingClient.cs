using System;
using System.Threading.Tasks;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;

namespace Catan.Client.Networking;

public class MatchmakingClient
{
    private TcpClientConnection? _client;

    public void Attach(TcpClientConnection client)
    {
        _client = client;
    }

    public async Task SendMatchResponse(bool accepted, Guid matchId, string game)
    {
        if (_client == null)
            return;

        await _client.SendAsync(new ClientMessage
        {
            Type = MessageType.MatchResponse,
            Payload = new MatchResponseDto
            {
                MatchId = matchId,
                Accepted = accepted,
                Game = game
            }
        });
    }
}
