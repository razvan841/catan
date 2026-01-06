namespace Catan.Shared.Networking.Dtos.Server
{
    public class GroupMessageIncomingDto
    {
        public string FromUsername { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}