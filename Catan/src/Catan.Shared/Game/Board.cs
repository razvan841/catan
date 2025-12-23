using System;
namespace Catan.Shared.Game;

public class Board
{
    public List<HexTile> Tiles { get; init; }
    public List<Road> Roads { get; init; }
    public List<Settlement> Settlements { get; init; }
    public List<City> Cities { get; init; }
    public List<Vertex> Vertices { get; init; }
    public List<Edge> Edges { get; init; }
    public List<Port> Ports { get; init; }
    const int HEX_COUNT = 19;
    const int VERTEX_COUNT = 54;
    const int EDGE_COUNT = 72;

    public Board()
    {
        Tiles = new List<HexTile>();
        Roads = new List<Road>();
        Settlements = new List<Settlement>();
        Cities = new List<City>();
        Vertices = new List<Vertex>();
        Edges = new List<Edge>();
        Ports = new List<Port>();

        InitializeTiles();
        InitializeVertices();
        InitializeEdges();
        InitializePorts();
    }

    private void InitializeTiles()
    {
        Random rand = new Random();
        Dictionary<string, int> tilesToPlace = new Dictionary<string, int>
        {
            {"sand", 1},
            {"stone", 3},
            {"brick", 3},
            {"wood", 4},
            {"sheep", 4},
            {"wheat", 4}
        };
        // key : token value => value : number of tokens of specific value
        Dictionary<int, int> tokensToPlace = new Dictionary<int, int>
        {
            {2, 1}, {3, 2}, {4, 2}, {5, 2}, {6, 2}, {8, 2}, {9, 2}, {10, 2}, {11, 2}, {12, 1}
        };
        for (int i = 0; i < HEX_COUNT; i++) 
        {
            List<string> availableResources = tilesToPlace
            .Where(kv => kv.Value > 0)
            .Select(kv => kv.Key)
            .ToList();

            List<int> availableTokens = tokensToPlace
            .Where(kv => kv.Value > 0)
            .Select(kv => kv.Key)
            .ToList();

            string resource = availableResources[rand.Next(availableResources.Count)];
            int token = availableTokens[rand.Next(availableTokens.Count)];
            Tiles.Add(new HexTile
            {
                Resource = resource == "sand" ? null : Enum.Parse<ResourceType>(resource, true),
                NumberToken = token,
                HasRobber = resource == "sand"
            });

            // Decrease count for this resource
            tilesToPlace[resource]--;
            tokensToPlace[token]--;
        }
    }

    private void InitializeVertices()
    {
        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
            Vertices.Add(new Vertex(adjacentTiles));
        }
    }
    private void InitializeEdges() 
    { 
        foreach (var pair in BoardMappings.EdgeMapping)
        {
            var vertexA = Vertices[pair[0]];
            var vertexB = Vertices[pair[1]];
            Edges.Add(new Edge(vertexA, vertexB));
        }
    }   

    private void InitializePorts()
    {
        Dictionary<string, int> portsToPlace = new Dictionary<string, int>
        {
            {"generic", 1},
            {"stone", 1},
            {"brick", 1},
            {"wood", 1},
            {"sheep", 1},
            {"wheat", 1}
        };

        List<int> possiblePortVertices = new List<int>
        {0, 1, 2, 3, 4, 5, 6, 7, 10, 11,
         15, 16, 20, 21, 26, 27, 32, 33,
         37, 38, 42, 43, 46, 47, 48, 49,
        50, 51, 52, 53};
    }


    // Board Actions
    public void ResetBoard() {}
    public void PlaceRoad(Player player /* parameters */) {}
    public void PlaceSettlement(Player player /* parameters */) {}
    public void UpgradeSettlementToCity(Player player /* parameters */) {}
    public List<Player> GetPlayersOnTile(HexTile tile) => new();
}
