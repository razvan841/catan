using System;
using Catan.Shared.Models;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Server;

namespace Catan.Client.UI;

public partial class MainWindow
{
    public void OnHealthResponse(HealthResponseDto dto)
    {
        AppendChatLine($"Health check: {(dto.Success ? "OK" : "FAIL")} at {dto.ServerTime}", "system");
    }

    public void OnEloResponse(EloResponseDto dto)
    {
        if (dto.Entries.Length == 0)
        {
            AppendChatLine("No Elo data returned.", "system");
            return;
        }

        AppendChatLine("Elo Scores:");
        foreach (var e in dto.Entries)
            AppendChatLine($"{e.Username}: {e.Elo}", "system");
    }

    public void OnLeaderboardResponse(LeaderboardResponseDto dto)
    {
        AppendChatLine("Leaderboard:", "system");
        for (int i = 0; i < dto.Entries.Length; i++)
            AppendChatLine($"{i + 1}. {dto.Entries[i].Username} - {dto.Entries[i].Elo}", "system");
    }

    public void OnPlayerInfoResponse(PlayerInfo? info)
    {
        if (info == null)
        {
            AppendChatLine("Player not found.", "error");
            return;
        }

        AppendChatLine($"Player: {info.Username}");
        AppendChatLine($"Elo: {info.Elo}");
        AppendChatLine($"Friends: {string.Join(", ", info.Friends)}");
    }
    public void OnBlockResponse(BlockResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Block Error] {dto.Message}", "error");
    }
    public void OnUnblockResponse(UnblockResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Block Error] {dto.Message}", "error");
    }

    public void OnWhisperResponse(WhisperResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Whisper Error] {dto.Message}", "error");
    }
    public void OnFriendResponse(FriendResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[Friend Request Error] {dto.Message}", "error");
            return;
        }
        // AppendChatLine($"Successfully sent the friend request!");
    }
    public void OnFriendRequestAccept(FriendAcceptedDto dto)
    {

        AppendChatLine($"[Friend Request] {dto.Username} accepted!", "system");
        return;
    }
    public void OnFriendRequestRejected(FriendAcceptedDto dto)
    {

        AppendChatLine($"[Friend Request] {dto.Username} declined!", "system");
        return;
    }
    public void OnFriendRequestIncoming(FriendRequestIncomingDto dto)
    {
        AppendChatLine($"Received Friend request from {dto.FromUsername}!\nType /yes {dto.FromUsername} or /no {dto.FromUsername}", "system");
    }
    public void OnGroupMessageResponse(GroupMessageResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Group Message Error] {dto.Message}", "error");
    }
    public void OnFriendListResponse(FriendListResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[Friend List Error] {dto.Message}", "error");
            return;
        }

        AppendChatLine("Friend List:", "system");

        if (dto.Entries.Length == 0)
        {
            AppendChatLine("  (No friends found)", "system");
            return;
        }

        foreach (var entry in dto.Entries)
        {
            string status = entry.Online ? "Online" : "Offline";
            string color = entry.Online ? "green" : "gray";

            AppendChatLine($"  {entry.Username} - {status}", color);
        }
    }
    public void OnUnfriendResponse(UnfriendResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Unfriend Error] {dto.Message}", "error");
    }

    public void OnNewGameResponse(NewGameResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[New Game Error] {dto.Message}", "error");
            return;
        }
        AppendChatLine($"Successfully sent the game request to all other users!", "system");
    }
    public void OnQueueResponse(QueueResponseDto dto)
    {
        if (!dto.Success)
        {
            AppendChatLine($"[Queue Error] {dto.Message}", "error");
            return;
        }
        AppendChatLine($"Joined the queue! Waiting for more players...", "system");
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

        AppendChatLine($"Match Canceled: {dto.Reason}", "error");
    }

    public void OnMatchStart(MatchStartDto dto)
    {
        _matchDialog?.Close(true);
        _matchDialog = null;

        AppendChatLine("Match starting!", "system");
    }
}
