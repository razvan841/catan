namespace Catan.Shared.Game;

public enum PortType
{
    Generic,    // 3:1 trade
    Brick,
    Wood,
    Sheep,
    Wheat,
    Stone
}

public class Port
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public PortType Type { get; init; }
    public Vertex Vertex { get; init; }

    // Trade ratio
    public int Ratio { get; init; } = 3;

    public Port(PortType type, Vertex vertex, int ratio = 3)
    {
        Type = type;
        Vertex = vertex;
        Ratio = ratio;
    }
}
