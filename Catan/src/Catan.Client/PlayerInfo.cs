using System;
namespace Catan.Client;

public class PlayerInfo
{
    public string Username { get; set; } = "";
    public int Elo { get; set; }
    public string[] Friends { get; set; } = Array.Empty<string>();
}
