namespace Catan.Shared.Networking.Dtos.Server
{
    public class LeaderboardEntryDto
    {
        public string Username { get; set; } = "";
        public int Elo { get; set; }
    }
}