namespace Catan.Shared.Game;

public enum DevelopmentCardType
{
    Knight,
    VictoryPoint,
    Monopoly,
    RoadBuilding,
    YearOfPlenty
}

public class DevelopmentCard
{
    public DevelopmentCardType Type { get; init; }
    public bool Played { get; set; }
    public int BoughtOnTurn { get; set; }
    public DevelopmentCard(DevelopmentCardType type)
    {
        Type = type;
        BoughtOnTurn = -1;
    }
}
