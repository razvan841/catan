namespace Catan.Shared.Game;

public class Dice
{
    private static readonly Random Random = new();

    public static (int, int) Roll()
    {
        return (Random.Next(1,7), Random.Next(1,7));
    }

    public static int RollSum()
    {
        var (d1, d2) = Roll();
        return d1 + d2;
    }
}
