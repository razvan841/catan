using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Dtos;

namespace Catan.Client;

public enum ClientUiState
{
    Connecting,
    Disconnected,
    Auth,
    InLobby
}

public partial class MainWindow : Window
{
    private TcpClientConnection? _connection;
    private ClientUiState _uiState = ClientUiState.Connecting;
    private readonly ClientState _state = new();

    public MainWindow()
    {
        InitializeComponent();
        UpdateUi();
        _ = TryConnectAsync();
    }

    // ========================
    // CONNECTION LOGIC
    // ========================

    private async Task TryConnectAsync()
    {
        try
        {
            _connection = new TcpClientConnection("127.0.0.1", 5000);
            _uiState = ClientUiState.Auth;

            _ = Task.Run(ListenAsync);

            Log("Connected to server.");
        }
        catch
        {
            _uiState = ClientUiState.Disconnected;
            Log("Failed to connect to server.");
        }

        UpdateUi();
    }

    private async void Reconnect_Click(object? sender, RoutedEventArgs e)
    {
        _uiState = ClientUiState.Connecting;
        UpdateUi();
        await TryConnectAsync();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (_connection != null)
            {
                var msg = await _connection.ReceiveAsync();
                Dispatcher.UIThread.Post(() => HandleServerMessage(msg));
            }
        }
        catch
        {
            Dispatcher.UIThread.Post(() =>
            {
                _uiState = ClientUiState.Disconnected;
                UpdateUi();
                Log("Disconnected from server.");
            });
        }
    }

    // ========================
    // AUTH LOGIC
    // ========================

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        var username = UsernameBox.Text;
        var password = PasswordBox.Text;

        _state.Username = username;

        await _connection.SendAsync(new ClientMessage
        {
            Type = MessageType.LoginRequest,
            Payload = new LoginRequestDto
            {
                Username = username,
                Password = password
            }
        });
    }

    private async void Register_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        var username = UsernameBox.Text;
        var password = PasswordBox.Text;

        _state.Username = username;

        await _connection.SendAsync(new ClientMessage
        {
            Type = MessageType.RegisterRequest,
            Payload = new RegisterRequestDto
            {
                Username = username,
                Password = password
            }
        });
    }

    // ========================
    // LOBBY ACTIONS
    // ========================

    private async void Queue_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        await _connection.SendAsync(new ClientMessage
        {
            Type = MessageType.QueueRequest
        });
    }

    private async void Pulse_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        await _connection.SendAsync(new ClientMessage
        {
            Type = MessageType.HealthRequest
        });
    }

    // ========================
    // TERMINAL INPUT
    // ========================

    private async void CommandBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var text = CommandBox.Text?.Trim();
        CommandBox.Text = "";

        if (string.IsNullOrEmpty(text)) return;

        if (text.StartsWith("/"))
        {
            HandleCommand(text[1..]);
        }
        else
        {
            if (_connection != null)
            {
                await _connection.SendAsync(new ClientMessage
                {
                    Type = MessageType.ChatMessage,
                    Payload = text
                });
            }
        }
    }

    private void HandleCommand(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();

        switch (cmd)
        {
            case "help":
                Log("Commands: /help, /ping, /quit");
                break;
            case "ping":
                Pulse_Click(null, null);
                break;
            default:
                Log($"Unknown command: {cmd}");
                break;
        }
    }

    // ========================
    // SERVER MESSAGE HANDLING
    // ========================

    private void HandleServerMessage(ServerMessage msg)
    {
        switch (msg.Type)
        {
            case MessageType.LoginResponse:
            case MessageType.RegisterResponse:
                _uiState = ClientUiState.InLobby;
                UpdateUi();
                break;

            case MessageType.ServerChat:
                MessagesBox.Text += msg.Payload + Environment.NewLine;
                MessagesBox.CaretIndex = MessagesBox.Text.Length;
                break;

            case MessageType.HealthResponse:
                MessagesBox.Text += msg.Payload + Environment.NewLine;
                MessagesBox.CaretIndex = MessagesBox.Text.Length;
                break;

            default:
                Log($"Unhandled message: {msg.Type}");
                break;
        }
    }

    // ========================
    // UI HELPERS
    // ========================

    private void UpdateUi()
    {
        DisconnectedPanel.IsVisible = _uiState == ClientUiState.Disconnected;
        AuthPanel.IsVisible = _uiState == ClientUiState.Auth;
        LobbyPanel.IsVisible = _uiState == ClientUiState.InLobby;
    }

    private void Log(string text)
    {
        LogBox.Text += text + Environment.NewLine;
        LogBox.CaretIndex = LogBox.Text.Length;
    }
}
