using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using Catan.Shared.Game;

namespace Catan.DebugClient.Views
{
    public partial class CatanView : Window
    {
        private readonly CatanViewModel vm;

        public CatanView()
        {
            InitializeComponent();

            var players = new List<Player>
            {
                new Player("PlayerA"),
                new Player("PlayerB"),
                new Player("PlayerC"),
                new Player("PlayerD"),
            };

            var session = new GameSession(players);
            session.StartGame();

            vm = new CatanViewModel(session);
            DataContext = vm;

            var allVertexControls = HexBoard.Children.OfType<Vertex>().ToList();
            for (int i = 0; i < allVertexControls.Count; i++)
            {
                var model = vm.AllVertices[i];
                allVertexControls[i].VertexModel = model;
                allVertexControls[i].DataContext = model;
            }

            BtnSettlements.Click += (sender, e) => vm.OnSettlementButtonClicked();

            vm.RefreshPlayers();
        }
    }
}
