using System;
namespace Catan.Client;

public class ClientState
{
    public string? Username { get; set; }
    public Guid? CurrentMatchId { get; set; }
}
