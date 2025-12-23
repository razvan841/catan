namespace Catan.Shared.Game;

public class Player
{
    public string Username { get; init; }
    public Guid Id { get; init; }

    public List<Resource> Resources { get; init; } = new();
    public List<Road> Roads { get; init; } = new();
    public List<Settlement> Settlements { get; init; } = new();
    public List<City> Cities { get; init; } = new();
    public List<Port> Ports { get; init; } = new();
    public List<DevelopmentCard> DevelopmentCards { get; init; } = new();

    // Game Actions
    public void BuildRoad(/* parameters */) {}
    public void BuildSettlement(/* parameters */) {}
    public void UpgradeToCity(/* parameters */) {}
    public void PlayDevelopmentCard(/* parameters */) {}
    public void BuyDevelopmentCard(/* parameters */) {}
    public void TradeWithBank(/* parameters */) {}
    public void TradeWithPort() {}
    public void TradeWithPlayer(Player targetPlayer /* trade details */) {}
}
