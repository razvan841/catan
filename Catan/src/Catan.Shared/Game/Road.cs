namespace Catan.Shared.Game;

public class Road
{
    public Player Owner { get; init; }
    public Edge Edge { get; init; }

    public Road(Player owner, Edge edge)
    {
        Owner = owner;
        Edge = edge;
    }

}