using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;

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
            Width = 320,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center
        };

        okButton.Click += (_, _) => window.Close();
        panel.Children.Add(okButton);

        window.Content = panel;
        await window.ShowDialog(this);
    }
}
