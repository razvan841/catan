using System;
using Catan.Shared.Models;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Server;

namespace Catan.Client.UI;

public partial class MainWindow
{
    public void OnHealthResponse(HealthResponseDto dto)
    {
        AppendChatLine($"Health check: {(dto.Success ? "OK" : "FAIL")} at {dto.ServerTime}");
    }

    public void OnEloResponse(EloResponseDto dto)
    {
        if (dto.Entries.Length == 0)
        {
            AppendChatLine("No Elo data returned.");
            return;
        }

        AppendChatLine("Elo Scores:");
        foreach (var e in dto.Entries)
            AppendChatLine($"{e.Username}: {e.Elo}");
    }

    public void OnLeaderboardResponse(LeaderboardResponseDto dto)
    {
        AppendChatLine("Leaderboard:");
        for (int i = 0; i < dto.Entries.Length; i++)
            AppendChatLine($"{i + 1}. {dto.Entries[i].Username} - {dto.Entries[i].Elo}");
    }

    public void OnPlayerInfoResponse(PlayerInfo? info)
    {
        if (info == null)
        {
            AppendChatLine("Player not found.");
            return;
        }

        AppendChatLine($"Player: {info.Username}");
        AppendChatLine($"Elo: {info.Elo}");
        AppendChatLine($"Friends: {string.Join(", ", info.Friends)}");
    }

    public void OnWhisperResponse(WhisperResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Whisper Error] {dto.Message}");
    }
    public void OnFriendResponse(FriendResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[Friend Request Error] {dto.Message}");
            return;
        }
        // AppendChatLine($"Successfully sent the friend request!");
    }
    public void OnFriendRequestAccept(FriendAcceptedDto dto)
    {

        AppendChatLine($"[Friend Request] {dto.Username} accepted!");
        return;
    }
    public void OnFriendRequestRejected(FriendAcceptedDto dto)
    {

        AppendChatLine($"[Friend Request] {dto.Username} declined!");
        return;
    }
    public void OnFriendRequestIncoming(FriendRequestIncomingDto dto)
    {
        AppendChatLine($"Received Friend request from {dto.FromUsername}!\nType /yes {dto.FromUsername} or /no {dto.FromUsername}");
    }
    public void OnGroupMessageResponse(GroupMessageResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Group Message Error] {dto.Message}");
    }
    public void OnNewGameResponse(NewGameResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[New Game Error] {dto.Message}");
            return;
        }
        AppendChatLine($"Successfully sent the game request to all other users!");
    }
    public void OnQueueResponse(QueueResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[Queue Error] {dto.Message}");
            return;
        }
        AppendChatLine($"Joined the queue! Waiting for more players...");
    }
    public async void OnMatchFound(MatchFoundDto dto)
    {
        if (_matchDialog != null)
            return;

        _matchDialog = new MatchFoundWindow();

        bool accepted = await _matchDialog.ShowDialog<bool>(this);

        _matchDialog = null;

        await _matchmakingClient.SendMatchResponse(accepted, dto.MatchId);
    }
    public void OnMatchCanceled(MatchCanceledDto dto)
    {
        _matchDialog?.Close(false);
        _matchDialog = null;

        AppendChatLine($"Match Canceled: {dto.Reason}");
    }

    public void OnMatchStart(MatchStartDto dto)
    {
        _matchDialog?.Close(true);
        _matchDialog = null;

        AppendChatLine("Match starting!");
    }
}
