namespace Catan.Shared.Networking.Dtos.Server

{
    public class ProfileResponseDto
    {
        public bool Success { get; set; }
        public FriendListResponseEntryDto[] FriendEntries { get; set; } = Array.Empty<FriendListResponseEntryDto>();
        public MatchHistoryResponseEntryDto[] MatchEntries { get; set; } = Array.Empty<MatchHistoryResponseEntryDto>();
    }
}