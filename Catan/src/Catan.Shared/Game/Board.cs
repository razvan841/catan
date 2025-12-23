using System;
namespace Catan.Shared.Game;

public class Board
{
    public List<HexTile> Tiles { get; private set; } = new();
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

        var tilesToPlace = new Dictionary<string, int>(BoardMappings.TilesToPlace);
        var tokensToPlace = new Dictionary<int, int>(BoardMappings.TokensToPlace);

        int?[] assignedTokens = new int?[HEX_COUNT];

        for (int i = 0; i < HEX_COUNT; i++)
        {
            var availableResources = tilesToPlace.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            string resource = availableResources[rand.Next(availableResources.Count)];
            tilesToPlace[resource]--;

            List<int> availableTokens = tokensToPlace.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();

            // Prevent 6 or 8 adjacent
            var forbiddenTokens = new HashSet<int>();
            foreach (var adj in BoardMappings.TileAdjacencyMapping[i])
            {
                if (assignedTokens[adj].HasValue && (assignedTokens[adj].Value == 6 || assignedTokens[adj].Value == 8))
                {
                    forbiddenTokens.Add(assignedTokens[adj].Value);
                }
            }

            var filteredTokens = availableTokens.Where(t => !forbiddenTokens.Contains(t)).ToList();

            if (!filteredTokens.Any())
                filteredTokens = availableTokens; // fallback if no choice left

            int token = filteredTokens[rand.Next(filteredTokens.Count)];
            tokensToPlace[token]--;

            Tiles.Add(new HexTile
            {
                Resource = resource == "sand" ? null : Enum.Parse<ResourceType>(resource, true),
                NumberToken = token,
                HasRobber = resource == "sand"
            });

            assignedTokens[i] = token;
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
        var portsToPlace = new Dictionary<string, int>(BoardMappings.PortsToPlace);
        var possibleVertices = BoardMappings.PossiblePortVertices.ToList();

        // TODO: assign ports to vertices randomly or following rules
    }


    // Board Actions
    public void ReshuffleBoard()
    {
        Random rand = new Random();

        // Reset tiles list
        Tiles = new List<HexTile>();

        // Make copies of resource and token counts
        var tilesToPlace = new Dictionary<string, int>(BoardMappings.TilesToPlace);
        var tokensToPlace = new Dictionary<int, int>(BoardMappings.TokensToPlace);

        for (int i = 0; i < HEX_COUNT; i++)
        {
            var availableResources = tilesToPlace
                .Where(kv => kv.Value > 0)
                .Select(kv => kv.Key)
                .ToList();

            var availableTokens = tokensToPlace
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

            tilesToPlace[resource]--;
            tokensToPlace[token]--;
        }

        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
            Vertices[i].AdjacentTiles.Clear();
            Vertices[i].AdjacentTiles.AddRange(adjacentTiles);
        }

        foreach (var tile in Tiles)
        {
            tile.HasRobber = tile.Resource == null;
        }
    }

    public void PlaceRoad(Player player /* parameters */) {}
    public void PlaceSettlement(Player player /* parameters */) {}
    public void UpgradeSettlementToCity(Player player /* parameters */) {}
    public List<Player> GetPlayersOnTile(HexTile tile) => new();
}
