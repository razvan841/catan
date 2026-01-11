using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Documents;
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

    public void AppendChatLine(string text, string? type = null)
    {
        if (MessagesPanel.Inlines == null)
            return;
        IBrush color = type switch
        {
            "error" => Brushes.Red,
            "system" => Brushes.Blue,
            "whisper" => Brushes.Purple,
            "green" => Brushes.Green,
            "gray" => Brushes.Gray,
            _ => Brushes.Black
        };

        var run = new Run
        {
            Text = text,
            Foreground = color
        };

        MessagesPanel.Inlines.Add(run);
        MessagesPanel.Inlines.Add(new Run { Text = Environment.NewLine });

        if (MessagesPanel.Parent is ScrollViewer sv)
            sv.ScrollToEnd();
    }

    public void ClearMessages()
    {
        if (MessagesPanel.Inlines == null)
            return;
        MessagesPanel.Inlines.Clear();
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
