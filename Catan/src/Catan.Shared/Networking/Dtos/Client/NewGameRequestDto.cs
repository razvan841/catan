namespace Catan.Shared.Networking.Dtos.Client
{
    public class NewGameRequestDto
    {
        public string Game { get; set; } = string.Empty;
        public string User1 { get; set; } = string.Empty;
        public string User2 { get; set; } = string.Empty;
        public string User3 { get; set; } = string.Empty;
    }
}