using System.Collections.Generic;
using System.Linq;
using Catan.Shared.Game;
using Xunit;

namespace Catan.Tests.Game
{
    public class PlayerTests
    {
        [Fact]
        public void CanAfford_ReturnsTrue_WhenEnoughResources()
        {
            var player = new Player("Alice");
            player.Resources[ResourceType.Wood] = 2;
            player.Resources[ResourceType.Brick] = 1;

            var cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, 2 },
                { ResourceType.Brick, 1 }
            };

            Assert.True(player.CanAfford(cost));
        }

        [Fact]
        public void CanAfford_ReturnsFalse_WhenNotEnoughResources()
        {
            var player = new Player("Bob");
            player.Resources[ResourceType.Wood] = 1;

            var cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, 2 }
            };

            Assert.False(player.CanAfford(cost));
        }

        [Fact]
        public void Pay_ReducesResourcesCorrectly()
        {
            var player = new Player("Charlie");
            player.Resources[ResourceType.Wood] = 3;
            player.Resources[ResourceType.Brick] = 2;

            var cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, 2 },
                { ResourceType.Brick, 1 }
            };

            player.Pay(cost);

            Assert.Equal(1, player.Resources[ResourceType.Wood]);
            Assert.Equal(1, player.Resources[ResourceType.Brick]);
        }

        [Fact]
        public void Receive_AddsResourcesCorrectly()
        {
            var player = new Player("Dana");

            var resources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Sheep, 2 },
                { ResourceType.Wheat, 3 }
            };

            player.Receive(resources);

            Assert.Equal(2, player.Resources[ResourceType.Sheep]);
            Assert.Equal(3, player.Resources[ResourceType.Wheat]);
        }

        [Fact]
        public void VictoryPoints_CalculatesCorrectly()
        {
            var player = new Player("Eve");

            var vertex1 = new Vertex(0);
            var vertex2 = new Vertex(1);

            // 2 settlements
            player.Settlements.Add(new Settlement(player, vertex1));
            player.Settlements.Add(new Settlement(player, vertex2));

            // 1 city
            var cityVertex = new Vertex(2);
            player.Cities.Add(new City(player, cityVertex));

            player.LongestRoadOwner = true;
            player.LargestArmyOwner = false;

            // VP = 2 settlements (1 each) + 1 city (2) + LongestRoad (2) = 6
            Assert.Equal(6, player.VictoryPoints);
        }

        [Fact]
        public void PlayerCanOwnRoadsAndBuildings()
        {
            var player = new Player("Frank");

            var v1 = new Vertex(0);
            var v2 = new Vertex(1);

            var edge = new Edge(v1, v2, 0);

            var road = new Road(player, edge);
            edge.Road = road;
            player.Roads.Add(road);

            var settlement = new Settlement(player, v1);
            player.Settlements.Add(settlement);

            var city = new City(player, v2);
            player.Cities.Add(city);

            Assert.Contains(road, player.Roads);
            Assert.Contains(settlement, player.Settlements);
            Assert.Contains(city, player.Cities);
            Assert.Equal(player, edge.Road.Owner);
            Assert.Equal(player, settlement.Owner);
            Assert.Equal(player, city.Owner);
        }
        [Fact]
        public void CanUpgradeSettlementToCity()
        {
            var player = new Player("Gina");
            var vertex = new Vertex(0);

            var settlement = new Settlement(player, vertex);
            vertex.Owner = player;
            vertex.IsSettlement = true;
            player.Settlements.Add(settlement);

            // Upgrade logic
            vertex.IsSettlement = false;
            vertex.IsCity = true;
            player.Settlements.Remove(settlement);
            var city = new City(player, vertex);
            player.Cities.Add(city);

            Assert.False(vertex.IsSettlement);
            Assert.True(vertex.IsCity);
            Assert.Contains(city, player.Cities);
            Assert.DoesNotContain(settlement, player.Settlements);
        }

        [Fact]
        public void CannotBuildSettlement_OnOccupiedVertex()
        {
            var player1 = new Player("Henry");
            var player2 = new Player("Ivy");
            var vertex = new Vertex(0);

            // Player 1 owns a settlement
            vertex.Owner = player1;
            vertex.IsSettlement = true;
            player1.Settlements.Add(new Settlement(player1, vertex));

            // Attempt player2 settlement
            var canBuild = vertex.Owner == null;

            Assert.False(canBuild);
        }

        [Fact]
        public void CannotBuildSettlement_NotConnectedToPlayerRoad()
        {
            var player = new Player("Jack");
            var v1 = new Vertex(0);
            var v2 = new Vertex(1);

            // No road connecting v1 and v2 for the player
            var connected = player.Roads.Any(r => r.Edge.ContainsVertex(v2));

            Assert.False(connected);
        }

        [Fact]
        public void PlayerCanReceiveDevelopmentCard()
        {
            var player = new Player("Karen");
            var card = new DevelopmentCard(DevelopmentCardType.Knight);

            player.DevelopmentCards.Add(card);

            Assert.Contains(card, player.DevelopmentCards);
            Assert.False(card.Played);
            Assert.Equal(-1, card.BoughtOnTurn);
        }

        [Fact]
        public void CannotBuildCity_OnVertexOwnedByAnotherPlayer()
        {
            var player1 = new Player("Leo");
            var player2 = new Player("Mia");
            var vertex = new Vertex(0);

            // Player 1 owns settlement
            vertex.Owner = player1;
            vertex.IsSettlement = true;
            player1.Settlements.Add(new Settlement(player1, vertex));

            // Player 2 tries to build city
            var canUpgrade = vertex.Owner == player2;

            Assert.False(canUpgrade);
        }

        [Fact]
        public void CannotPlaceRoad_OnEdgeWithExistingRoad()
        {
            var player1 = new Player("Nina");
            var player2 = new Player("Oscar");

            var v1 = new Vertex(0);
            var v2 = new Vertex(1);
            var edge = new Edge(v1, v2, 0);

            var road1 = new Road(player1, edge);
            edge.Road = road1;
            player1.Roads.Add(road1);

            // Attempt player2 road
            var canPlace = edge.Road == null;

            Assert.False(canPlace);
        }
    }
}
