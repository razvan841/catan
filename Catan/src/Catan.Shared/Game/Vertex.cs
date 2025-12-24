namespace Catan.Shared.Game;

public class Vertex
{
    public Guid Id { get; init; }
    public int Index { get; set; }
    public List<HexTile> AdjacentTiles { get; init; }
    public List<Edge> ConnectedEdges { get; init; }
    public Player? Owner { get; set; }
    public bool IsSettlement { get; set; }
    public bool IsCity { get; set; }
    public Port? Port { get; set; }

    public Vertex(int index, List<HexTile>? adjacentTiles = null)
    {
        Id = Guid.NewGuid();
        Index = index;
        AdjacentTiles = adjacentTiles ?? new List<HexTile>();
        ConnectedEdges = new List<Edge>();
        Owner = null;
        IsSettlement = false;
        IsCity = false;
        Port = null;
    }
}
