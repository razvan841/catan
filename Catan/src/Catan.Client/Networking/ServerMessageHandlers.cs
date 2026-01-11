using System;
using System.Text.Json;
using Avalonia.Threading;
using Catan.Client.UI;
using Catan.Shared.Networking.Dtos.Server;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Shared.Models;

namespace Catan.Client.Networking;

public class ServerMessageHandlers
{
    private readonly MainWindow _ui;

    public ServerMessageHandlers(MainWindow ui)
    {
        _ui = ui;
    }

    public void Register(ServerMessageRouter router)
    {
        router.Register(MessageType.LoginResponse, HandleLogin);
        router.Register(MessageType.RegisterResponse, HandleRegister);
        router.Register(MessageType.ServerChat, HandleServerChat);
        router.Register(MessageType.HealthResponse, HandleHealth);
        router.Register(MessageType.EloResponse, HandleElo);
        router.Register(MessageType.LeaderboardResponse, HandleLeaderboard);
        router.Register(MessageType.PlayerInfoResponse, HandlePlayerInfo);
        router.Register(MessageType.WhisperResponse, HandleWhisperResponse);
        router.Register(MessageType.WhisperIncoming, HandleWhisperIncoming);
        router.Register(MessageType.ChatMessageIncoming, HandleChatIncoming);
        router.Register(MessageType.FriendResponse, HandleFriendResponse);
        router.Register(MessageType.FriendRequestIncoming, HandleFriendRequestIncoming);
        router.Register(MessageType.FriendAccepted, HandleFriendRequestAccepted);
        router.Register(MessageType.FriendRejected, HandleFriendRequestRejected);
        router.Register(MessageType.GroupMessageResponse, HandleGroupMessageResponse);
        router.Register(MessageType.GroupMessageIncoming, HandleGroupMessageIncoming);
        router.Register(MessageType.NewGameResponse, HandleNewGameResponse);

        router.Register(MessageType.QueueResponse, HandleQueueResponse);
        router.Register(MessageType.MatchFound, HandleMatchFound);
        router.Register(MessageType.MatchCanceled, HandleMatchCanceled);
        router.Register(MessageType.MatchStart, HandleMatchStart);
    }

    private void HandleLogin(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<LoginResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnLoginResponse(dto));
    }

    private void HandleRegister(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<RegisterResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnRegisterResponse(dto));
    }

    private void HandleServerChat(ServerMessage msg)
    {
        Dispatcher.UIThread.Post(() =>
            _ui.AppendChatLine(msg.Payload?.ToString() ?? string.Empty));
    }

    private void HandleHealth(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<HealthResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnHealthResponse(dto));
    }

    private void HandleElo(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<EloResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnEloResponse(dto));
    }

    private void HandleLeaderboard(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<LeaderboardResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnLeaderboardResponse(dto));
    }

    private void HandlePlayerInfo(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<PlayerInfo>();
        Dispatcher.UIThread.Post(() => _ui.OnPlayerInfoResponse(dto));
    }

    private void HandleWhisperResponse(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<WhisperResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnWhisperResponse(dto));
    }

    private void HandleWhisperIncoming(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<WhisperIncomingDto>()!;
        Dispatcher.UIThread.Post(() => _ui.AppendChatLine($"[Whisper ← {dto.FromUsername}] {dto.Message}"));
    }

    private void HandleChatIncoming(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<ChatMessageIncomingDto>()!;
        Dispatcher.UIThread.Post(() => _ui.AppendChatLine($"{dto.FromUsername}: {dto.Message}"));
    }
    private void HandleFriendResponse(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<FriendResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnFriendResponse(dto));
    }
    private void HandleFriendRequestAccepted(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<FriendAcceptedDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnFriendRequestAccept(dto));
    }
    private void HandleFriendRequestRejected(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<FriendAcceptedDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnFriendRequestRejected(dto));
    }
    private void HandleFriendRequestIncoming(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<FriendRequestIncomingDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnFriendRequestIncoming(dto));
    }
    private void HandleGroupMessageResponse(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<GroupMessageResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnGroupMessageResponse(dto));
    }
    private void HandleGroupMessageIncoming(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<GroupMessageIncomingDto>()!;
        Dispatcher.UIThread.Post(() => _ui.AppendChatLine($"{dto.FromUsername}: {dto.Message}"));
    }
    private void HandleNewGameResponse(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<NewGameResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnNewGameResponse(dto));
    }
    private void HandleQueueResponse(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<QueueResponseDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnQueueResponse(dto));
    }
    private void HandleMatchFound(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<MatchFoundDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnMatchFound(dto));
    }
    private void HandleMatchCanceled(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<MatchCanceledDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnMatchCanceled(dto));
    }
    private void HandleMatchStart(ServerMessage msg)
    {
        var dto = ((JsonElement)msg.Payload!).Deserialize<MatchStartDto>()!;
        Dispatcher.UIThread.Post(() => _ui.OnMatchStart(dto));
    }
}
