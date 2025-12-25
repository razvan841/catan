namespace Catan.Shared.Game
{
    public class GameSession
    {

        public enum GamePhase
        {
            NotStarted,
            RandomizingOrder,
            SetupRound1,
            SetupRound2,
            MainGame,
            EndGame
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
            NotEnoughResourcesOtherPlayer,
            NotYourTurn,
            NoCardsLeft,
            NotSetupPhase
        }

        internal static class Costs
        {
            public static readonly Dictionary<ResourceType, int> Road = new()
            {
                { ResourceType.Wood, 1 },
                { ResourceType.Brick, 1 }
            };

            public static readonly Dictionary<ResourceType, int> Settlement = new()
            {
                { ResourceType.Wood, 1 },
                { ResourceType.Brick, 1 },
                { ResourceType.Sheep, 1 },
                { ResourceType.Wheat, 1 }
            };

            public static readonly Dictionary<ResourceType, int> City = new()
            {
                { ResourceType.Wheat, 2 },
                { ResourceType.Stone, 3 }
            };

            public static readonly Dictionary<ResourceType, int> DevelopmentCard = new()
            {
                { ResourceType.Sheep, 1 },
                { ResourceType.Wheat, 1 },
                { ResourceType.Stone, 1 }
            };
        }

        // =========================
        // State
        // =========================

        public Guid Id { get; init; } = Guid.NewGuid();
        public List<Player> Players { get; private set; }
        public Board Board { get; init; } = new();
        public DevelopmentDeck DevelopmentDeck { get; }

        public int CurrentPlayerIndex { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.NotStarted;

        public Player? LongestRoadOwner { get; private set; }
        public Player? LargestArmyOwner { get; private set; }

        // =========================
        // Construction
        // =========================

        public GameSession(List<Player> players)
        {
            if (players == null || players.Count != 4)
                throw new ArgumentException("Game must have exactly four players!");

            Players = [.. players];
            DevelopmentDeck = DevelopmentDeck.CreateStandardDeck();
        }

        // =========================
        // Game Lifecycle
        // =========================

        public void StartGame()
        {
            Phase = GamePhase.RandomizingOrder;
            RandomizePlayerOrder();

            CurrentPlayerIndex = 0;
            Phase = GamePhase.SetupRound1;
        }

        public void EndGame() { }

        public void HandleEndGame(Player winner)
        {
            Phase = GamePhase.EndGame;
            EndGame();
        }

        // =========================
        // Turn Helpers
        // =========================

        public Player GetCurrentPlayer() => Players[CurrentPlayerIndex];

        public void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        }

        private void AdvanceSetupTurn()
        {
            if (Phase == GamePhase.SetupRound1)
            {
                if (CurrentPlayerIndex < Players.Count - 1)
                    CurrentPlayerIndex++;
                else
                    Phase = GamePhase.SetupRound2;
            }
            else if (Phase == GamePhase.SetupRound2)
            {
                if (CurrentPlayerIndex > 0)
                    CurrentPlayerIndex--;
                else
                {
                    Phase = GamePhase.MainGame;
                    CurrentPlayerIndex = 0;
                }
            }
        }

        private void RandomizePlayerOrder()
        {
            for (int i = Players.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (Players[i], Players[j]) = (Players[j], Players[i]);
            }
        }

        // =========================
        // Setup Phase Actions
        // =========================

        public ActionResult BuildInitialSettlement(Player player, Vertex vertex)
        {
            if (Phase != GamePhase.SetupRound1 && Phase != GamePhase.SetupRound2)
                return ActionResult.NotSetupPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            Settlement settlement;
            try
            {
                settlement = Board.BuildSettlement(player, vertex);
            }
            catch
            {
                return ActionResult.VertexOccupied;
            }

            player.Settlements.Add(settlement);
            return ActionResult.Success;
        }

        public ActionResult BuildInitialRoad(Player player, Edge edge)
        {
            if (Phase != GamePhase.SetupRound1 && Phase != GamePhase.SetupRound2)
                return ActionResult.NotSetupPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            // Must connect to the player's most recently placed settlement
            var lastSettlement = player.Settlements.LastOrDefault();
            if (lastSettlement == null)
                return ActionResult.NotConnected;

            if (edge.VertexA != lastSettlement.Vertex &&
                edge.VertexB != lastSettlement.Vertex)
                return ActionResult.NotConnected;

            Road road;
            try
            {
                road = Board.PlaceRoad(player, edge);
            }
            catch
            {
                return ActionResult.EdgeOccupied;
            }

            player.Roads.Add(road);

            AdvanceSetupTurn();
            return ActionResult.Success;
        }

        // =========================
        // Main Game Actions
        // =========================

        public ActionResult BuildRoad(Player player, Edge edge)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.NotYourTurn;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            if (!player.CanAfford(Costs.Road))
                return ActionResult.NotEnoughResources;

            Road road;
            try
            {
                road = Board.PlaceRoad(player, edge);
            }
            catch
            {
                return ActionResult.NotConnected;
            }

            player.Pay(Costs.Road);
            player.Roads.Add(road);

            UpdateLongestRoad(player);
            return ActionResult.Success;
        }

        public ActionResult BuildSettlement(Player player, Vertex vertex)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.NotYourTurn;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            if (!player.CanAfford(Costs.Settlement))
                return ActionResult.NotEnoughResources;

            Settlement settlement;
            try
            {
                settlement = Board.BuildSettlement(player, vertex);
            }
            catch
            {
                return ActionResult.VertexOccupied;
            }

            player.Pay(Costs.Settlement);
            player.Settlements.Add(settlement);

            return ActionResult.Success;
        }

        public ActionResult UpgradeToCity(Player player, Settlement settlement)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.NotYourTurn;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            if (!player.CanAfford(Costs.City))
                return ActionResult.NotEnoughResources;

            City city;
            try
            {
                city = Board.UpgradeSettlement(player, settlement);
            }
            catch
            {
                return ActionResult.SettlementDoesntExist;
            }

            player.Pay(Costs.City);
            player.Settlements.Remove(settlement);
            player.Cities.Add(city);

            return ActionResult.Success;
        }

        public ActionResult BuyDevelopmentCard(Player player)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.NotYourTurn;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            if (!player.CanAfford(Costs.DevelopmentCard))
                return ActionResult.NotEnoughResources;

            var card = DevelopmentDeck.Draw();
            if (card == null)
                return ActionResult.NoCardsLeft;

            player.Pay(Costs.DevelopmentCard);
            player.DevelopmentCards.Add(card);

            return ActionResult.Success;
        }

        // =========================
        // Dice & Resources
        // =========================

        public void RollDice()
        {
            if (Phase != GamePhase.MainGame)
                return;

            int dice = Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);
            DistributeResources(dice);
        }

        public void DistributeResources(int diceNumber) { }

        // =========================
        // Achievements
        // =========================

        private void UpdateLongestRoad(Player triggeringPlayer)
        {
            // TODO: DFS-based longest road calculation
        }
    }
}
