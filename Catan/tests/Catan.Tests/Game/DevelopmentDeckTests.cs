using System.Linq;
using Catan.Shared.Game;
using Xunit;

namespace Catan.Tests.Game
{
    public class DevelopmentDeckTests
    {
        [Fact]
        public void CreateStandardDeck_HasCorrectCardCounts()
        {
            var deck = DevelopmentDeck.CreateStandardDeck();
            Assert.Equal(25, deck.Count);

            var cards = deck.DrawAll().ToList();
            Assert.Equal(14, cards.Count(c => c.Type == DevelopmentCardType.Knight));
            Assert.Equal(5, cards.Count(c => c.Type == DevelopmentCardType.VictoryPoint));
            Assert.Equal(2, cards.Count(c => c.Type == DevelopmentCardType.RoadBuilding));
            Assert.Equal(2, cards.Count(c => c.Type == DevelopmentCardType.YearOfPlenty));
            Assert.Equal(2, cards.Count(c => c.Type == DevelopmentCardType.Monopoly));
        }

        [Fact]
        public void Draw_ReturnsCardAndReducesCount()
        {
            var deck = DevelopmentDeck.CreateStandardDeck();
            int initialCount = deck.Count;

            var card = deck.Draw();

            Assert.NotNull(card);
            Assert.Equal(initialCount - 1, deck.Count);
        }

        [Fact]
        public void Draw_EmptyDeck_ReturnsNull()
        {
            var deck = DevelopmentDeck.CreateStandardDeck();

            while (deck.Draw() != null) { }

            Assert.Null(deck.Draw());
            Assert.Equal(0, deck.Count);
        }

        [Fact]
        public void DevelopmentCard_InitialProperties()
        {
            var card = new DevelopmentCard(DevelopmentCardType.Knight);

            Assert.Equal(DevelopmentCardType.Knight, card.Type);
            Assert.False(card.Played);
            Assert.Equal(-1, card.BoughtOnTurn);
        }

        [Fact]
        public void DevelopmentCard_CanBePlayedAndBought()
        {
            var card = new DevelopmentCard(DevelopmentCardType.Monopoly);

            card.Played = true;
            card.BoughtOnTurn = 3;

            Assert.True(card.Played);
            Assert.Equal(3, card.BoughtOnTurn);
        }
    }

    internal static class DevelopmentDeckExtensions
    {
        public static IEnumerable<DevelopmentCard> DrawAll(this DevelopmentDeck deck)
        {
            DevelopmentCard? card;
            while ((card = deck.Draw()) != null)
            {
                yield return card;
            }
        }
    }
}
