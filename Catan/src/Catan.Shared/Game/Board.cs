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
        Tiles.Clear();

        for (int i = 0; i < HEX_COUNT; i++)
        {
            var availableResources = tilesToPlace.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
            string resource = availableResources[rand.Next(availableResources.Count)];
            tilesToPlace[resource]--;

            Tiles.Add(new HexTile
            {
                Resource = resource == "sand" ? null : Enum.Parse<ResourceType>(resource, true),
                NumberToken = 0,
                HasRobber = resource == "sand"
            });
        }

        var tokensToPlace = new List<int>();
        foreach (var kv in BoardMappings.TokensToPlace)
        {
            for (int i = 0; i < kv.Value; i++)
            {
                tokensToPlace.Add(kv.Key);
            }
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

            // Check adjacency rule
            validAssignment = true;
            for (int i = 0; i < HEX_COUNT; i++)
            {
                var token = Tiles[i].NumberToken;
                if (token == 6 || token == 8)
                {
                    foreach (var adj in BoardMappings.TileAdjacencyMapping[i])
                    {
                        var adjToken = Tiles[adj].NumberToken;
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

        var desertTile = Tiles.FirstOrDefault(t => t.Resource == null);
        if (desertTile != null)
            desertTile.HasRobber = true;
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
        Ports.Add(new Port(PortType.Brick, GetVertex(43), 2));
        Ports.Add(new Port(PortType.Brick, GetVertex(43), 2));
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

    public Vertex GetVertex(int index)
    {
        if (index < 0 || index >= Vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Vertex index {index} is out of range.");

        return Vertices[index];
    }

    public void PlaceRoad(Player player /* parameters */) {}
    public void PlaceSettlement(Player player /* parameters */) {}
    public void UpgradeSettlementToCity(Player player /* parameters */) {}
    public List<Player> GetPlayersOnTile(HexTile tile) => new();
}
