namespace Catan.Shared.Game;

public class HexTile
{
    public ResourceType? Resource { get; init; }
    public int NumberToken { get; init; }
    public bool HasRobber { get; set; }
}
