# Catan Debug Client

A lightweight Avalonia desktop app for exercising the Catan game logic locally. It runs a full in-memory game using `Catan.Shared` and exposes board interactions (place, roll, robber, end turn) without networking or matchmaking.

## Architecture

- Entry point: `Program` boots Avalonia and creates the `App`.
- `App` sets the main window to `Views.CatanView` (the default `MainWindow` is unused for this client).
- `CatanView` (XAML + code-behind) hosts the UI and wires all interactions to `CatanViewModel`.
- `CatanViewModel` owns a `GameSession` from `Catan.Shared` and drives all game rules:
  - setup phase placement, main phase turns, dice rolls, and robber flow
  - resource changes, victory points, and player state
- Board rendering is a fixed layout:
  - `HexTile`, `Edge`, `Vertex`, and `Port` are custom user controls
  - `CatanView` maps these controls to `HexTileModel`, `EdgeModel`, and `VertexModel` instances at startup
- Click handlers live in the controls (and in `CatanView` for buttons) and forward actions to the view model.

## Run It

Prerequisites:
- .NET SDK 10.0 (project targets `net10.0`)

From the repo root:

```powershell
# Run the debug client

dotnet run --project Catan/src/Catan.DebugClient/Catan.DebugClient.csproj
```

From the `Catan` folder:

```powershell
# Run the debug client

dotnet run --project src/Catan.DebugClient/Catan.DebugClient.csproj
```

Build only:

```powershell
# Build the debug client

dotnet build Catan/src/Catan.DebugClient/Catan.DebugClient.csproj
```

## Controls and Flow

- Board clicks:
  - Click a highlighted vertex to place a settlement or upgrade a city.
  - Click a highlighted edge to place a road.
  - Click a highlighted tile when moving the robber.
- Bottom action bar:
  - Roads, Settlements, Cities, Dev Cards buttons trigger placement modes.
  - Roll Dice and End Turn control turn flow.
- Robber overlay:
  - If a 7 is rolled, discard selection appears for the current player.
  - Robber move highlights valid tiles, then steal options appear if needed.

## Notes and Limits

- This client runs fully offline and does not connect to `Catan.Server`.
- Players are created locally with fixed names (PlayerA to PlayerD).
- The board layout is hard-coded in XAML and in the Catan view constructor.
- Development cards and trading are stubbed or not implemented yet.
