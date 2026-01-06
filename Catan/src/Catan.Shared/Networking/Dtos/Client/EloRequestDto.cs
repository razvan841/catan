namespace Catan.Shared.Networking.Dtos.Client
{
    public class EloRequestDto
    {
        public string[] Usernames { get; set; } = Array.Empty<string>();
    }
}