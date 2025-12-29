using System;
using System.Collections.Generic;
using System.Linq;
using Catan.Shared.Game;
using Xunit;

namespace Catan.Tests.Game
{
    public class BoardTests
    {
        [Fact]
        public void Board_InitializesCorrectly()
        {
            var board = new Board();
            Assert.Equal(19, board.Tiles.Count);
            Assert.Contains(board.Tiles, t => t.Resource == null && t.HasRobber);

            Assert.Equal(54, board.Vertices.Count);
            foreach (var v in board.Vertices)
            {
                Assert.NotNull(v.AdjacentTiles);
                Assert.InRange(v.AdjacentTiles.Count, 1, 3);
            }

            Assert.Equal(72, board.Edges.Count);
            foreach (var e in board.Edges)
            {
                Assert.NotNull(e.VertexA);
                Assert.NotNull(e.VertexB);
            }

            Assert.NotEmpty(board.Ports);
        }

        [Fact]
        public void CanPlaceRoad_SuccessAndFailures()
        {
            var board = new Board();
            var player = new Player("Alice");

            var v1 = board.GetVertex(0);
            var v2 = board.GetVertex(1);
            var edge = new Edge(v1, v2, 0);
            board.Edges.Add(edge);

            Assert.Equal(Board.RoadPlacementResult.NotConnected, board.CanPlaceRoad(player, edge));

            var settlement = board.BuildSettlement(player, v1, true);
            Assert.Equal(Board.RoadPlacementResult.Success, board.CanPlaceRoad(player, edge));

            var road = board.PlaceRoad(player, edge);
            Assert.Equal(player, road.Owner);
            Assert.Equal(edge, road.Edge);
            Assert.Contains(road, board.Roads);

            Assert.Equal(Board.RoadPlacementResult.EdgeOccupied, board.CanPlaceRoad(player, edge));
        }

        [Fact]
        public void CanPlaceSettlement_RespectRules()
        {
            var board = new Board();
            var player = new Player("Bob");

            var vertex = board.GetVertex(0);

            Assert.Equal(Board.SettlementPlacementResult.Success, board.CanPlaceSettlement(player, vertex, true));
            var settlement = board.BuildSettlement(player, vertex, true);

            foreach (var adj in board.UnbuildableVertices)
                Assert.Contains(adj, board.UnbuildableVertices);

            Assert.Equal(Board.SettlementPlacementResult.VertexOccupied, board.CanPlaceSettlement(player, vertex, true));
        }

        [Fact]
        public void CannotBuildSettlement_IfVertexUnbuildable()
        {
            var board = new Board();
            var player = new Player("Charlie");

            var vertex = board.GetVertex(0);
            board.UnbuildableVertices.Add(vertex);

            Assert.Equal(Board.SettlementPlacementResult.VertexUnbuildable, board.CanPlaceSettlement(player, vertex, false));
        }

        [Fact]
        public void SettlementUpgrade_SuccessAndFailure()
        {
            var board = new Board();
            var player = new Player("Dana");

            var vertex = board.GetVertex(0);
            var settlement = board.BuildSettlement(player, vertex, true);

            var city = board.UpgradeSettlement(player, settlement);
            Assert.Contains(city, board.Cities);
            Assert.DoesNotContain(settlement, board.Settlements);
            Assert.True(vertex.IsCity);
            Assert.False(vertex.IsSettlement);

            var otherPlayer = new Player("Eve");
            var invalidSettlement = new Settlement(otherPlayer, board.GetVertex(1));
            var ex = Assert.Throws<InvalidOperationException>(() => board.UpgradeSettlement(player, invalidSettlement));
            Assert.Equal("Settlement does not exist.", ex.Message);

            var settlement2 = board.BuildSettlement(otherPlayer, board.GetVertex(2), true);
            var ex2 = Assert.Throws<InvalidOperationException>(() => board.UpgradeSettlement(player, settlement2));
            Assert.Equal("Wrong owner.", ex2.Message);
        }

        [Fact]
        public void PlaceRobber_MovesRobberCorrectly()
        {
            var board = new Board();

            var desert = board.Tiles.First(t => t.Resource == null);
            var hex = board.Tiles.First(t => t.Resource != null);

            Assert.True(desert.HasRobber);
            board.PlaceRobber(hex);

            Assert.False(desert.HasRobber);
            Assert.True(hex.HasRobber);
            Assert.Equal(hex, board.RobberTile);
        }

        [Fact]
        public void GetPlayersOnTile_ReturnsCorrectCounts()
        {
            var board = new Board();
            var player1 = new Player("Frank");
            var player2 = new Player("Gina");

            var tile = board.Tiles.First(t => t.Resource != null);
            var vertices = BoardMappings.TileToEdgesAdjacencyMapping[tile.Index];

            var v1 = board.GetVertex(vertices[0]);
            var v2 = board.GetVertex(vertices[1]);

            board.BuildSettlement(player1, v1, true);
            board.BuildSettlement(player2, v2, true);

            var results = board.GetPlayersOnTile(tile);

            var p1 = results.FirstOrDefault(r => r.player == player1);
            var p2 = results.FirstOrDefault(r => r.player == player2);

            Assert.Equal(1, p1.counts["settlement"]);
            Assert.Equal(1, p2.counts["settlement"]);
        }

        [Fact]
        public void ReshuffleBoard_UpdatesTilesAndVertices()
        {
            var board = new Board();

            var oldResources = board.Tiles.Select(t => t.Resource).ToList();

            board.ReshuffleBoard();

            Assert.Equal(19, board.Tiles.Count);
            Assert.Equal(54, board.Vertices.Count);

            foreach (var v in board.Vertices)
            {
                Assert.NotEmpty(v.AdjacentTiles);
            }

            Assert.Contains(true, board.Tiles.Select((t, i) => t.Resource != oldResources[i]));
        }
    }
}
