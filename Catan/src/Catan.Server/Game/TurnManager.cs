using Catan.Server.Sessions;
namespace Catan.Server.Game;

public class TurnManager
{
    private int _currentIndex = 0;
    private readonly List<ClientSession> _players;

    public TurnManager(List<ClientSession> players)
    {
        _players = players;
    }

    public ClientSession CurrentPlayer => _players[_currentIndex];

    public void NextTurn()
    {
        _currentIndex = (_currentIndex + 1) % _players.Count;
    }
}