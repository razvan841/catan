using System.Text.Json;
using System.Windows;
using System;
using Catan.Client.Networking;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Dtos;
using System.Threading.Tasks;

namespace Catan.Client;

public partial class MainWindow : Window
{
    private readonly TcpClientConnection _connection;
    private readonly ClientState _state = new();

    public MainWindow()
    {
        InitializeComponent();

        _connection = new TcpClientConnection("127.0.0.1", 5000);

        Task.Run(ListenAsync);
        Log("Connected to server.");
    }

    private async Task ListenAsync()
    {
        while (true)
        {
            var msg = await _connection.ReceiveAsync();
            Dispatcher.Invoke(() => HandleServerMessage(msg));
        }
    }

    // ========================
    // BUTTON HANDLERS
    // ========================

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        _state.Username = UsernameBox.Text;

        await SendAsync(new ClientMessage
        {
            Type = MessageType.RegisterRequest,
            Payload = new RegisterRequestDto
            {
                Username = _state.Username
            }
        });
    }

    private async void Queue_Click(object sender, RoutedEventArgs e)
    {
        await SendAsync(new ClientMessage
        {
            Type = MessageType.QueueRequest,
            Payload = new QueueRequestDto
            {
                Username = _state.Username!
            }
        });
    }

    private async void Pulse_Click(object sender, RoutedEventArgs e)
    {
        await SendAsync(new ClientMessage
        {
            Type = MessageType.HealthRequest
        });
    }

    private async void AcceptMatch_Click(object sender, RoutedEventArgs e)
    {
        if (_state.CurrentMatchId == null) return;

        await SendAsync(new ClientMessage
        {
            Type = MessageType.MatchResponse,
            Payload = new MatchResponseDto
            {
                MatchId = _state.CurrentMatchId.Value,
                Accepted = true
            }
        });
    }

    private async void DeclineMatch_Click(object sender, RoutedEventArgs e)
    {
        if (_state.CurrentMatchId == null) return;

        await SendAsync(new ClientMessage
        {
            Type = MessageType.MatchResponse,
            Payload = new MatchResponseDto
            {
                MatchId = _state.CurrentMatchId.Value,
                Accepted = false
            }
        });
    }

    // ========================
    // SERVER MESSAGE HANDLING
    // ========================

    private void HandleServerMessage(ServerMessage message)
    {
        switch (message.Type)
        {
            case MessageType.RegisterResponse:
            case MessageType.QueueResponse:
                Log(JsonSerializer.Serialize(message.Payload, Pretty));
                break;

            case MessageType.MatchFound:
            {
                var match = ((JsonElement)message.Payload!)
                    .Deserialize<MatchFoundDto>()!;

                _state.CurrentMatchId = match.MatchId;

                Log("Match found with:");
                foreach (var p in match.Players)
                    Log($" - {p}");

                break;
            }

            case MessageType.MatchStart:
                Log("Game started!");
                Log(JsonSerializer.Serialize(message.Payload, Pretty));
                break;

            case MessageType.TurnChanged:
                Log("Turn changed:");
                Log(JsonSerializer.Serialize(message.Payload, Pretty));
                break;
            
            case MessageType.HealthResponse:
                Log("Pulse Response:");
                Log(JsonSerializer.Serialize(message.Payload, Pretty));
                break;

            default:
                Log($"Unhandled message: {message.Type}");
                break;
        }
    }

    // ========================
    // HELPERS
    // ========================

    private async Task SendAsync(ClientMessage msg)
    {
        await _connection.SendAsync(msg);
    }

    private void Log(string text)
    {
        LogBox.AppendText(text + Environment.NewLine);
        LogBox.ScrollToEnd();
    }

    private static readonly JsonSerializerOptions Pretty =
        new() { WriteIndented = true };
}
