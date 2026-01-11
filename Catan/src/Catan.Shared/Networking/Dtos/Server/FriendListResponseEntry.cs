namespace Catan.Shared.Networking.Dtos.Server
{
    public class FriendListResponseDto
    {
        public FriendListResponseEntryDto[] Entries { get; set; } = Array.Empty<FriendListResponseEntryDto>();
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}