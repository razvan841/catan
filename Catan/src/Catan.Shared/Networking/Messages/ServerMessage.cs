using Catan.Shared.Enums;

namespace Catan.Shared.Networking.Messages;

public class ServerMessage
{
    public MessageType Type { get; set; }
    public object? Payload { get; set; }
}
