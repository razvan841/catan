using System;
using System.Collections.Generic;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Networking;

public class ServerMessageRouter
{
    private readonly Dictionary<MessageType, Action<ServerMessage>> _handlers = new();

    public void Register(MessageType type, Action<ServerMessage> handler)
    {
        _handlers[type] = handler;
    }

    public void Route(ServerMessage message)
    {
        if (_handlers.TryGetValue(message.Type, out var handler))
            handler(message);
    }
}
