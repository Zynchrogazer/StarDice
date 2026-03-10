# Singleton Audit

Scope: `Assets/0StarDice0/Scripts`
## Summary

- Total singleton-style `Instance` declarations found (initial audit): **22**
- Method: search `public static ... Instance` and count `ClassName.Instance` cross-file references in C# files.

## Progress update (current)

- Removed singleton `Instance` from:
  - `DeckData`
  - `CameraController`
  - `PlayerInventory`
  - `PlayerCardInventory`
  - `ShopManager`
  - `CharacterSelectManager`
  - `NormaUIManager`
  - `EventManager`
  - `GameManagerLevel1`
  - `GameManagerLevel2`
  - `GameManagerLevel3`
  - `PassiveSkillTooltip`
  - `ScoreManager`
  - `PlayerStatAggregator`
  - `PassiveSkillManager`
  - `SkillManager`
- Remaining singleton-style `Instance` declarations: **6** (excluding commented code).
## Singleton list (initial snapshot)

| Class | File | `Class.Instance` refs in `.cs` | Referenced from other files | Notes |
|---|---|---:|---:|---|
| `DeckManager` | `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckManager.cs` | 5243 | 57 | Core flow / high coupling |
| `DeckData` | `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckData.cs` | 0 | 0 | Candidate for removal/refactor first (no direct code usage) |
| `CameraController` | `Assets/0StarDice0/Scripts/Code/MainGame/_CameraManager/CameraController.cs` | 0 | 0 | Candidate for removal/refactor first (no direct code usage) |
| `PassiveSkillTooltip` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/PassiveSkillTooltip.cs` | 4 | 1 | Low coupling, good early refactor target |
| `SkillManager` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/SkillManager.cs` | 14 | 3 | Core flow / high coupling |
| `PassiveSkillManager` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/PassiveSkillManager.cs` | 14 | 2 | Low coupling, good early refactor target |
| `NormaSystem` | `Assets/0StarDice0/Scripts/Code/MainGame/_Events/NormaSystem.cs` | 14 | 4 | Core flow / high coupling |
| `NormaUIManager` | `Assets/0StarDice0/Scripts/Code/MainGame/_Events/NormaUIManager.cs` | 6 | 1 | Low coupling, good early refactor target |
| `GameEventManager` | `Assets/0StarDice0/Scripts/Code/MainGame/_Events/GameEventManager.cs` | 90 | 41 | Core flow / high coupling |
| `PlayerInventory` | `Assets/0StarDice0/Scripts/Code/MainGame/_Player/PlayerInventory.cs` | 0 | 0 | Candidate for removal/refactor first (no direct code usage) |
| `EventManager` | `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/EventManager.cs` | 7 | 2 | Low coupling, good early refactor target |
| `PlayerStatAggregator` | `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/PlayerStatAggregator.cs` | 8 | 3 | Core flow / high coupling |
| `GameTurnManager` | `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/GameTurnManager.cs` | 61 | 12 | Core flow / high coupling |
| `PlayerCardInventory` | `Assets/0StarDice0/Scripts/Code/MainGame/CardMain/PlayerCardInventory.cs` | 6 | 2 | Low coupling, good early refactor target |
| `ShopManager` | `Assets/0StarDice0/Scripts/Code/MainGame/CardMain/ShopManager.cs` | 5 | 2 | Low coupling, good early refactor target |
| `DiceRollerFromPNG` | `Assets/0StarDice0/Scripts/Code/MainGame/dice panel/DiceRollerFromPNG.cs` | 16 | 6 | Core flow / high coupling |
| `RouteManager` | `Assets/0StarDice0/Scripts/Code/MainGame/_RouteManager/RouteManager.cs` | 13 | 4 | Core flow / high coupling |
| `GameManagerLevel1` | `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/GameManagerLevel1.cs` | 2 | 1 | Low coupling, good early refactor target |
| `GameManagerLevel2` | `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/GameManagerLevel2.cs` | 2 | 1 | Low coupling, good early refactor target |
| `GameManagerLevel3` | `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/GameManagerLevel3.cs` | 2 | 1 | Low coupling, good early refactor target |
| `ScoreManager` | `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/ScoreManager.cs` | 12 | 3 | Core flow / high coupling |
| `CharacterSelectManager` | `Assets/0StarDice0/Scripts/Code/Test/TestFight/CharacterSelectManager.cs` | 2 | 1 | Low coupling, good early refactor target |

## Suggested removal/refactor order

1. **Immediate candidates**: `DeckData`, `CameraController`, `PlayerInventory` (0 cross-file `Class.Instance` usage).

2. **Low-coupling singletons**: `NormaUIManager`, `EventManager`, `PlayerCardInventory`, `ShopManager`, `GameManagerLevel1/2/3`, `CharacterSelectManager`.

3. **Core high-coupling (last)**: `GameEventManager`, `GameTurnManager`, `DeckManager`, `RouteManager`, `DiceRollerFromPNG`, `NormaSystem`, `PlayerStatAggregator`, `SkillManager`, `PassiveSkillManager`, `ScoreManager`.

## Next singleton targets (scene-migration focused)

Based on the progress update (already removed: `DeckData`, `CameraController`, `PlayerInventory`, `PlayerCardInventory`, `ShopManager`, `CharacterSelectManager`), the next best removals to reduce scene-move complexity are:

1. **`NormaUIManager`** (`6` refs, `1` external file)
   - Why next: mostly UI-facing + low fan-out, so converting to scene-local reference (serialized field or context injection) should have low blast radius.
2. **`EventManager`** (`7` refs, `2` external files)
   - Why next: small dependency surface; helps remove hidden global event dependency between scenes.
3. **`GameManagerLevel1/2/3`** (`2` refs each, `1` external file each)
   - Why next: isolated mini-game managers; can be migrated to per-scene ownership with minimal impact.
4. **`PassiveSkillTooltip`** (`4` refs, `1` external file)
   - Why next: lightweight presentation singleton; easy to swap to explicit object references.

### Keep for later (still relatively central)

- `PassiveSkillManager`, `NormaSystem`, `PlayerStatAggregator` (moderate coupling, gameplay flow).
- `GameEventManager`, `GameTurnManager`, `DeckManager`, `RouteManager`, `DiceRollerFromPNG`, `SkillManager`, `ScoreManager` (core orchestration / higher coupling).

### Practical rule for "next to remove"

Prefer singletons with **small external reference count** and **UI or scene-local responsibility** first; postpone singletons that coordinate turn flow, deck lifecycle, or global game state.


## Refactor readiness improvements (high-risk singleton phase)

- Added `TryGet(out manager)` helpers on remaining high-risk singletons (`DeckManager`, `RouteManager`, `GameEventManager`, `NormaSystem`, `GameTurnManager`, `DiceRollerFromPNG`) to standardize call-site migration away from direct static access.
- Added/expanded resolver-based references in core flow scripts (`GameTurnManager`, `GameEventManager`, `NormaSystem`, `BoardGameGroup`, `ChangeSceneButton`) so future singleton removal can be done per-system with lower blast radius.
- Goal of this phase: **prepare dependency seams first**, then remove high-risk singleton declarations in later PRs with less gameplay-flow risk.
