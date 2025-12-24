using System;
using System.Collections.Generic;
using System.Linq;

namespace Catan.Shared.Game
{
    public class GameSession
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public List<Player> Players { get; private set; }
        public Board Board { get; init; } = new();
        public int CurrentPlayerIndex { get; private set; } = 0;
        public Player? LongestRoadOwner { get; private set; }
        public Player? LargestArmyOwner { get; private set; }
        public Dictionary<Player, int> PlayerVictoryPoints { get; private set; } = new();

        public GameSession(List<Player> players)
        {
            if (players == null || players.Count != 4)
                throw new ArgumentException("Game must have exactly four players!");

            Players = [.. players];
            foreach (var player in Players)
            {
                PlayerVictoryPoints[player] = 0;
            }
        }

        public void StartGame()
        {
            RandomizePlayerOrder();
            CurrentPlayerIndex = 0;
            // Additional setup (optional)
        }

        public void EndGame() { /* ... */ }

        public Player GetCurrentPlayer() => Players[CurrentPlayerIndex];

        public void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        }

        public void RandomizePlayerOrder()
        {
            var rng = new Random();
            Players = Players.OrderBy(p => rng.Next()).ToList();
        }

        // Game Actions (delegates to player/board)
        public void BuildRoad(Player player /* params */) { }
        public void BuildSettlement(Player player /* params */) { }
        public void UpgradeSettlementToCity(Player player /* params */) { }
        public void PlayDevelopmentCard(Player player, DevelopmentCard card) { }
        public void TradeWithBank(Player player /* params */) { }
        public void TradeWithPlayer(Player from, Player to /* params */) { }

        // Dice / resource distribution
        public void RollDice() { }
        public void DistributeResources(int diceNumber) { }
        public void AddVictoryPoints(Player player, int points)
        {
            if (!PlayerVictoryPoints.ContainsKey(player))
                PlayerVictoryPoints[player] = 0;

            PlayerVictoryPoints[player] += points;
        }

        public Player? GetWinner()
        {
            if (PlayerVictoryPoints.Count == 0) return null;
            var player = PlayerVictoryPoints.OrderByDescending(kv => kv.Value).First().Key;
            if(player.Points >= 10)
                return player;
            return null;
        }
    }
}
