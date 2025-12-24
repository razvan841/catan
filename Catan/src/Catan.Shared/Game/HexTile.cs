namespace Catan.Shared.Game;

public class HexTile
{
    public ResourceType? Resource { get; init; }
    public int Index { get; init; }
    public int NumberToken { get; set; }
    public bool HasRobber { get; set; }
}
