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
    public HexTile RobberTile { get; private set; }
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
        InitializeRobber();
    }

    private void InitializeTiles()
    {
        Random rand = new Random();

        var tilesToPlace = new Dictionary<string, int>(BoardMappings.TilesToPlace);
        Tiles.Clear();

        for (int i = 0; i < HEX_COUNT; i++)
        {
            var availableResources = tilesToPlace.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            if (availableResources.Count == 0)
                throw new Exception("No available resources left to place.");

            string resource = availableResources[rand.Next(availableResources.Count)];
            tilesToPlace[resource]--;

            Tiles.Add(new HexTile
            {
                Resource = resource.Equals("sand", StringComparison.OrdinalIgnoreCase) ? null : Enum.Parse<ResourceType>(resource, true),
                Index = i,
                NumberToken = 0,
                HasRobber = resource.Equals("sand", StringComparison.OrdinalIgnoreCase)
            });
        }

        var tokensToPlace = BoardMappings.TokensToPlace
            .SelectMany(kv => Enumerable.Repeat(kv.Key, kv.Value))
            .ToList();

        if (tokensToPlace.Count != Tiles.Count(t => t.Resource != null))
            throw new Exception("Mismatch between non-desert tiles and tokens to place.");

        // Assign tokens respecting adjacency rules
        bool validAssignment = false;
        while (!validAssignment)
        {
            var shuffledTokens = tokensToPlace.OrderBy(_ => rand.Next()).ToList();
            int tokenIndex = 0;

            foreach (var tile in Tiles)
            {
                if (tile.Resource != null)
                {
                    tile.NumberToken = shuffledTokens[tokenIndex++];
                }
                else
                {
                    tile.NumberToken = 0;
                }
            }

            // Validate adjacency: no 6 or 8 adjacent
            validAssignment = true;
            for (int i = 0; i < Tiles.Count; i++)
            {
                int token = Tiles[i].NumberToken;
                if (token == 6 || token == 8)
                {
                    foreach (var adj in BoardMappings.TileAdjacencyMapping[i])
                    {
                        if (adj < 0 || adj >= Tiles.Count)
                            throw new Exception($"TileAdjacencyMapping index out of range: {adj}");

                        int adjToken = Tiles[adj].NumberToken;
                        if (adjToken == 6 || adjToken == 8)
                        {
                            validAssignment = false;
                            break;
                        }
                    }
                }
                if (!validAssignment) break;
            }
        }

        // Update vertex adjacency
        Vertices.Clear();
        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexAdjacencyMapping[i]
                .Select(index =>
                {
                    if (index < 0 || index >= Tiles.Count)
                        throw new Exception($"VertexAdjacencyMapping index out of range: {index}");
                    return Tiles[index];
                })
                .ToList();

            Vertices.Add(new Vertex(i, adjacentTiles));
        }
    }

    private void InitializeVertices()
    {
        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
            Vertices.Add(new Vertex(i, adjacentTiles));
        }
    }

    private void InitializeEdges()
    {
        int index = 0;
        foreach (var pair in BoardMappings.EdgeMapping)
        {
            var vertexA = Vertices[pair[0]];
            var vertexB = Vertices[pair[1]];
            Edges.Add(new Edge(vertexA, vertexB, index));
            index ++;
        }
    }

    private void InitializePorts()
    {
        var portsToPlace = new Dictionary<string, int>(BoardMappings.PortsToPlace);
        var allowedPortVertices = BoardMappings.PossiblePortVertices.ToList();

        // TODO: assign ports to vertices randomly or following rules
        Ports.Add(new Port(PortType.Generic, GetVertex(1), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(4), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(2), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(6), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(21), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(27), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(50), 3));
        Ports.Add(new Port(PortType.Generic, GetVertex(53), 3));

        Ports.Add(new Port(PortType.Wheat, GetVertex(48), 2));
        Ports.Add(new Port(PortType.Wheat, GetVertex(52), 2));
        Ports.Add(new Port(PortType.Wood, GetVertex(37), 2));
        Ports.Add(new Port(PortType.Wood, GetVertex(42), 2));
        Ports.Add(new Port(PortType.Sheep, GetVertex(7), 2));
        Ports.Add(new Port(PortType.Sheep, GetVertex(11), 2));
        Ports.Add(new Port(PortType.Stone, GetVertex(38), 2));
        Ports.Add(new Port(PortType.Stone, GetVertex(43), 2));
        // TODO: Fix index of brick port
        Ports.Add(new Port(PortType.Brick, GetVertex(15), 2));
        Ports.Add(new Port(PortType.Brick, GetVertex(20), 2));
    }

    public void PlaceRobber(HexTile tile)
    {
        if (RobberTile != null)
            RobberTile.HasRobber = false;

        RobberTile = tile;
        tile.HasRobber = true;
    }

    public void InitializeRobber()
    {
        var desert = Tiles.FirstOrDefault(t => t.Resource == null);
        if (desert != null)
            PlaceRobber(desert);
    }


    // Board Actions
    public void ReshuffleBoard()
    {
        Random rand = new Random();

        var tilesToPlace = new Dictionary<string, int>(BoardMappings.TilesToPlace);
        Tiles.Clear();

        for (int i = 0; i < HEX_COUNT; i++)
        {
            var availableResources = tilesToPlace.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            string resource = availableResources[rand.Next(availableResources.Count)];
            tilesToPlace[resource]--;

            Tiles.Add(new HexTile
            {
                Resource = resource == "sand" ? null : Enum.Parse<ResourceType>(resource, true),
                Index = i,
                NumberToken = 0,
                HasRobber = resource == "sand"
            });
        }

        var tokensToPlace = new List<int>();
        foreach (var kv in BoardMappings.TokensToPlace)
        {
            for (int i = 0; i < kv.Value; i++)
                tokensToPlace.Add(kv.Key);
        }

        bool validAssignment = false;

        while (!validAssignment)
        {
            var shuffledTokens = tokensToPlace.OrderBy(_ => rand.Next()).ToList();

            int tokenIndex = 0;
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Resource != null)
                {
                    Tiles[i].NumberToken = shuffledTokens[tokenIndex];
                    tokenIndex++;
                }
                else
                {
                    Tiles[i].NumberToken = 0;
                }
            }

            validAssignment = true;

            for (int i = 0; i < HEX_COUNT; i++)
            {
                int token = Tiles[i].NumberToken;
                if (token == 6 || token == 8)
                {
                    foreach (int adj in BoardMappings.TileAdjacencyMapping[i])
                    {
                        int adjToken = Tiles[adj].NumberToken;
                        if (adjToken == 6 || adjToken == 8)
                        {
                            validAssignment = false;
                            break;
                        }
                    }
                }
                if (!validAssignment) break;
            }
        }

        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
            Vertices[i].AdjacentTiles.Clear();
            Vertices[i].AdjacentTiles.AddRange(adjacentTiles);
        }
    }


    public Vertex GetVertex(int index)
    {
        if (index < 0 || index >= Vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Vertex index {index} is out of range.");

        return Vertices[index];
    }

    public void PlaceRoad(Player player, Vertex vertex) {}
    public void PlaceSettlement(Player player, Vertex vertex) {}
    public void UpgradeSettlementToCity(Player player, Settlement settlement) {}
    public List<Player> GetPlayersOnTile(HexTile tile) => new();
}
