namespace Catan.Shared.Game;

public class Road
{
    public required Player Owner { get; init; }
    // Possibly store connected vertices/edges
    public required Edge Edge { get; init; }
}