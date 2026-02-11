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

            var allHexControls = HexBoard.Children.OfType<HexTile>().ToList();
            for (int i = 0; i < allHexControls.Count; i++)
            {
                var model = vm.AllTiles[i];
                allHexControls[i].HexModel = model;
                allHexControls[i].DataContext = model;
            }

            var allEdgeControls = new List<Edge>
            {
                edge1, edge2, edge3, edge4, edge5, edge6,
                edge7, edge8, edge9, edge10, edge11, edge12,
                edge13, edge14, edge15, edge16, edge17, edge18,
                edge19, edge20, edge21, edge22, edge23, edge24,
                edge25, edge26, edge27, edge28, edge29, edge30,
                edge31, edge32, edge33, edge34, edge35, edge36,
                edge37, edge38, edge39, edge40, edge41, edge42,
                edge43, edge44, edge45, edge46, edge47, edge48,
                edge49, edge50, edge51, edge52, edge53, edge54,
                edge55, edge56, edge57, edge58, edge59, edge60,
                edge61, edge62, edge63, edge64, edge65, edge66,
                edge67, edge68, edge69, edge70, edge71, edge72
            };

            var edgeCoordinates = new (double X1, double Y1, double X2, double Y2)[]
            {
                (195, 22, 147.5, 50), (195, 20, 247.5, 50),
                (295, 22, 247.5, 50), (295, 20, 347.5, 50),
                (395, 22, 347.5, 50), (395, 20, 447.5, 50),
                (147.5, 50, 147.5, 107.5), (247.5, 50, 247.5, 107.5),
                (347.5, 50, 347.5, 107.5), (447.5, 50, 447.5, 107.5),
                (147.5, 107.5, 95, 135.5), (147.5, 107.5, 195, 132.5),
                (247.5, 107.5, 195, 135.5), (247.5, 107.5, 295, 132.5),
                (347.5, 107.5, 295, 135.5), (347.5, 107.5, 395, 132.5),
                (447.5, 107.5, 395, 135.5), (447.5, 107.5, 495, 132.5),
                (100, 135.5, 100, 193), (200, 135.5, 200, 193),
                (300, 135.5, 300, 193), (400, 135.5, 400, 193),
                (500, 135.5, 500, 193), (100, 193, 47.5, 221),
                (100, 193, 147.5, 221), (200, 193, 147.5, 221),
                (200, 193, 247.5, 221), (300, 193, 247.5, 221),
                (300, 193, 347.5, 221), (400, 193, 347.5, 221),
                (400, 193, 447.5, 221), (500, 193, 447.5, 221),
                (500, 193, 547.5, 221), (47.5, 221, 47.5, 276.5),
                (147.5, 221, 147.5, 276.5), (247.5, 221, 247.5, 276.5),
                (347.5, 221, 347.5, 276.5), (447.5, 221, 447.5, 276.5),
                (547.5, 221, 547.5, 276.5), (47.5, 276.5, 100, 304.5),
                (147.5, 276.5, 100, 304.5), (147.5, 276.5, 200, 304.5),
                (247.5, 276.5, 200, 304.5), (247.5, 276.5, 300, 304.5),
                (347.5, 276.5, 300, 304.5), (347.5, 276.5, 400, 304.5),
                (447.5, 276.5, 400, 304.5), (447.5, 276.5, 500, 304.5),
                (547.5, 276.5, 500, 304.5), (100, 304.5, 100, 362),
                (200, 304.5, 200, 362), (300, 304.5, 300, 362), (400, 304.5, 400, 362),
                (500, 304.5, 500, 362), (100, 362, 152.5, 390), (200, 362, 152.5, 390),
                (200, 362, 252.5, 390), (300, 362, 252.5, 390), (300, 362, 352.5, 390),
                (400, 362, 352.5, 390), (400, 362, 452.5, 390), (500, 362, 452.5, 390),
                (152.5, 390, 152.5, 447), (252.5, 390, 252.5, 447),
                (352.5, 390, 352.5, 447), (452.5, 390, 452.5, 447),
                (152.5, 452, 205, 480), (252.5, 447, 205, 475), (252.5, 452, 305, 480),
                (352.5, 447, 305, 475), (352.5, 452, 405, 480), (452.5, 447, 405, 475)

            };
            
            for (int i = 0; i < allEdgeControls.Count; i++)
            {
                var control = allEdgeControls[i];
                var gameEdge = vm.AllEdges[i].GameEdge;

                var model = vm.AllEdges[i];
                model.StartPoint = new Avalonia.Point(edgeCoordinates[i].X1, edgeCoordinates[i].Y1);
                model.EndPoint = new Avalonia.Point(edgeCoordinates[i].X2, edgeCoordinates[i].Y2);

                model.OnClicked = (edgeModel) =>
                {
                    vm.TryPlaceRoad(edgeModel);
                };

                control.DataContext = model;
            }

            BtnSettlements.Click += (sender, e) => vm.OnSettlementButtonClicked();
            BtnRoads.Click += (sender, e) => vm.OnRoadButtonClicked();
            BtnCities.Click += (sender, e) => vm.OnCityButtonClicked();
            BtnDevCards.Click += (sender, e) => vm.OnDevCardButtonClicked();
            RollDiceButton.Click += (sender, e) => vm.OnRollDiceClicked();
            EndTurnButton.Click += (sender, e) => vm.EndTurn();

            vm.RefreshPlayers();
        }
    }
}
