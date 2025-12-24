namespace Catan.Shared.Game;

public enum PortType
{
    Generic,
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
    public int Ratio { get; init; } = 3;

    public Port(PortType type, Vertex vertex, int ratio = 3)
    {
        Type = type;
        Vertex = vertex;
        Ratio = ratio;
        vertex.Port = this;
    }
}
