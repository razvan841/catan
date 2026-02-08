using System;
namespace Catan.Shared.Game;

public class Board
{
    public List<HexTile> Tiles { get; private set; }
    public List<Road> Roads { get; init; }
    public List<Settlement> Settlements { get; init; }
    public List<City> Cities { get; init; }
    public List<Vertex> Vertices { get; init; }
    public List<Vertex> UnbuildableVertices { get; init; } = new List<Vertex>();
    public List<Edge> Edges { get; init; }
    public List<Port> Ports { get; init; }
    public HexTile? RobberTile { get; set; }
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

            var res = Enum.Parse<ResourceType>(resource, true);

            Tiles.Add(new HexTile
            {
                Resource = res,
                Index = i,
                NumberToken = res == ResourceType.Sand ? 0 : 0, // will be assigned later
                HasRobber = res == ResourceType.Sand
            });
        }

        var tokensToPlace = BoardMappings.TokensToPlace
            .SelectMany(kv => Enumerable.Repeat(kv.Key, kv.Value))
            .ToList();

        if (tokensToPlace.Count != Tiles.Count(t => t.Resource != ResourceType.Sand))
            throw new Exception("Mismatch between non-desert tiles and tokens to place.");

        // Assign tokens respecting adjacency rules
        bool validAssignment = false;
        while (!validAssignment)
        {
            var shuffledTokens = tokensToPlace.OrderBy(_ => rand.Next()).ToList();
            int tokenIndex = 0;

            foreach (var tile in Tiles)
            {
                if (tile.Resource != ResourceType.Sand)
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
                    foreach (var adj in BoardMappings.TileToTileAdjacencyMapping[i])
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

        Vertices.Clear();
    }

    private void InitializeVertices()
    {
        for (int i = 0; i < VERTEX_COUNT; i++)
        {
            var adjacentTiles = BoardMappings.VertexToTileAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
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
                    foreach (int adj in BoardMappings.TileToTileAdjacencyMapping[i])
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
            var adjacentTiles = BoardMappings.VertexToTileAdjacencyMapping[i].Select(index => Tiles[index]).ToList();
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

    public Edge GetEdge(int index)
    {
        if (index < 0 || index >= Edges.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Edge index {index} is out of range.");

        return Edges[index];
    }

    public enum RoadPlacementResult
    {
        Success,
        EdgeOccupied,
        NotConnected
    }
    public enum SettlementPlacementResult
    {
        Success,
        VertexOccupied,
        VertexUnbuildable,
        NotConnected
    }

    public enum SettlementUpgradeResult
    {
        Success,
        SettlementDoesntExist,
        DifferentOwner
    }

    public Road PlaceRoad(Player owner, Edge edge)
    {
        var result = CanPlaceRoad(owner, edge);
        if (result != RoadPlacementResult.Success)
            throw new InvalidOperationException(result.ToString());

        var road = new Road(owner, edge);
        Roads.Add(road);
        edge.Road = road;
        return road;
    }

    public RoadPlacementResult CanPlaceRoad(Player player, Edge edge)
    {
        if (edge.Road != null)
            return RoadPlacementResult.EdgeOccupied;

        bool isConnected = false;

        foreach (var vertex in new[] { edge.VertexA, edge.VertexB })
        {
            if (Settlements.Any(s => s.Vertex == vertex && s.Owner != player) ||
                Cities.Any(c => c.Vertex == vertex && c.Owner != player))
                continue;

            if (Settlements.Any(s => s.Vertex == vertex && s.Owner == player) ||
                Cities.Any(c => c.Vertex == vertex && c.Owner == player))
            {
                isConnected = true;
                break;
            }

            foreach (var edgeIndex in BoardMappings.VertexToEdgeMapping[vertex.Index])
            {
                var adjacentEdge = Edges[edgeIndex];
                if (adjacentEdge.Road?.Owner == player)
                {
                    isConnected = true;
                    break;
                }
            }

            if (isConnected) break;
        }

        return isConnected ? RoadPlacementResult.Success : RoadPlacementResult.NotConnected;
    }


    public Settlement BuildSettlement(Player owner, Vertex vertex, bool ignoreRoadRequirement)
    {
        var result = CanPlaceSettlement(owner, vertex, ignoreRoadRequirement);
        if (result != SettlementPlacementResult.Success)
            throw new InvalidOperationException(result.ToString());

        var settlement = new Settlement(owner, vertex);
        Settlements.Add(settlement);
        vertex.IsSettlement = true;
        MarkAdjacentVerticesUnbuildable(vertex);
        return settlement;
    }

    public SettlementPlacementResult CanPlaceSettlement(Player player, Vertex vertex, bool ignoreRoadRequirement)
    {
        if (vertex.IsSettlement || vertex.IsCity)
            return SettlementPlacementResult.VertexOccupied;

        if (UnbuildableVertices.Contains(vertex))
            return SettlementPlacementResult.VertexUnbuildable;

        if (ignoreRoadRequirement)
            return SettlementPlacementResult.Success;

        bool isConnected = BoardMappings.VertexToEdgeMapping[vertex.Index]
            .Select(i => Edges[i])
            .Any(e => e.Road?.Owner == player);

        return isConnected
            ? SettlementPlacementResult.Success
            : SettlementPlacementResult.NotConnected;
    }


    private void MarkAdjacentVerticesUnbuildable(Vertex vertex)
    {
        int vertexIndex = vertex.Index;
        var edges = BoardMappings.VertexToEdgeMapping[vertexIndex];

        foreach (int edgeIndex in edges)
        {
            var edge = Edges[edgeIndex];
            Vertex otherVertex = edge.VertexA == vertex ? edge.VertexB : edge.VertexA;

            if (!UnbuildableVertices.Contains(otherVertex))
                UnbuildableVertices.Add(otherVertex);
        }
    }
    
    public City UpgradeSettlement(Player owner, Settlement settlement)
    {
        if (!Settlements.Contains(settlement))
            throw new InvalidOperationException("Settlement does not exist.");

        if (settlement.Owner != owner)
            throw new InvalidOperationException("Wrong owner.");

        Settlements.Remove(settlement);
        settlement.Vertex.IsSettlement = false;

        var city = new City(owner, settlement.Vertex);
        Cities.Add(city);
        settlement.Vertex.IsCity = true;
        return city;
    }

    public List<(Player player, Dictionary<string, int> counts)> GetPlayersOnTile(HexTile tile)
    {
        int tileIndex = tile.Index;

        if (tileIndex < 0 || tileIndex >= BoardMappings.TileToVerticesAdjacencyMapping.Length)
            throw new ArgumentOutOfRangeException(nameof(tile), $"Tile index {tileIndex} is out of range.");

        var adjacentVertexIndices = BoardMappings.TileToVerticesAdjacencyMapping[tileIndex];

        var playerCounts = new Dictionary<Player, Dictionary<string, int>>();

        foreach (var vertexIndex in adjacentVertexIndices)
        {
            var vertex = Vertices[vertexIndex];

            var settlement = Settlements.FirstOrDefault(s => s.Vertex == vertex);
            if (settlement != null)
            {
                if (!playerCounts.ContainsKey(settlement.Owner))
                    playerCounts[settlement.Owner] = new Dictionary<string, int> { { "settlement", 0 }, { "city", 0 } };

                playerCounts[settlement.Owner]["settlement"] += 1;
            }

            var city = Cities.FirstOrDefault(c => c.Vertex == vertex);
            if (city != null)
            {
                if (!playerCounts.ContainsKey(city.Owner))
                    playerCounts[city.Owner] = new Dictionary<string, int> { { "settlement", 0 }, { "city", 0 } };

                playerCounts[city.Owner]["city"] += 1;
            }
        }

        return playerCounts.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
