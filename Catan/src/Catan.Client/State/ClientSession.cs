using System;
using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Messages;

namespace Catan.Client.State
{
    public class ClientSession : IDisposable
    {
        public string? Username { get; set; }
        public string? Id { get; set; }
        public ClientUiState UiState { get; set; } = ClientUiState.Connecting;

        public TcpClientConnection? Connection { get; private set; }

        public async Task ConnectAsync(string host, int port)
        {
            Connection = await TcpClientConnection.ConnectAsync(host, port);
            UiState = ClientUiState.Auth;
        }

        public async Task SendMessageAsync(ClientMessage message)
        {
            if (Connection == null)
                throw new InvalidOperationException("Not connected.");

            await Connection.SendAsync(message);
        }

        public async Task<ServerMessage> ReceiveMessageAsync()
        {
            if (Connection == null)
                throw new InvalidOperationException("Not connected.");

            return await Connection.ReceiveAsync();
        }

        public void Disconnect()
        {
            Connection?.Dispose();
            Connection = null;
            Username = null;
            Id = null;
            UiState = ClientUiState.Auth;
        }
        public void Dispose()
        {
            Disconnect();
        }
    }
}
