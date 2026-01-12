using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Catan.Shared.Networking.Dtos.Server;

namespace Catan.Client.UI;

public partial class ProfileView : UserControl
{
    public event Action? BackRequested;

    public ProfileView()
    {
        InitializeComponent();
    }

    public void LoadProfile(ProfileResponseDto dto)
    {
        if (!dto.Success)
            return;

        LoadFriends(dto.FriendEntries);
        LoadMatchHistory(dto.MatchEntries);
    }

    private void LoadFriends(FriendListResponseEntryDto[] friends)
    {
        FriendList.ItemsSource = friends
            .Select(f => $"{f.Username} {(f.Online ? "● Online" : "○ Offline")}")
            .ToList();
    }

    private void LoadMatchHistory(MatchHistoryResponseEntryDto[] matches)
    {
        MatchHistoryList.ItemsSource = matches.Select(m =>
        {
            var players = string.Join(", ",
                m.UserEntries.Select(u => $"{u.Username} ({u.Elo})"));

            return $"[{players}] → Winner: {m.Winner}";
        }).ToList();
    }

    private void Back_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        BackRequested?.Invoke();
    }
}
