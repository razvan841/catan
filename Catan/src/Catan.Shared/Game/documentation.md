# Catan Game Logic – Technical Documentation

## 1. Overview

This codebase provides a **rules-and-state engine for a 4-player game of Catan**. It:

- Maintains all authoritative game state (board, players, turn, resources, etc.).
- Enforces rules for building, trading, dice rolling, development cards, robber, and victory conditions.
- Does **not** provide:
  - A user interface  
  - Networking  
  - Animations  
  - Move suggestions  
  - AI players  

A client (UI, server, or API layer) is expected to:

1. Create a `GameSession`
2. Call its methods in the correct order
3. Render the board and player states based on the public properties
4. Handle player input and display `ActionResult` values

---

## 2. Core Architecture

### Main Entry Point: `GameSession`

`GameSession` is the central class that:

- Owns the game state
- Controls turn order
- Validates all player actions
- Updates victory conditions
- Coordinates board changes

A game is always played with **exactly 4 players**. Can be easily changed to 3.

---

### Key Enums in `GameSession`

#### `GamePhase`

Represents the overall stage of the game:

| Phase | Meaning |
|------|---------|
| `NotStarted` | Game created but not started |
| `RandomizingOrder` | Players are being shuffled |
| `SetupRound1` | First placement round (forward order) |
| `SetupRound2` | Second placement round (reverse order) |
| `MainGame` | Normal gameplay |
| `EndGame` | Game finished |

#### `TurnPhase`

Represents what the current player can do in their turn:

| Phase | Meaning |
|------|---------|
| `NotStarted` | No active turn |
| `Roll` | Player must roll dice |
| `Trade` | Player may trade |
| `Build` | Player may build / play cards |

#### `ActionResult`

Every action returns one of these values instead of throwing exceptions.  
This allows the UI to show proper error messages.

Examples:

- `Success`
- `NotYourTurn`
- `WrongPhase`
- `NotEnoughResources`
- `VertexOccupied`
- `EdgeOccupied`
- `NoCardsLeft`
- `NoPortAccess`
- etc.

---

## 3. Core Game Objects

### Player

Each `Player` has:

- `Username`
- `Resources` → Dictionary of:
  - Wood, Brick, Sheep, Wheat, Stone
- `Roads`
- `Settlements`
- `Cities`
- `Ports`
- `DevelopmentCards`
- `KnightsPlayed`
- `LongestRoad`
- Flags for:
  - `LongestRoadOwner`
  - `LargestArmyOwner`

#### Victory Points (computed property)

A player’s score is:

- 1 point per Settlement
- 2 points per City
- +2 if they have Longest Road
- +2 if they have Largest Army
- 1 point for each VP Dev Card

### Board

The `Board` contains the physical game layout:

- 19 `HexTile`s (resource tiles)
- 54 `Vertex` (settlement/city spots)
- 72 `Edge`s (roads)
- `Road`, `Settlement`, `City` lists
- `Ports`
- The current `RobberTile`

#### Board is responsible for:

- Generating a valid random board
- Ensuring no adjacent 6s and 8s (you can add extra rules for board generation - good luck!)
- Tracking:
  - Where roads are placed
  - Where settlements and cities are
  - Which vertices are unbuildable (due to distance rule)
- Validating placement rules

---

### HexTile

Each tile has:

- `Resource` (Brick, Wood, Sheep, Wheat, Stone, or Sand)
- `NumberToken` (2–12, except 7)
- `HasRobber`

---

### Vertex

Represents a corner where settlements/cities can be built.

Properties:
- `Index` (0–53)
- `AdjacentTiles`
- `ConnectedEdges`
- `Owner`
- `IsSettlement`
- `IsCity`
- `Port` (if applicable)

---

### Edge

Represents where a road can be placed.

Has:
- `VertexA`
- `VertexB`
- `Road` (if occupied)

---

### Port

Represents a trading port.

Types:
- `Generic` (3:1 trade)
- `Brick`, `Wood`, `Sheep`, `Wheat`, `Stone` (2:1 trade)

---

### DevelopmentDeck

Standard Catan deck:

- 14 Knight
- 5 Victory Point
- 2 Road Building
- 2 Year of Plenty
- 2 Monopoly

Shuffled with a cryptographic RNG.

---

## 4. Game Lifecycle (How to Actually Run a Game)

### Step 1 — Create the Game

```csharp
var players = new List<Player>
{
    new Player("Alice"),
    new Player("Bob"),
    new Player("Charlie"),
    new Player("Diana")
};

var game = new GameSession(players);
game.StartGame();
```

What this does:

- Randomizes player order

- Sets Phase = SetupRound1

- Sets CurrentPlayerIndex = 0

### Step 2 - Setup Phase (Initial Placements)

Each player must:

Place a Settlement → BuildInitialSettlement

Place a Road → BuildInitialRoad

AdvanceSetupTurn() is called after each player places their initial road, so you don't need to worry about the whose turn it is

First round:

Players go 0 → 1 → 2 → 3

Second round:

Players go 3 → 2 → 1 → 0

During SetupRound2, when a player places their second settlement, they receive resources from adjacent tiles.

Example:

game.BuildInitialSettlement(player, someVertex);
game.BuildInitialRoad(player, someEdge);


After second round ends, the game moves to MainGame.

### Step 3 - Main Game Turn Flow

Each turn follows this sequence:

1. Roll Dice
var (dice, distribution) = game.RollDice();
If dice == 7 → robber event (handled separately)

Otherwise, resources are automatically distributed to all players with settlements/cities adjacent to tiles with that number.

After rolling:

game.Turn == TurnPhase.Trade

2. Players may trade with:

Bank (4:1)
game.TradeWithBank(player, ResourceType.Wood, ResourceType.Brick);

Port (2:1 or 3:1)
game.TradeWithPort(player, ResourceType.Wood, ResourceType.Brick, PortType.Wood);

Other Players
game.TradeWithPlayer(playerA, playerB, payment, offer);

3. Build Phase

Player can:
Build Road
game.BuildRoad(player, edge, free:false);

Build Settlement
game.BuildSettlement(player, vertex);

Upgrade to City
game.UpgradeToCity(player, settlement);

Buy Development Card
game.BuyDevelopmentCard(player);

Player can no longer trade after entering building phase of the turn

## 5. Dev Cards

Knight
game.PlayKnightCard(player);


Effects:

Increases KnightsPlayed

May grant Largest Army

May trigger victory

You have to handle later where the robber will be placed

Road Building
game.PlayRoadBuildingCard(player, edge1, edge2);


Places two free roads. You will have to ask the user to provide the edges on which to build the roads. 

Monopoly
game.PlayMonopolyCard(player, ResourceType.Wood);


Steals all of that resource from all other players.

Year of Plenty
game.PlayYearOfPlentyCard(player, ResourceType.Wood, ResourceType.Brick);


Gives 2 free resources.

## 6. Robber

If a player rolls a 7, the UI should:

Ask players with >7 cards to discard half:

game.DiscardResources(player, discardedCards);


Let current player move the robber:

var result = game.MoveRobber(player, newTile, out var stealablePlayers);


Let them steal from one of the adjacent players:

game.StealResource(player, targetPlayer);

## 7. Longest Road & Largest Army

These are automatically tracked:

Longest Road: updated whenever a road is built

Largest Army: updated whenever a Knight is played

Both grant +2 victory points.

## 8. Victory & End Game

After any action that could change victory points, the game checks:

CheckVictory(player);


If a player reaches 10 points, the game ends:

Phase = EndGame

Winner is set

FinalStandings is computed

EndedAt is recorded

## 9. How a UI Should Use This

A minimal game UI should display:

Board (Board.Tiles, Board.Vertices, Board.Edges)

Player resources (player.Resources)

Player structures (player.Roads, Settlements, Cities)

Current turn (game.GetCurrentPlayer())

Game phase (game.Phase, game.Turn)

Call methods in this order:

StartGame()

Setup:

BuildInitialSettlement

BuildInitialRoad

Loop turns:

RollDice

Trade methods (optional)

Build / Play cards

NextTurn()

## Examples

### Creating and Starting a Game
```csharp
var players = new List<Player>
{
    new Player("Alice"),
    new Player("Bob"),
    new Player("Charlie"),
    new Player("Diana")
};

var game = new GameSession(players);
game.StartGame();
```


### Getting Core UI State

```csharp
Player current = game.GetCurrentPlayer();
GamePhase phase = game.Phase;
TurnPhase turn = game.Turn;
int turnNumber = game.TurnNumber;
Board board = game.Board;

// Resources
Dictionary<ResourceType, int> resources = player.Resources;

// Structures
List<Road> roads = player.Roads;
List<Settlement> settlements = player.Settlements;
List<City> cities = player.Cities;

// Achievements
bool hasLongestRoad = player.LongestRoadOwner;
bool hasLargestArmy = player.LargestArmyOwner;
int victoryPoints = player.VictoryPoints;

var result1 = game.BuildInitialSettlement(player, selectedVertex);
var result2 = game.BuildInitialRoad(player, selectedEdge);

var (dice, distribution) = game.RollDice();

// Example: show what each player received
foreach (var entry in distribution)
{
    Player p = entry.Key;
    Dictionary<ResourceType, int> gained = entry.Value;
}

var result = game.TradeWithBank(player, ResourceType.Wood, ResourceType.Brick);
var result = game.TradeWithPort(player, ResourceType.Wood, ResourceType.Brick, PortType.Wood);

var payment = new Dictionary<ResourceType, int>
{
    { ResourceType.Wood, 2 }
};

var offer = new Dictionary<ResourceType, int>
{
    { ResourceType.Brick, 1 }
};

var result = game.TradeWithPlayer(playerA, playerB, payment, offer);


var result = game.BuildRoad(player, selectedEdge, free: false);
var result = game.BuildSettlement(player, selectedVertex);
var result = game.UpgradeToCity(player, existingSettlement);
var result = game.BuyDevelopmentCard(player);

var result = game.MoveRobber(player, selectedTile, out var stealablePlayers);
var result = game.StealResource(player, stealablePlayers.First());

// Discard Resources (when rolling 7)
var discarded = new Dictionary<ResourceType, int>
{
    { ResourceType.Wood, 2 },
    { ResourceType.Brick, 1 }
};

var result = game.DiscardResources(player, discarded);

game.NextTurn();

if (game.Phase == GamePhase.EndGame)
{
    Player winner = game.Winner;
    IReadOnlyList<Player> standings = game.FinalStandings;
    DateTime? endedAt = game.EndedAt;
}

List<HexTile> tiles = game.Board.Tiles;
List<Vertex> vertices = game.Board.Vertices;
List<Edge> edges = game.Board.Edges;

// Check if a vertex has a settlement or city
bool hasSettlement = vertex.IsSettlement;
bool hasCity = vertex.IsCity;
Player? owner = vertex.Owner;

// Check if an edge has a road
bool hasRoad = edge.Road != null;
Player? roadOwner = edge.Road?.Owner;
```