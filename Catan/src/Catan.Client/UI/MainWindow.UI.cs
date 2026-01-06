using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Catan.Client.UI;

public partial class MainWindow
{
    private void UpdateUi()
    {
        DisconnectedPanel.IsVisible = _session.UiState == ClientUiState.Disconnected;
        AuthPanel.IsVisible = _session.UiState == ClientUiState.Auth;
        LobbyPanel.IsVisible = _session.UiState == ClientUiState.InLobby;
    }

    private void Log(string text)
    {
        LogBox.Text += text + Environment.NewLine;
        LogBox.CaretIndex = LogBox.Text.Length;
    }

    public void AppendChatLine(string text)
    {
        MessagesBox.Text += text + Environment.NewLine;
        MessagesBox.CaretIndex = MessagesBox.Text.Length;
    }
    public void ClearMessages()
    {
        MessagesBox.Text = "";
        MessagesBox.CaretIndex = 0;
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
    }
}
