namespace Catan.Shared.Game;

public class Edge
{
    public Guid Id { get; init; }
    public int Index { get; init; }
    public Vertex VertexA { get; init; }
    public Vertex VertexB { get; init; }
    public Road? Road { get; set; }

    public Edge(Vertex vertexA, Vertex vertexB, int index)
    {
        Id = Guid.NewGuid();
        VertexA = vertexA;
        VertexB = vertexB;
        Index = index;
        Road = null;

        vertexA.ConnectedEdges.Add(this);
        vertexB.ConnectedEdges.Add(this);
    }

    public bool ContainsVertex(Vertex v) => v == VertexA || v == VertexB;
}