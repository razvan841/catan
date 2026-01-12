using Avalonia.Interactivity;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Avalonia.Input;
using System;

namespace Catan.Client.UI;

public partial class MainWindow
{
    private async void Reconnect_Click(object? sender, RoutedEventArgs e)
    {
        _session.UiState = ClientUiState.Connecting;
        UpdateUi();
        await TryConnectAsync();
    }

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowErrorPopup(
                "Invalid Input",
                "Username and password cannot be empty."
            );
            return;
        }

        _session.Username = username;

        await _sender.SendAsync(new ClientMessage
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
        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowErrorPopup(
                "Invalid Input",
                "Username and password cannot be empty."
            );
            return;
        }

        _session.Username = username;

        await _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.RegisterRequest,
            Payload = new RegisterRequestDto
            {
                Username = username,
                Password = password
            }
        });
    }

    private async void Queue_Click(object? sender, RoutedEventArgs e)
    {
        if (_session.Username == null)
            return;
        await _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.QueueRequest,
            Payload = new QueueRequestDto
            {
                Username = _session.Username
            }
        });
    }

    private async void Pulse_Click(object? sender, RoutedEventArgs e)
    {
        await _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.HealthRequest
        });
    }

    private void Disconnect_Click(object? sender, RoutedEventArgs e)
    {
        _connection?.Dispose();
        _connection = null;

        _session.Disconnect();

        MessagesPanel.Text = "";
        LogBox.Text = "";

        UpdateUi();
    }

    private async void UsernameDisplay_Click(object? sender, PointerPressedEventArgs e)
    {
        LobbyPanel.IsVisible = false;
        ProfileView.IsVisible = true;
        await RequestProfileDataAsync();
    }

}
