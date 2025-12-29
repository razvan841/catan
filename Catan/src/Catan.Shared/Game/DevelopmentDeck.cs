using System.Security.Cryptography;

namespace Catan.Shared.Game;

public class DevelopmentDeck
{
    private readonly Stack<DevelopmentCard> _cards;

    private DevelopmentDeck(Stack<DevelopmentCard> cards)
    {
        _cards = cards;
    }

    public static DevelopmentDeck CreateStandardDeck()
    {
        var cards = new List<DevelopmentCard>();

        Add(cards, DevelopmentCardType.Knight, 14);
        Add(cards, DevelopmentCardType.VictoryPoint, 5);
        Add(cards, DevelopmentCardType.RoadBuilding, 2);
        Add(cards, DevelopmentCardType.YearOfPlenty, 2);
        Add(cards, DevelopmentCardType.Monopoly, 2);

        Shuffle(cards);
        return new DevelopmentDeck(new Stack<DevelopmentCard>(cards));
    }

    public DevelopmentCard? Draw()
    {
        return _cards.Count > 0 ? _cards.Pop() : null;
    }

    public int Count => _cards.Count;

    // ---------- Helpers ----------
    private static void Add(List<DevelopmentCard> list, DevelopmentCardType type, int count)
    {
        for (int i = 0; i < count; i++)
            list.Add(new DevelopmentCard(type));
    }

    private static void Shuffle(List<DevelopmentCard> list)
    {
        // Fisher–Yates using cryptographic RNG
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
