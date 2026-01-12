namespace Catan.Shared.Networking.Dtos.Server
{
    public class MatchHistoryResponseEntryDto
    {
        public LeaderboardEntryDto[] UserEntries { get; set; } = Array.Empty<LeaderboardEntryDto>();

        public string Winner { get; set; } = string.Empty;
    }
}