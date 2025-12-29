using System;
using System.Collections.Generic;
using System.Linq;
using Catan.Shared.Game;
using Xunit;

namespace Catan.Tests.Game
{
    public class GameSessionTests
    {
        private List<Player> CreateFourPlayers() => new()
        {
            new Player("Alice"),
            new Player("Bob"),
            new Player("Charlie"),
            new Player("Dana")
        };

        private GameSession SetupToMainGame(out Player currentPlayer)
        {
            var players = CreateFourPlayers();
            var session = new GameSession(players);
            session.StartGame();

            // Advance through SetupRound1 and SetupRound2 to MainGame
            for (int i = 0; i < 2 * players.Count + 1; i++)
            {
                session.AdvanceSetupTurn();
            }

            currentPlayer = session.GetCurrentPlayer();
            return session;
        }

        [Fact]
        public void Constructor_ThrowsIfNotFourPlayers()
        {
            Assert.Throws<ArgumentException>(() => new GameSession(new List<Player>()));
            Assert.Throws<ArgumentException>(() => new GameSession(new List<Player> { new Player("A") }));
            Assert.Throws<ArgumentException>(() => new GameSession(new List<Player> { new Player("A"), new Player("B"), new Player("C") }));
        }

        [Fact]
        public void StartGame_SetsPhaseAndRandomizesPlayers()
        {
            var players = CreateFourPlayers();
            var session = new GameSession(players);
            session.StartGame();

            Assert.Equal(GameSession.GamePhase.SetupRound1, session.Phase);
            Assert.Equal(0, session.CurrentPlayerIndex);
            Assert.Contains(session.GetCurrentPlayer(), players);
        }

        [Fact]
        public void NextTurn_IncrementsPlayerAndTurnNumber()
        {
            var session = new GameSession(CreateFourPlayers());
            session.StartGame();

            var firstPlayer = session.GetCurrentPlayer();
            session.NextTurn();
            var secondPlayer = session.GetCurrentPlayer();

            Assert.NotEqual(firstPlayer, secondPlayer);
            Assert.Equal(1, session.TurnNumber);
            Assert.Equal(GameSession.TurnPhase.Roll, session.Turn);
        }

        [Fact]
        public void BuildRoad_MainGame_SuccessAndFailures()
        {
            var session = SetupToMainGame(out var player);

            // Give player enough resources
            player.Resources[ResourceType.Wood] = 2;
            player.Resources[ResourceType.Brick] = 2;

            // Build initial settlement for connection
            var v1 = session.Board.GetVertex(0);
            var v2 = session.Board.GetVertex(1);
            session.Board.BuildSettlement(player, v1, true);

            var edge = session.Board.Edges.FirstOrDefault(e => e.ContainsVertex(v1) && e.ContainsVertex(v2));
            if(edge == null)
            {
                edge = new Edge(v1, v2, session.Board.Edges.Count);
                session.Board.Edges.Add(edge);
            }

            var result = session.BuildRoad(player, edge, false);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Contains(player.Roads, r => r.Edge == edge);

            // Not enough resources
            player.Resources[ResourceType.Wood] = 0;
            player.Resources[ResourceType.Brick] = 0;
            var result2 = session.BuildRoad(player, edge, false);
            Assert.Equal(GameSession.ActionResult.NotEnoughResources, result2);
        }

        [Fact]
        public void BuildSettlement_MainGame_SuccessAndFailures()
        {
            var session = SetupToMainGame(out var player);

            // Give player enough resources
            player.Resources[ResourceType.Wood] = 2;
            player.Resources[ResourceType.Brick] = 2;
            player.Resources[ResourceType.Sheep] = 1;
            player.Resources[ResourceType.Wheat] = 1;

            var vertex = session.Board.GetVertex(0);
            var edge = session.Board.GetEdge(0);
            var road = session.BuildRoad(player, edge, false);
            var result = session.BuildSettlement(player, vertex);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Contains(player.Settlements, s => s.Vertex == vertex);

            // Cannot build on same vertex
            var result2 = session.BuildSettlement(player, vertex);
            Assert.Equal(GameSession.ActionResult.VertexOccupied, result2);
        }

        [Fact]
        public void UpgradeToCity_SuccessAndFailures()
        {
            var session = SetupToMainGame(out var player);

            player.Resources[ResourceType.Wheat] = 2;
            player.Resources[ResourceType.Stone] = 3;

            var vertex = session.Board.GetVertex(0);
            var settlement = session.Board.BuildSettlement(player, vertex, true);

            var result = session.UpgradeToCity(player, settlement);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.True(vertex.IsCity);
            Assert.Contains(player.Cities, c => c.Vertex == vertex);

            // Cannot upgrade non-existent settlement
            var otherPlayer = new Player("Bob");
            var fakeSettlement = new Settlement(otherPlayer, session.Board.GetVertex(1));
            otherPlayer.Resources[ResourceType.Wheat] = 2;
            otherPlayer.Resources[ResourceType.Stone] = 3;
            var result2 = session.UpgradeToCity(player, fakeSettlement);
            Assert.Equal(GameSession.ActionResult.SettlementDoesntExist, result2);
        }

        [Fact]
        public void BuyDevelopmentCard_SuccessAndEdgeCases()
        {
            var session = SetupToMainGame(out var player);

            player.Resources[ResourceType.Sheep] = 1;
            player.Resources[ResourceType.Wheat] = 1;
            player.Resources[ResourceType.Stone] = 1;

            var result = session.BuyDevelopmentCard(player);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Single(player.DevelopmentCards);

            // Not enough resources
            var result2 = session.BuyDevelopmentCard(player);
            Assert.Equal(GameSession.ActionResult.NotEnoughResources, result2);
        }

        [Fact]
        public void PlayDevelopmentCards_SuccessAndFailures()
        {
            var session = SetupToMainGame(out var player);

            var knightCard = new DevelopmentCard(DevelopmentCardType.Knight) { BoughtOnTurn = session.TurnNumber - 1 };
            player.DevelopmentCards.Add(knightCard);

            // Successful knight play
            var result = session.PlayKnightCard(player);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Equal(1, player.KnightsPlayed);

            // Already played
            var result2 = session.PlayKnightCard(player);
            Assert.Equal(GameSession.ActionResult.AlreadyPlayedDevelopmentCard, result2);
        }

        [Fact]
        public void MoveRobber_StealResource()
        {
            var session = SetupToMainGame(out var current);

            var target = session.Players.First(p => p != current);

            var tile = session.Board.Tiles.First(t => t.Resource != null);
            var vertex = session.Board.GetVertex(BoardMappings.TileToVerticesAdjacencyMapping[tile.Index][0]);
            session.Board.BuildSettlement(target, vertex, true);

            var result = session.MoveRobber(current, tile, out var stealable);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Contains(target, stealable);

            target.Resources[ResourceType.Wood] = 1;
            current.Resources[ResourceType.Wood] = 0;
            var stealResult = session.StealResource(current, target);
            Assert.Equal(GameSession.ActionResult.Success, stealResult);
            Assert.Equal(1, current.Resources[ResourceType.Wood]);
            Assert.Equal(0, target.Resources[ResourceType.Wood]);
        }

        [Fact]
        public void TradeWithBank_Port_Player()
        {
            var session = SetupToMainGame(out var player);

            session.RollDice();

            player.Resources[ResourceType.Wood] = 4;

            var result = session.TradeWithBank(player, ResourceType.Wood, ResourceType.Brick);
            Assert.Equal(GameSession.ActionResult.Success, result);
            Assert.Equal(0, player.Resources[ResourceType.Wood]);
            Assert.Equal(1, player.Resources[ResourceType.Brick]);
        }


        [Fact]
        public void DiceRoll_DistributesResources()
        {
            var session = SetupToMainGame(out var player);

            var tile = session.Board.Tiles.First(t => t.Resource != null);
            tile.NumberToken = 6;
            var vertex = session.Board.GetVertex(BoardMappings.TileToEdgesAdjacencyMapping[tile.Index][0]);
            session.Board.BuildSettlement(player, vertex, true);

            session.DistributeResources(6);

            Assert.Equal(1, player.Resources[tile.Resource.Value]);
        }
    }
}
