
# Simple Reaction Timer (Console)

A small .NET console “reaction time” game built around a simple state-machine controller. You insert a coin, press **GO**, wait for the prompt, then press **STOP** as quickly as you can.

The program currently runs the **Enhanced** controller, which plays **3 rounds** and then shows an **average**.

## Requirements

- .NET SDK that supports `net9.0`
- macOS / Windows / Linux (console app)

## Build

From the repo root:

```bash
dotnet build
```

Or build the project directly:

```bash
dotnet build SimpleReactionMachine/SimpleReactionMachine.csproj
```

## Run

```bash
dotnet run --project SimpleReactionMachine/SimpleReactionMachine.csproj
```

## Controls

- **SPACE**: Insert coin
- **ENTER**: GO / STOP
- **ESC**: Exit

## Game flow (Enhanced controller)

- Insert coin → shows `Press GO!`
- Press **ENTER** → shows `Wait...` for a random delay
- When timing starts, the display counts up from `0.00`
- Press **ENTER** to stop (or it auto-stops at `2.00s`)
- Repeats for **3 rounds**
- Shows `Average = X.XX` then returns to `Insert coin`

Notes:

- The internal timer ticks every **10ms** (so `1 tick = 0.01s`).
- If you press **ENTER** too early during `Wait...`, the game resets back to `Insert coin`.
- If you wait too long at `Press GO!`, it times out after **10 seconds**.

## Project structure

- `SimpleReactionMachine/SimpleReactionMachine.cs`: Entry point (`Main`) + console UI + timer loop
- `SimpleReactionMachine/EnhancedReactionController.cs`: 3-round mode with average
- `SimpleReactionMachine/SimpleReactionController.cs`: Single-round mode (simpler version)
- `SimpleReactionMachine/IController.cs`, `IGui.cs`, `IRandom.cs`: Small interfaces used to keep the controller testable/replaceable

## Switching controllers

`Main` currently uses:

```csharp
contoller = new EnhancedReactionController();
```

To try the simpler controller, change it to:

```csharp
contoller = new SimpleReactionController();
```

Then rebuild/run.

