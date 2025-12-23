namespace Catan.Shared.Game;

public enum ResourceType
{
    Brick,
    Wood,
    Sheep,
    Wheat,
    Stone,
    Sand
}

public class Resource
{
    public ResourceType Type { get; init; }
    public int Amount { get; set; }
}
