using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using System;
using System.Linq;
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
    // ========================
    // FIELDS
    // ========================
    private TcpClientConnection? _connection;
    private ClientUiState _uiState = ClientUiState.Connecting;
    private readonly ClientState _state = new();

    // ========================
    // CONSTRUCTOR
    // ========================
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

    private async Task SendAsync(ClientMessage msg)
    {
        if (_connection != null)
            await _connection.SendAsync(msg);
    }

    // ========================
    // AUTH LOGIC
    // ========================
    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowErrorPopup("Error", "Username and password cannot be empty.");
            return;
        }

        _state.Username = username;

        await SendAsync(new ClientMessage
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

        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowErrorPopup("Error", "Username and password cannot be empty.");
            return;
        }

        _state.Username = username;

        await SendAsync(new ClientMessage
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

        await SendAsync(new ClientMessage
        {
            Type = MessageType.QueueRequest
        });
    }

    private async void Pulse_Click(object? sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        await SendAsync(new ClientMessage
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

        await SendTerminalMessage(text);
    }

    private async Task SendTerminalMessage(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        if (input.StartsWith("/"))
        {
            var parts = input.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            var command = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            await HandleCommandAsync(command, args);
        }
        else
        {
            await SendChatMessageAsync(input);
        }

        CommandBox.Text = "";
    }

    private async Task HandleCommandAsync(string command, string[] args)
    {
        switch (command)
        {
            case "help":
                Log("Commands: /help, /ping, /queue");
                break;

            case "ping":
            case "pulse":
                await SendAsync(new ClientMessage { Type = MessageType.HealthRequest });
                break;

            case "queue":
                await SendAsync(new ClientMessage
                {
                    Type = MessageType.QueueRequest,
                    Payload = new QueueRequestDto { Username = _state.Username! }
                });
                break;

            default:
                MessagesBox.Text += $"Unknown command: {command}{Environment.NewLine}";
                MessagesBox.CaretIndex = MessagesBox.Text.Length;
                break;
        }
    }

    private async Task SendChatMessageAsync(string text)
    {
        MessagesBox.Text += $"You: {text}{Environment.NewLine}";
        MessagesBox.CaretIndex = MessagesBox.Text.Length;

        if (_connection != null)
        {
            await SendAsync(new ClientMessage
            {
                Type = MessageType.ChatMessage,
                Payload = text
            });
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
                HandleLoginResponse(msg);
                break;

            case MessageType.RegisterResponse:
                HandleRegisterResponse(msg);
                break;

            case MessageType.ServerChat:
                HandleServerChat(msg);
                break;

            case MessageType.HealthResponse:
                HandleHealthResponse(msg);
                break;

            default:
                Log($"Unhandled message: {msg.Type}");
                break;
        }
    }

    private void HandleLoginResponse(ServerMessage msg)
    {
        var payload = ((JsonElement)msg.Payload!).Deserialize<LoginResponseDto>()!;
        if (payload.Success)
        {
            _uiState = ClientUiState.InLobby;
            UpdateUi();
            Log("Login successful.");
        }
        else
        {
            _uiState = ClientUiState.Auth;
            UpdateUi();
            Log($"Login failed: {payload.Message}");
            ShowErrorPopup("Login Failed", payload.Message);
        }
    }

    private void HandleRegisterResponse(ServerMessage msg)
    {
        var payload = ((JsonElement)msg.Payload!).Deserialize<RegisterResponseDto>()!;
        if (payload.Success)
        {
            _uiState = ClientUiState.InLobby;
            UpdateUi();
            Log("Registration successful.");
        }
        else
        {
            _uiState = ClientUiState.Auth;
            UpdateUi();
            Log($"Registration failed: {payload.Message}");
            ShowErrorPopup("Registration Failed", payload.Message);
        }
    }

    private void HandleServerChat(ServerMessage msg)
    {
        if (_uiState != ClientUiState.InLobby) return;

        var text = msg.Payload?.ToString() ?? "";
        MessagesBox.Text += text + Environment.NewLine;
        MessagesBox.CaretIndex = MessagesBox.Text.Length;
    }

    private void HandleHealthResponse(ServerMessage msg)
    {
        if (_uiState != ClientUiState.InLobby) return;

        var payload = ((JsonElement)msg.Payload!).Deserialize<HealthResponseDto>()!;
        MessagesBox.Text += $"Health check: {(payload.Success ? "OK" : "FAIL")} at {payload.ServerTime}" + Environment.NewLine;
        MessagesBox.CaretIndex = MessagesBox.Text.Length;
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

    private async void ShowErrorPopup(string title, string message)
    {
        var window = new Window
        {
            Title = title,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var stack = new StackPanel { Margin = new Thickness(10) };

        var textBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        okButton.Click += (_, __) => window.Close();

        stack.Children.Add(textBlock);
        stack.Children.Add(okButton);

        window.Content = stack;

        await window.ShowDialog(this);
    }
}
