namespace Catan.Shared.Game;

public class GameSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public List<Player> Players { get; init; } = new();
    public Board Board { get; init; } = new();
    public int CurrentPlayerIndex { get; private set; } = 0;

    // Lifecycle
    public void StartGame() {}
    public void EndGame() {}

    // Turn Management
    public Player GetCurrentPlayer() => Players[CurrentPlayerIndex];
    public void NextTurn() {}

    // Game Actions (delegates to player/board)
    public void BuildRoad(Player player /* params */) {}
    public void BuildSettlement(Player player /* params */) {}
    public void UpgradeSettlementToCity(Player player /* params */) {}
    public void PlayDevelopmentCard(Player player, DevelopmentCard card) {}
    public void TradeWithBank(Player player /* params */) {}
    public void TradeWithPlayer(Player from, Player to /* params */) {}
    
    // Dice / resource distribution
    public void RollDice() {}
    public void DistributeResources(int diceNumber) {}
}
