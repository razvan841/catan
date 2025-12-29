namespace Catan.Shared.Game;

public class Player
{
    public string Username { get; init; }
    public Guid Id { get; } = Guid.NewGuid();

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
    public List<PortType> Ports { get; init; } = new();
    public List<DevelopmentCard> DevelopmentCards { get; init; } = new();
    public int KnightsPlayed { get; set; }
    public int LongestRoad { get; set; }
    public bool LongestRoadOwner { get; set; }
    public bool LargestArmyOwner { get; set; }

    public int VictoryPoints =>
        Settlements.Count +
        Cities.Count * 2 +
        (LongestRoadOwner ? 2 : 0) +
        (LargestArmyOwner ? 2 : 0);

    public Player(string username)
    {
        Username = username;
    }


    public bool CanAfford(Dictionary<ResourceType, int> cost) =>
        cost.All(kv => Resources[kv.Key] >= kv.Value);

    public void Pay(Dictionary<ResourceType, int> cost)
    {
        foreach (var kv in cost)
            Resources[kv.Key] -= kv.Value;
    }

    public void Receive(Dictionary<ResourceType, int> resources)
    {
        foreach (var kv in resources)
            Resources[kv.Key] += kv.Value;
    }    

}
