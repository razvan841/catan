using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Media;
using Catan.Shared.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Catan.DebugClient.Views
{
    public enum PlacementType
    {
        None,
        Settlement,
        City,
        Road
    }
    
    public class CatanViewModel : ViewModelBase
    {
        public GameSession Game { get;}
        public Dictionary<Player, IBrush> PlayerColors { get; } = new();

        public PlacementType CurrentPlacement { get; private set; } = PlacementType.None;

        public ObservableCollection<VertexModel> AllVertices { get; set; } = new();

        private string _gameLog = "";
        public string GameLog
        {
            get => _gameLog;
            set { _gameLog = value; RaisePropertyChanged(nameof(GameLog)); }
        }

        public void AppendToGameLog(string message)
        {
            GameLog += message + "\n";
        }

        public CatanViewModel(GameSession game)
        {
            Game = game;
            // Link colors to each player
            var colors = new[] { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange };
            for (int i = 0; i < Game.Players.Count; i++)
                PlayerColors[Game.Players[i]] = colors[i];

            // Link UI vertex to Game vertex
            foreach (var vertex in Game.Board.Vertices)
            {
                AllVertices.Add(new VertexModel(vertex, PlayerColors));
            }

            AppendToGameLog($"Player {CurrentPlayerName}'s turn to build initial settlement!");
        }

        public string CurrentPlayerName => Game.GetCurrentPlayer().Username;
        public string Player1Name => Game.Players[0].Username;
        public string Player2Name => Game.Players[1].Username;
        public string Player3Name => Game.Players[2].Username;
        public string Player4Name => Game.Players[3].Username;

        public int Player1Cards => Game.Players[0].Resources.Values.Sum();
        public int Player2Cards => Game.Players[1].Resources.Values.Sum();
        public int Player3Cards => Game.Players[2].Resources.Values.Sum();
        public int Player4Cards => Game.Players[3].Resources.Values.Sum();

        public int Player1DevCards => Game.Players[0].DevelopmentCards.Count;
        public int Player2DevCards => Game.Players[1].DevelopmentCards.Count;
        public int Player3DevCards => Game.Players[2].DevelopmentCards.Count;
        public int Player4DevCards => Game.Players[3].DevelopmentCards.Count;

        public int Player1Knights => Game.Players[0].KnightsPlayed;
        public int Player2Knights => Game.Players[1].KnightsPlayed;
        public int Player3Knights => Game.Players[2].KnightsPlayed;
        public int Player4Knights => Game.Players[3].KnightsPlayed;

        public int Player1Points => Game.Players[0].VictoryPoints;
        public int Player2Points => Game.Players[1].VictoryPoints;
        public int Player3Points => Game.Players[2].VictoryPoints;
        public int Player4Points => Game.Players[3].VictoryPoints;

        public IBrush Player1Border => IsCurrentPlayer(0) ? Brushes.Red : Brushes.Gray;
        public IBrush Player2Border => IsCurrentPlayer(1) ? Brushes.Red : Brushes.Gray;
        public IBrush Player3Border => IsCurrentPlayer(2) ? Brushes.Red : Brushes.Gray;
        public IBrush Player4Border => IsCurrentPlayer(3) ? Brushes.Red : Brushes.Gray;

        private bool IsCurrentPlayer(int index)
        {
            return Game.GetCurrentPlayer() == Game.Players[index];
        }

        public string Wheat => Game.GetCurrentPlayer().Resources[ResourceType.Wheat].ToString();
        public string Sheep => Game.GetCurrentPlayer().Resources[ResourceType.Sheep].ToString();
        public string Stone => Game.GetCurrentPlayer().Resources[ResourceType.Stone].ToString();
        public string Brick => Game.GetCurrentPlayer().Resources[ResourceType.Brick].ToString();
        public string Wood => Game.GetCurrentPlayer().Resources[ResourceType.Wood].ToString();

        // --- Commands for Buttons ---

        public async void RollDice()
        {
            var roll = Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);

            // TODO: call your board/resource distribution logic here

            Game.Turn = GameSession.TurnPhase.Trade;

            // Notify UI that things changed
            RaisePropertyChanged(nameof(Wheat));
            RaisePropertyChanged(nameof(Sheep));
            RaisePropertyChanged(nameof(Stone));
            RaisePropertyChanged(nameof(Brick));
            RaisePropertyChanged(nameof(Wood));
        }

        public void OnSettlementButtonClicked()
        {
            CurrentPlacement = PlacementType.Settlement;
            AppendToGameLog($"{CurrentPlayerName} is placing a settlement. Click a highlighted vertex.");

            HighlightInitialValidVertices();
        }

        public void RefreshPlayers()
        {
            RaisePropertyChanged(nameof(Player1Cards));
            RaisePropertyChanged(nameof(Player2Cards));
            RaisePropertyChanged(nameof(Player3Cards));
            RaisePropertyChanged(nameof(Player4Cards));

            RaisePropertyChanged(nameof(Player1DevCards));
            RaisePropertyChanged(nameof(Player2DevCards));
            RaisePropertyChanged(nameof(Player3DevCards));
            RaisePropertyChanged(nameof(Player4DevCards));

            RaisePropertyChanged(nameof(Player1Knights));
            RaisePropertyChanged(nameof(Player2Knights));
            RaisePropertyChanged(nameof(Player3Knights));
            RaisePropertyChanged(nameof(Player4Knights));

            RaisePropertyChanged(nameof(Player1Points));
            RaisePropertyChanged(nameof(Player2Points));
            RaisePropertyChanged(nameof(Player3Points));
            RaisePropertyChanged(nameof(Player4Points));
        }

        private IBrush GetPlayerColor(Player player)
        {
            var index = Game.Players.IndexOf(player);
            return index switch
            {
                0 => Brushes.Blue,
                1 => Brushes.Green,
                2 => Brushes.Red,
                3 => Brushes.Orange,
                _ => Brushes.Gray
            };
        }

        // --- SETTLEMENT PLACEMENT LOGIC ---

        private void HighlightInitialValidVertices()
        {
            foreach (var vertex in AllVertices)
            {
                vertex.IsHighlightable = CurrentPlacement == PlacementType.Settlement &&
                                         CanBuildInitialSettlement(vertex.GameVertex);
            }
        }

        public void TryPlaceSettlement(VertexModel vertexModel)
        {
            if (CurrentPlacement != PlacementType.Settlement)
                return;
            if(Game.Phase == GameSession.GamePhase.SetupRound1 || Game.Phase == GameSession.GamePhase.SetupRound2)
            {
                if (CanBuildInitialSettlement(vertexModel.GameVertex))
                {
                    vertexModel.GameVertex.IsSettlement = true;
                    vertexModel.GameVertex.Owner = Game.GetCurrentPlayer();

                    AppendToGameLog($"{CurrentPlayerName} built a settlement at vertex #{vertexModel.GameVertex.Index}");

                    CurrentPlacement = PlacementType.None;
                }
                else
                {
                    AppendToGameLog($"{CurrentPlayerName} cannot build a settlement at vertex #{vertexModel.GameVertex.Index}. Press 'Settlement' again to retry.");
                    CurrentPlacement = PlacementType.None;
                }
            }
            else
            {
                if (CanBuildSettlement(vertexModel.GameVertex))
                {
                    vertexModel.GameVertex.IsSettlement = true;
                    vertexModel.GameVertex.Owner = Game.GetCurrentPlayer();

                    AppendToGameLog($"{CurrentPlayerName} built a settlement at vertex #{vertexModel.GameVertex.Index}");

                    CurrentPlacement = PlacementType.None;
                }
                else
                {
                    AppendToGameLog($"{CurrentPlayerName} cannot build a settlement at vertex #{vertexModel.GameVertex.Index}. Press 'Settlement' again to retry.");
                    CurrentPlacement = PlacementType.None;
                }
            }
            
            foreach (var v in AllVertices)
                v.IsHighlightable = false;

            RaisePropertyChanged(nameof(AllVertices));
        }

        private bool CanBuildInitialSettlement(Shared.Game.Vertex vertex)
        {
            var result = Game.Board.CanPlaceSettlement(Game.GetCurrentPlayer(), vertex, true);
            return result == Board.SettlementPlacementResult.Success;
        }

        private bool CanBuildSettlement(Shared.Game.Vertex vertex)
        {
            var result = Game.Board.CanPlaceSettlement(Game.GetCurrentPlayer(), vertex, false);
            return result == Board.SettlementPlacementResult.Success;
        }
    }
}