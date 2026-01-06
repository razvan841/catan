using System;
namespace Catan.Client.Networking;

public class ClientState
{
    public string? Username { get; set; }
    public Guid? CurrentMatchId { get; set; }
}
