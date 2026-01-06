namespace Catan.Shared.Networking.Dtos.Server
{
    public class EloResponseDto
    {
        public EloResponseEntryDto[] Entries { get; set; } = Array.Empty<EloResponseEntryDto>();
    }
}