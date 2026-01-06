using System;
using Catan.Shared.Models;
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
            AppendChatLine($"[Friend Request Error] {dto.Message}");
        AppendChatLine($"Successfully sent the friend request!");
    }
    public void OnGroupMessageResponse(GroupMessageResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[Group Message Error] {dto.Message}");
    }
    public void OnNewGameResponse(NewGameResponseDto dto)
    {
        if (!dto.Success)
            AppendChatLine($"[New Game Error] {dto.Message}");
        AppendChatLine($"Successfully sent the game request to all other users!");
    }
}
