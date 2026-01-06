namespace Catan.Shared.Networking.Dtos.Server
{
    public class LeaderboardResponseDto
    {
        public LeaderboardEntryDto[] Entries { get; set; } = Array.Empty<LeaderboardEntryDto>();
    }
}