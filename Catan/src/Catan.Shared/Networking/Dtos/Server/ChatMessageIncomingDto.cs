namespace Catan.Shared.Networking.Dtos.Server
{
    public class ChatMessageIncomingDto
    {
        public string FromUsername { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}