namespace Catan.Shared.Game;

public class Player
{
    public string Username { get; init; }
    public Guid Id { get; init; }

    public Dictionary<ResourceType, int> Resources { get; init; } = new()
    {
        { ResourceType.Wood, 0 },
        { ResourceType.Brick, 0 },
        { ResourceType.Sheep, 0 },
        { ResourceType.Wheat, 0 },
        { ResourceType.Stone, 0 }
    };
    public List<Road> Roads { get; init; } = new();
    public List<Settlement> Settlements { get; init; } = new();
    public List<City> Cities { get; init; } = new();
    public List<Port> Ports { get; init; } = new();
    public List<DevelopmentCard> DevelopmentCards { get; init; } = new();
    public int LongestRoadBuilt { get; set; } = 0;
    public int KnightsPlayed { get; set; } = 0; 
    public bool LongestRoadOwner = false;
    public bool LargestArmyOwner = false;
    public int Points
    {
        get
        {
            int settlementPoints = Settlements.Count;
            int cityPoints = Cities.Count * 2;
            int bonusPoints = 0;

            if (LongestRoadOwner) bonusPoints += 2;
            if (LargestArmyOwner) bonusPoints += 2;

            return settlementPoints + cityPoints + bonusPoints;
        }
    }


    public Player(string name)
    {
        Username = name;
        Id = Guid.NewGuid();
    }
    public enum ActionResult
    {
        Success,
        EdgeOccupied,
        NotConnected,
        VertexOccupied,
        VertexUnbuildable,
        SettlementDoesntExist,
        DifferentOwner,
        NoPortAccess,
        NotEnoughResources,
        NotEnoughResourcesOtherPlayer
    }

    // Game Actions
    public ActionResult BuildRoad(Board board, Edge edge, bool setup)
    {
        if(!setup)
        {
            if (!Resources.TryGetValue(ResourceType.Wood, out int wood) || wood < 1 ||
                !Resources.TryGetValue(ResourceType.Brick, out int brick) || brick < 1)
                return ActionResult.NotEnoughResources;
        }
        var result = board.PlaceRoad(this, edge, setup);
        if (result != Board.RoadPlacementResult.Success)
        {
            return result switch
            {
                Board.RoadPlacementResult.EdgeOccupied => ActionResult.EdgeOccupied,
                Board.RoadPlacementResult.NotConnected => ActionResult.NotConnected,
                _ => ActionResult.NotConnected
            };
        }
        if(!setup)
        {
            Resources[ResourceType.Wood] -= 1;
            Resources[ResourceType.Brick] -= 1;
        }
        var road = new Road(this, edge);
        Roads.Add(road);

        return ActionResult.Success;
    }

    public ActionResult BuildSettlement(Board board, Vertex vertex, bool setup)
    {
        if (!setup)
        {
            if (!Resources.TryGetValue(ResourceType.Wood, out int wood) || wood < 1 ||
                !Resources.TryGetValue(ResourceType.Brick, out int brick) || brick < 1 ||
                !Resources.TryGetValue(ResourceType.Sheep, out int sheep) || sheep < 1 ||
                !Resources.TryGetValue(ResourceType.Wheat, out int wheat) || wheat < 1)
                return ActionResult.NotEnoughResources;
        }

        var result = board.PlaceSettlement(this, vertex, setup);
        if (result != Board.SettlementPlacementResult.Success)
        {
            return result switch
            {
                Board.SettlementPlacementResult.VertexOccupied => ActionResult.VertexOccupied,
                Board.SettlementPlacementResult.NotConnected => ActionResult.NotConnected,
                _ => ActionResult.NotConnected
            };
        }
        if (!setup)
        {
            Resources[ResourceType.Wood] -= 1;
            Resources[ResourceType.Brick] -= 1;
            Resources[ResourceType.Sheep] -= 1;
            Resources[ResourceType.Wheat] -= 1;
        }
        var settlement = new Settlement(this, vertex);

        Settlements.Add(settlement);

        return ActionResult.Success;
    }
    public ActionResult UpgradeToCity(Board board, Settlement settlement)
    {
        return ActionResult.Success;
    }
    public ActionResult PlayDevelopmentCard(GameSession gameSession, DevelopmentCard developmentCard)
    {
        return ActionResult.Success;
    }
    public ActionResult BuyDevelopmentCard(GameSession gameSession, DevelopmentCard developmentCard)
    {
        return ActionResult.Success;
    }
    public ActionResult TradeWithBank(GameSession gameSession, ResourceType currency, ResourceType target)
    {
        return ActionResult.Success;
    }
    public ActionResult TradeWithPort(GameSession gameSession, ResourceType currency, ResourceType target)
    {
        return ActionResult.Success;
    }
    public ActionResult TradeWithPlayer(GameSession gameSession, Player targetPlayer, List<Resource> payment, List<Resource> value)
    {
        return ActionResult.Success;
    }

}
