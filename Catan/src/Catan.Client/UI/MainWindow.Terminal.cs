using System.Threading.Tasks;
using Avalonia.Input;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;

namespace Catan.Client.UI;

public partial class MainWindow
{
    private async void CommandBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        var text = CommandBox.Text?.Trim();
        CommandBox.Text = "";

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (text.StartsWith("/"))
        {
            var parts = text[1..].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            await _commandDispatcher.Dispatch(parts[0], parts[1..]);
        }
        else
        {
            await SendChatMessageAsync(text);
        }
    }

    private async Task SendChatMessageAsync(string text)
    {
        // Local echo (same behavior as before)
        AppendChatLine($"You: {text}");

        await _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.ChatMessage,
            Payload = new ChatMessageDto
            {
                Message = text
            }
        });
    }
}
