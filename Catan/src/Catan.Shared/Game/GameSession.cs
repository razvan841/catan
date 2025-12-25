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
        public enum TurnPhase
        {
            NotStarted,
            Roll,
            Trade,
            Build
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
            WrongPhase,
            WrongTurnPhase,
            RobberAlreadyThere,
            NoResourcesToSteal,
            NoDiscardNecessary,
            NotEnoughDiscardedCards
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
        public TurnPhase Turn { get; private set; } = TurnPhase.NotStarted;

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
                return ActionResult.WrongPhase;

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
            if (vertex.Port != null && !player.Ports.Contains(vertex.Port.Type))
                player.Ports.Add(vertex.Port.Type);
            return ActionResult.Success;
        }

        public ActionResult BuildInitialRoad(Player player, Edge edge)
        {
            if (Phase != GamePhase.SetupRound1 && Phase != GamePhase.SetupRound2)
                return ActionResult.WrongPhase;

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
                return ActionResult.WrongPhase;

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
            Turn = TurnPhase.Build;
            UpdateLongestRoad(player);
            return ActionResult.Success;
        }

        public ActionResult BuildSettlement(Player player, Vertex vertex)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

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
            if (vertex.Port != null && !player.Ports.Contains(vertex.Port.Type))
                player.Ports.Add(vertex.Port.Type);
            player.Pay(Costs.Settlement);
            player.Settlements.Add(settlement);
            Turn = TurnPhase.Build;
            return ActionResult.Success;
        }

        public ActionResult UpgradeToCity(Player player, Settlement settlement)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

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
            Turn = TurnPhase.Build;
            return ActionResult.Success;
        }

        public ActionResult BuyDevelopmentCard(Player player)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;

            if (!player.CanAfford(Costs.DevelopmentCard))
                return ActionResult.NotEnoughResources;

            var card = DevelopmentDeck.Draw();
            if (card == null)
                return ActionResult.NoCardsLeft;

            player.Pay(Costs.DevelopmentCard);
            player.DevelopmentCards.Add(card);
            Turn = TurnPhase.Build;
            return ActionResult.Success;
        }

        public ActionResult TradeWithBank(Player player, ResourceType currency, ResourceType target)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (Turn != TurnPhase.Trade)
                return ActionResult.WrongTurnPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;
            var cost = new Dictionary<ResourceType, int> { { currency, 4 } };
            if (!player.CanAfford(cost))
                return ActionResult.NotEnoughResources;
            player.Pay(cost);
            player.Receive(new Dictionary<ResourceType, int> { { target, 1 } });
            return ActionResult.Success;
        }

        public ActionResult TradeWithPort(Player player, ResourceType currency, ResourceType target, PortType portType)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (Turn != TurnPhase.Trade)
                return ActionResult.WrongTurnPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;
            
            if(!player.Ports.Contains(portType))
                return ActionResult.NoPortAccess;
            var cost = new Dictionary<ResourceType, int> { { currency, 2 } };
            if (!player.CanAfford(cost))
                return ActionResult.NotEnoughResources;

            player.Pay(cost);
            player.Receive(new Dictionary<ResourceType, int> { { target, 1 } });
            return ActionResult.Success;
        }

        public ActionResult TradeWithPlayer(Player currentPlayer, Player otherPlayer, Dictionary<ResourceType, int> payment, Dictionary<ResourceType, int> offer)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (Turn != TurnPhase.Trade)
                return ActionResult.WrongTurnPhase;

            if (GetCurrentPlayer() != currentPlayer)
                return ActionResult.NotYourTurn;

            if (!currentPlayer.CanAfford(payment))
                return ActionResult.NotEnoughResources;
            if (!otherPlayer.CanAfford(offer))
                return ActionResult.NotEnoughResourcesOtherPlayer;
                
            currentPlayer.Pay(payment);
            otherPlayer.Pay(offer);
            currentPlayer.Receive(offer);
            otherPlayer.Receive(payment);
            return ActionResult.Success;
        }

        public ActionResult MoveRobber(Player player, HexTile hexTile, out List<Player> stealablePlayers)
        {
            stealablePlayers = new List<Player>();
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (GetCurrentPlayer() != player)
                return ActionResult.NotYourTurn;
            if (hexTile.HasRobber)
                return ActionResult.RobberAlreadyThere;
            if (Board.RobberTile != null)
                Board.RobberTile.HasRobber = false;

            Board.RobberTile = hexTile;
            hexTile.HasRobber = true;
            var adjacentVertices = BoardMappings.TileToVerticesAdjacencyMapping[hexTile.Index];
            foreach (var vertexIndex in adjacentVertices)
            {
                var vertex = Board.Vertices[vertexIndex];
                if (vertex.Owner != null && vertex.Owner != player && !stealablePlayers.Contains(vertex.Owner))
                {
                    stealablePlayers.Add(vertex.Owner);
                }
            }

            return ActionResult.Success;
        }
        public ActionResult StealResource(Player receiver, Player target)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            if (GetCurrentPlayer() != receiver)
                return ActionResult.NotYourTurn;

            var availableResources = target.Resources
                .Where(kv => kv.Value > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (!availableResources.Any())
                return ActionResult.NoResourcesToSteal;

            var randomResource = availableResources[Random.Shared.Next(availableResources.Count)];

            target.Resources[randomResource]--;
            receiver.Resources[randomResource]++;

            return ActionResult.Success;
        }

        public ActionResult DiscardResources(Player player, Dictionary<ResourceType, int> discardedCards)
        {
            if (Phase != GamePhase.MainGame)
                return ActionResult.WrongPhase;

            int totalResources = player.Resources.Values.Sum();

            if (totalResources <= 7)
                return ActionResult.NoDiscardNecessary;

            int requiredDiscard = totalResources / 2;

            int discardCount = discardedCards.Values.Sum();
            if (discardCount != requiredDiscard)
                return ActionResult.NotEnoughDiscardedCards;

            foreach (var kv in discardedCards)
            {
                if (!player.Resources.ContainsKey(kv.Key) || player.Resources[kv.Key] < kv.Value)
                    return ActionResult.NotEnoughResources;
            }

            foreach (var kv in discardedCards)
            {
                player.Resources[kv.Key] -= kv.Value;
            }

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
            if (dice != 7)
            {
                DistributeResources(dice);  
            }
            
        }

        public void DistributeResources(int diceNumber)
        {
            foreach (var tile in Board.Tiles)
            {
                if (tile.NumberToken != diceNumber || tile.Resource == null)
                    continue;
                if (tile.HasRobber)
                    continue;

                var vertexIndices = BoardMappings.TileToVerticesAdjacencyMapping[tile.Index];
                foreach (var vertexIndex in vertexIndices)
                {
                    var vertex = Board.Vertices[vertexIndex];
                    if (vertex.IsSettlement)
                    {
                        var settlement = Board.Settlements.FirstOrDefault(s => s.Vertex == vertex);
                        if (settlement != null)
                        {
                            settlement.Owner.Receive(new Dictionary<ResourceType, int>
                            {
                                { tile.Resource.Value, 1 }
                            });
                        }
                    }
                    else if (vertex.IsCity)
                    {
                        var city = Board.Cities.FirstOrDefault(c => c.Vertex == vertex);
                        if (city != null)
                        {
                            city.Owner.Receive(new Dictionary<ResourceType, int>
                            {
                                { tile.Resource.Value, 2 }
                            });
                        }
                    }
                }
            }
        }


        // =========================
        // Achievements
        // =========================

        private void UpdateLongestRoad(Player triggeringPlayer)
        {
            // TODO: DFS-based longest road calculation
        }
    }
}
