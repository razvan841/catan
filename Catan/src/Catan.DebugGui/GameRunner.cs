using System.Collections.Generic;
using Catan.Shared.Game;

namespace Catan.DebugGui;

public class GameRunner
{
    public Board Board { get; }
    public List<Player> Players { get; }
    public int CurrentPlayerIndex { get; private set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public GameRunner()
    {
        Board = new Board();
        Players = new List<Player>
        {
            new Player("A"),
            new Player("B"),
            new Player("C"),
            new Player("D")
        };
    }

    public void NextTurn()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }
}
