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
  - `NormaSystem`
  - `DiceRollerFromPNG`
  - `RouteManager`
  - `GameTurnManager`
  - `GameEventManager`
  - `DeckManager`
- Remaining singleton-style `Instance` declarations: **0** (excluding commented code).
## Singleton list (initial snapshot)

| Class | File | `Class.Instance` refs in `.cs` | Referenced from other files | Notes |
|---|---|---:|---:|---|
| `DeckManager` | `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckManager.cs` | 5243 | 57 | Core flow / high coupling |
| `DeckData` | `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckData.cs` | 0 | 0 | Candidate for removal/refactor first (no direct code usage) |
| `CameraController` | `Assets/0StarDice0/Scripts/Code/MainGame/_CameraManager/CameraController.cs` | 0 | 0 | Candidate for removal/refactor first (no direct code usage) |
| `PassiveSkillTooltip` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/PassiveSkillTooltip.cs` | 4 | 1 | Low coupling, good early refactor target |
| `SkillManager` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/SkillManager.cs` | 14 | 3 | Core flow / high coupling |
| `PassiveSkillManager` | `Assets/0StarDice0/Scripts/Code/MainGame/Temp/PassiveSkillManager.cs` | 14 | 2 | Low coupling, good early refactor target |
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

3. **Core high-coupling (last)**: `PlayerStatAggregator`, `SkillManager`, `PassiveSkillManager`, `ScoreManager`.

## Next singleton targets (scene-migration focused)

- Singleton-style static `Instance` declarations in scope are now fully removed.
- Next improvement focus should move from singleton removal to:
  1. replacing high-frequency `TryGet` call sites with serialized references/context injection where practical,
  2. reducing hidden global coupling in core gameplay services,
  3. adding targeted regression tests around scene transitions and turn/event flow.

### Keep for later (still relatively central)

- `PassiveSkillManager`, `PlayerStatAggregator` (moderate coupling, gameplay flow).
- `SkillManager`, `ScoreManager` (core orchestration / higher coupling).

### Practical rule for "next to remove"

Prefer singletons with **small external reference count** and **UI or scene-local responsibility** first; postpone singletons that coordinate turn flow, deck lifecycle, or global game state.


## Refactor readiness improvements (high-risk singleton phase)

- Added `TryGet(out manager)` helpers and safe wrappers for high-risk managers to standardize call-site migration away from direct static access.
- Added/expanded resolver-based references in core flow scripts (`GameTurnManager`, `GameEventManager`, `NormaSystem`, `BoardGameGroup`, `ChangeSceneButton`) so future singleton removal can be done per-system with lower blast radius.
- Goal of this phase: **prepare dependency seams first**, then remove high-risk singleton declarations in later PRs with less gameplay-flow risk.


## Structural health check (removed singletons)

- Checked all classes already listed under 'Removed singleton `Instance` from' and confirmed there are no remaining `Class.Instance` call sites for them in `Assets/0StarDice0/Scripts`.
- Quality status: **stable / not messy** for the removed set from a coupling perspective (no backsliding to static access found).
- Current caution area: several scripts still rely on runtime `TryGet` discovery. Prefer serialized references (where possible) for high-frequency paths in future refactors to reduce lookup cost and hidden wiring.

### Suggested next removal order (updated)

1. No remaining singleton-style `Instance` declarations in scope (`Assets/0StarDice0/Scripts`).


## Performance improvements in this phase

- `TryGet` for removed-singleton managers now uses static cache + lazy resolve to avoid repeated `FindFirstObjectByType` scans (including `GameTurnManager`, `GameEventManager`, and `DeckManager` in this phase).
- Cache is reset safely in `OnDestroy` to avoid stale references across scene/object lifecycle.
- Practical impact: lower lookup overhead in frequently triggered call-sites (`UI`, `AI turn`, `card use`) while keeping non-singleton API surface.
