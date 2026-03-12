# Singleton Audit

Scope: `Assets/0StarDice0/Scripts`
## Agreed flow reminder (must keep aligned)

- Current expected play flow: `Menu -> (Start) -> FirstMonsterSelect -> FirstDeck -> TestMain -> Intermission -> TestMain`.
- `Start` from menu must route through `MainMenuController.RequestFlowScene(...)` and ensure `Bootstrap` is loaded additively before requesting gameplay scene via `SceneFlowController`.
- Default target from menu Start is `FirstMonsterSelect` (not `GameScene`).

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

## Architecture recommendation (flow vs additive bootstrap scene)

Given current status (all singleton-style `Instance` declarations removed, and migration now focused on dependency wiring), the most practical direction is a **hybrid**:

1. Keep the current gameplay flow and resolver migration as the default path.
2. Introduce a small **additive bootstrap scene** that stays loaded for true cross-scene services only.

### Why this is safer than a full flow switch now

- The audit already marks gameplay as stable after singleton removal; replacing the whole scene flow at once raises regression risk unnecessarily.
- Existing scripts still contain runtime `TryGet` discovery, so centralizing only long-lived services first gives a cleaner migration seam.
- High-coupling systems (`turn/event/deck` orchestration) are still the caution area; changing loading strategy and service ownership simultaneously would increase blast radius.

### Suggested ownership split

Keep in additive bootstrap scene (persistent):

- `DeckManager` (if deck lifecycle truly spans multiple gameplay scenes)
- save/profile/run-state services
- global audio/settings/event bus (if used globally)

Keep scene-local:

- UI presenters/controllers
- board/route actors
- minigame-local managers

### Guardrails

- Use explicit installer/wiring (serialized refs or context injectors) between bootstrap services and scene-local systems.
- Avoid reintroducing static globals; persistence should come from scene lifetime, not static `Instance` access.
- Add transition checks for load/unload order and stale references (especially around `DeckManager` consumers).

### Practical next step

Pilot with `DeckManager` only: move it to a persistent bootstrap scene, keep current flow intact, and validate turn/event/deck transitions before moving any other service.


## Detailed flow if using scene additive (recommended hybrid rollout)

Below is a concrete execution flow for an additive setup with one persistent bootstrap scene.

### Scene topology

- `Bootstrap` (always loaded): contains long-lived services only (e.g., `DeckManager`, run-state/profile/save services, global event bus).
- `Shell` (optional): loading screen / transition coordinator.
- `Gameplay_*` (loaded/unloaded additively): board, route, combat/minigame, and scene-local UI.

### Runtime lifecycle (step-by-step)

1. **App start**
   - Load `Bootstrap` first.
   - `BootstrapInstaller` builds a `GameContext` (or equivalent service registry).
   - Persistent services initialize in deterministic order (config -> data -> gameplay services).

2. **Enter gameplay**
   - Load target `Gameplay_*` scene additively.
   - `SceneInstaller` in gameplay scene resolves required services from `GameContext` and wires scene-local managers via serialized references.
   - Scene-local systems run `Initialize(context)` instead of global static lookup.

3. **During turn/event loop**
   - Scene-local systems call persistent services through injected references/interfaces.
   - Example chain: `TurnController` -> `DeckFacade` -> `DeckManager` (persistent) -> callback/event -> UI presenter (scene-local).
   - Ownership rule: persistent services store durable state; scene-local objects hold presentation/transient state.

4. **Scene transition (e.g., board -> minigame -> board)**
   - `TransitionCoordinator` freezes inputs.
   - Unload current gameplay scene.
   - Load next gameplay scene additively.
   - New `SceneInstaller` rebinds references to persistent services.
   - Resume loop once all installers report ready.

5. **Failure/edge handling**
   - If a required persistent service is missing, fail fast with explicit error (no silent `Find*` fallback).
   - On unload, scene-local systems unsubscribe from event bus to avoid ghost listeners.
   - Persistent service caches should be invalidated only on full run reset/app quit, not on gameplay scene swap.

### DeckManager-specific flow (pilot)

- Move only `DeckManager` into `Bootstrap` first.
- Expose narrow interface (`IDeckService`) to gameplay scenes.
- In each gameplay scene, install adapters/presenters that consume `IDeckService`.
- Validate these transitions:
  - new run/start game,
  - entering and leaving minigame,
  - returning to board,
  - save/load or continue flow (if present).

### Practical implementation rules

- Prefer **installer + serialized references** over repeated runtime discovery.
- Keep cross-scene contract small (interfaces/facades), avoid leaking scene object references into persistent services.
- Track load order with explicit states: `Bootstrapping`, `LoadingScene`, `Wiring`, `Ready`, `Transitioning`.
- Add regression checklist for every new persistent service before moving the next manager into `Bootstrap`.


## Unity setup guide for current flow (`menu -> firstMonsterSelect -> first deck -> testmain -> intermission -> testmain`)

This section describes **how to configure additive scenes in Unity editor** for your exact current flow, while keeping state machine behavior stable.

### 1) Create scene roles (recommended names)

- `Bootstrap.unity` (persistent, loaded once)
  - `RunSessionStore` (selected monster, selected deck/loadout, run seed, run flags)
  - `DeckService` / `DeckManager` (persistent deck state)
  - `SceneFlowController` (single authority for scene transitions)
- `Menu.unity` (scene-local UI)
- `FirstMonsterSelect.unity` (scene-local selection UI)
- `FirstDeck.unity` (scene-local deck building UI)
- `TestMain.unity` (main gameplay board)
- `Intermission.unity` (reward/shop/upgrade)

### 2) Build Settings order

- Put `Bootstrap` as index 0 in Build Settings.
- Keep gameplay scenes as normal entries after it.
- Start game from `Bootstrap` (not directly from `Menu`) so services always exist.

### 3) Bootstrap scene contents

- Add a root object `__App` with:
  - `BootstrapInstaller`
  - `RunSessionStore` (ScriptableObject runtime copy or MonoBehaviour data holder)
  - `SceneFlowController`
- Optional: add `LoadingCanvas` for transition fade/loading.
- Do **not** put scene-local UI/gameplay objects here.

### 4) Transition pattern (single way only)

All buttons should call `SceneFlowController.Request(state)` instead of `SceneManager.LoadScene(...)` directly.

Recommended state path:

1. `Menu`
2. `FirstMonsterSelect`
3. `FirstDeck`
4. `TestMain`
5. `Intermission`
6. `TestMain` (next loop)

Under the hood each transition uses:

- `LoadSceneAsync(nextScene, LoadSceneMode.Additive)`
- wait scene ready + installer ready
- set active scene to new scene
- `UnloadSceneAsync(previousScene)`

This prevents duplicated managers and race conditions from mixed load styles.

### 5) Where to store selected monster/deck

Store in `RunSessionStore` (persistent in `Bootstrap`), not in scene-local objects.

Suggested runtime fields:

- `SelectedMonsterId`
- `StartingDeckIds` / `DeckSnapshot`
- `CurrentHP`, `Gold`, `RoundIndex` (if run-level data)

Write points:

- `FirstMonsterSelect` confirm button -> save monster into `RunSessionStore`
- `FirstDeck` confirm button -> save deck snapshot into `RunSessionStore`

Read points:

- `TestMain` installer reads `RunSessionStore` and initializes battle/board systems
- `Intermission` reads+writes run data, then returns to `TestMain`

### 6) State machine safety (answer to "will it break?")

State machine is safe **if it controls only flow state**, while persistent services hold data.

Use this split:

- `FlowStateMachine` in `SceneFlowController`: `Menu`, `MonsterSelect`, `DeckSelect`, `Battle`, `Intermission`, `Transitioning`
- `RunSessionStore`: player/monster/deck/run data
- Scene installers: bind scene-local references when a scene becomes active

Key rule: no gameplay state should live only inside a scene that will be unloaded.

### 7) Anti-bug checklist for additive migration

- Only one transition entry point (`SceneFlowController`).
- Block re-entry while transitioning (`isTransitioning` guard).
- Every scene has installer with `Initialize(RunSessionStore, Services...)`.
- Unsubscribe event listeners on `OnDisable/OnDestroy` in scene-local objects.
- Never call `Find*` for core dependencies during gameplay loop.
- Add sanity log on transition: from scene, to scene, load time, ready time.

### 8) Low-risk rollout plan for your project

1. Keep current scene order unchanged.
2. Add `Bootstrap` + `RunSessionStore` + `SceneFlowController` first.
3. Migrate only monster/deck selections to persistent store.
4. Migrate `menu -> firstMonsterSelect -> first deck` path first and test.
5. Then migrate `testmain <-> intermission` loop.
6. After stable, move additional cross-scene systems one by one.


## Estimated implementation timeline (for the proposed additive flow)

Below is a realistic estimate for one developer familiar with the current project.

### Baseline assumptions

- Existing scenes and core gameplay are already working in non-additive flow.
- Work includes setup, migration, integration fixes, and smoke/regression testing.
- Scope focuses on: `menu -> firstMonsterSelect -> first deck -> testmain -> intermission -> testmain`.

### Phase estimate

1. **Bootstrap foundation** (`Bootstrap`, `RunSessionStore`, `SceneFlowController`)
   - 0.5 to 1.0 day

2. **Migrate early flow** (`menu -> firstMonsterSelect -> first deck`)
   - 0.5 to 1.0 day

3. **Migrate gameplay loop** (`testmain <-> intermission` additive transition + rebind)
   - 1.0 to 2.0 days

4. **Stabilization** (edge cases, duplicate listeners, load-order/race condition fixes)
   - 1.0 to 2.0 days

5. **Regression pass + polish** (transition logs, sanity checks, fallback handling)
   - 0.5 to 1.0 day

### Total expected duration

- **Fast path (low friction): ~3 days**
- **Typical path: ~5 working days**
- **If hidden coupling is high: ~7–8 working days**

### What usually extends timeline

- Scene scripts still using direct `LoadScene` in many places.
- Heavy runtime `Find*` dependency discovery in turn/event code.
- Missing unsubscribe cleanup causing duplicated callbacks after scene swaps.
- Data contracts between scene-local UI and persistent run state not yet explicit.

### Recommended planning buffer

If this migration is part-time alongside feature work, plan **1.5 to 2 calendar weeks** to finish safely without rushing QA.


## 2-day MVP plan (avoid singleton, minimize complexity)

If the goal is to ship quickly in **2 days** (not perfect, but stable enough), use this reduced-scope plan.

### Scope cut (important)

Do only these in 2 days:

- Add one persistent `Bootstrap` scene.
- Add one lightweight `RunSessionStore` for cross-scene data.
- Route transitions through one `SceneFlowController`.
- Migrate only essential data persistence:
  - selected monster from `FirstMonsterSelect`
  - starting deck from `FirstDeck`
  - read both in `TestMain`

Do **not** migrate every manager/service yet.

### Minimal architecture

- `Bootstrap` keeps exactly 2 runtime objects:
  - `RunSessionStore`
  - `SceneFlowController`
- Keep `DeckManager` scene-local for now **unless** it already breaks when changing scenes.
- No static `Instance`; pass references via installer or direct serialized link from scene entry scripts.

### Day 1 (implementation)

1. Create `Bootstrap.unity` and set as Build Settings index 0.
2. Implement `RunSessionStore` fields only:
   - `SelectedMonsterId`
   - `SelectedDeckIds` (or compact deck snapshot)
3. Implement `SceneFlowController` with guarded transition API:
   - `Request(SceneState next)`
   - `isTransitioning` lock
4. Convert these transitions to additive pipeline:
   - `Menu -> FirstMonsterSelect -> FirstDeck -> TestMain`
5. Update confirm buttons in select/deck scenes to write into `RunSessionStore`.

### Day 2 (stabilize + loop)

1. Convert `TestMain <-> Intermission` transitions to same pipeline.
2. Ensure each migrated scene has one installer/init point that reads `RunSessionStore`.
3. Add essential guards/logs:
   - reject transition while loading
   - log from/to scene and duration
4. Fix duplicate callbacks by unsubscribing scene-local listeners on unload.
5. Run smoke checklist (start run, choose monster, build deck, enter testmain, go intermission, return testmain).

### What to postpone (after MVP)

- Full DI container or full `GameContext` abstraction.
- Migrating all managers into persistent services.
- Broad refactor of `TryGet` callsites.
- Deep optimization/perf tuning.

### Risk controls for 2-day delivery

- One transition authority only (`SceneFlowController`).
- One persistent data source only (`RunSessionStore`).
- One rollback switch: if additive transition fails, fallback temporarily to current normal load only for that path (keep data in store).
- Freeze feature work during these 2 days to avoid merge conflicts in scene flow scripts.

### Expected outcome in 2 days

- You avoid singleton reintroduction.
- You reduce scene-flow complexity by centralizing transition logic.
- Monster/deck choices survive scene swaps in the main loop.
- System is not final architecture, but becomes a safe base for incremental cleanup later.


## What to start now (no-bias execution plan)

This section gives a neutral recommendation based on effort/risk tradeoff, and what can be done immediately.

### Decision matrix (neutral)

- **Option A: Keep normal scene loading for now**
  - Pros: lowest immediate risk, no migration cost this week.
  - Cons: persistent run-data handling stays fragmented; future refactor cost remains.
  - Best when: deadline is very close and current flow is stable enough.

- **Option B: Full additive architecture now**
  - Pros: cleaner long-term architecture if completed successfully.
  - Cons: highest short-term risk/regression and larger integration surface.
  - Best when: team can allocate a dedicated refactor window with strong QA support.

- **Option C: 2-day additive MVP (recommended practical middle path)**
  - Pros: avoids singleton, reduces flow complexity, keeps scope controllable.
  - Cons: not final architecture; requires a second pass later.
  - Best when: need progress now without destabilizing the game.

### What you should start first (today)

1. Lock migration scope to **monster/deck cross-scene state only**.
2. Create task board with 6 items:
   - Bootstrap scene
   - RunSessionStore
   - SceneFlowController
   - Menu->MonsterSelect->Deck->TestMain migration
   - TestMain<->Intermission migration
   - smoke/regression checklist
3. Freeze unrelated gameplay changes during migration branch.
4. Add transition debug log format before refactor (so baseline and after-migration can be compared).

### Concrete deliverables by day

- **End of Day 1**
  - `Bootstrap` exists and starts first.
  - `RunSessionStore` stores selected monster + selected deck.
  - First path (`Menu -> FirstMonsterSelect -> FirstDeck -> TestMain`) runs via one transition controller.

- **End of Day 2**
  - `TestMain <-> Intermission` uses same transition flow.
  - No duplicate manager/listener behavior in smoke run.
  - Known issues list is written (if any) with reproducible steps.

### What I can help you fix right now

If you want, I can immediately prepare these assets in the next pass:

1. **SceneFlowController skeleton**
   - additive load/unload coroutine
   - `isTransitioning` guard
   - unified `Request(nextState)` entry point

2. **RunSessionStore data contract**
   - minimal runtime fields (`SelectedMonsterId`, `SelectedDeckIds`, run meta)
   - clear read/write API from scene scripts

3. **Scene installer pattern**
   - per-scene `Initialize(store, services)` skeleton
   - lifecycle-safe event subscribe/unsubscribe template

4. **Migration patch list by file**
   - exact file targets for button handlers and transition calls
   - before/after call pattern (`LoadScene` -> controller request)

5. **Smoke test checklist**
   - deterministic test steps for your exact flow
   - expected results and failure signatures

### Anti-bias note

Recommendation here is based on delivery risk, change surface, and reversibility (not preference for a specific architecture style). If your priority changes (e.g., long-term cleanliness over short-term safety), the preferred option can change accordingly.


## Scripts I can patch for you immediately (high-impact first)

Based on current scene-flow risk and direct scene-loading usage, these are the best immediate targets.

### Priority 1 — transition entry points (migrate first)

1. `Assets/0StarDice0/Scripts/Code/MainMenu/MainMenuController.cs`
   - Replace direct `SceneManager.LoadScene(...)` start path with `SceneFlowController.Request(...)`.
   - Make this the first handoff into your unified flow.

2. `Assets/0StarDice0/Scripts/Code/CodeInterMission/ShopInterMission/ChangeSceneButton.cs`
   - Replace button-based direct load with flow-controller request.
   - Prevent users from triggering multiple transitions quickly.

3. `Assets/0StarDice0/Scripts/Code/CodeInterMission/InterMission/sceneloader.cs`
   - Convert generic `LoadSceneByName/Index` to controller-based request wrapper.
   - Keep the same public API to avoid breaking existing button bindings.

4. `Assets/0StarDice0/Scripts/Code/CodeInterMission/InterMission/LevelSelector.cs`
   - Route level selection through flow controller instead of direct scene load.

### Priority 2 — loop-critical scripts (`testmain <-> intermission`)

5. `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/PlayerState.cs`
   - Replace intermission scene jump with flow-controller transition.
   - Ensure run-state writeback happens before leaving scene.

6. `Assets/0StarDice0/Scripts/Code/MainGame/_Events/GameEventManager.cs`
   - Wrap scene changes behind one transition service (do not call SceneManager directly in event branches).
   - Add guard against transition re-entry in chained events.

7. `Assets/0StarDice0/Scripts/Code/MainGame/_BossController/BossSceneController.cs`
8. `Assets/0StarDice0/Scripts/Code/MainGame/BattleRewardButton.cs`
9. `Assets/0StarDice0/Scripts/Code/MiniGame/MiniGameRewardService.cs`
   - Unify return-to-board logic via flow controller (currently fragmented direct loads).

### Priority 3 — data persistence handoff

10. `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckManager.cs`
    - Add explicit read/write boundary with `RunSessionStore` (deck snapshot in/out).
    - Keep deck logic local first; do not force full persistent manager migration in MVP.

11. `Assets/0StarDice0/Scripts/Code/CodeMenu/CharacterSelectMenu.cs`
    - Persist selected monster into `RunSessionStore` at confirm time.

### Priority 4 — cleanup after flow is stable

12. `Assets/0StarDice0/Scripts/Code/MainGame/SceneController.cs`
13. `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/GameManagerLevel1.cs`
14. `Assets/0StarDice0/Scripts/Code/MiniGame/CodeCard/GameManagerLevel2.cs`
15. `Assets/0StarDice0/Scripts/Code/MiniGame/CodeFappyBird/SceneChanger.cs`
    - Remove remaining direct loads or route them via compatibility wrapper.

### What I can implement in the very next pass (actual code changes)

- Add `SceneFlowController` script skeleton (state enum + guarded additive transition coroutine).
- Add `RunSessionStore` script for monster/deck snapshot.
- Patch Priority 1 scripts to use controller while preserving existing UI button bindings.
- Add temporary compatibility wrapper so old calls still work during migration.


## Bootstrap start issue fix (latest)

- Added auto-start handoff in `SceneFlowController.Start()` for bootstrap runs.
- New serialized controls:
  - `autoLoadFirstGameplayScene` (default: true)
  - `firstGameplaySceneName` (default: `Menu`)
- Behavior: when active scene is `Bootstrap`, controller automatically requests first gameplay scene additively.
- Safety checks added:
  - skip if first scene name is empty,
  - skip + warning if scene is not in Build Settings (`CanStreamedLevelBeLoaded == false`).


## Current implemented flow (post-migration snapshot)

This section reflects what is already implemented in code right now.

### Runtime flow (menu-first)

1. Game starts at `Menu`.
2. Player presses **Start** (`MainMenuController.OnNewGameClicked`).
3. Controller tries `SceneFlowController.TryRequestScene(target)` first.
4. If flow controller is not available yet, menu loads `Bootstrap` additively and waits briefly for `SceneFlowController`.
5. Once available, scene transition is requested through flow controller; otherwise fallback to direct `SceneManager.LoadScene`.

### Selected monster state update (single point)

When player confirms monster selection (`CharacterSelectMenu.SelectCharacter`), one method now updates all required channels:

- `PlayerPrefs[SelectedMonster]` (restore/continue)
- `RunSessionStore.SelectedMonsterId` (runtime additive session)
- `GameData.Instance.selectedPlayer` (primary selected player pointer for existing gameplay systems)

### Source-of-truth model (short term)

- `GameData.selectedPlayer`: primary selected player reference used by core gameplay scripts.
- `PlayerState`: board-runtime mutable state (HP, buffs/debuffs, runtime progression).
- `RunSessionStore`: additive-flow session helper for cross-scene runtime handoff.
- `PlayerPrefs`: persistence/restore channel, not sole runtime source.

## Recommended next steps (from current baseline)

### 1) Remove high-risk fallback paths gradually

- Reduce scattered `PlayerPrefs` fallback reads that infer selected player at runtime.
- Prefer reading from `GameData.selectedPlayer` and use `RunSessionStore` only as helper handoff.

### 2) Migrate remaining direct scene loads in orchestration scripts

- Prioritize `GameEventManager` and other event/boss/minigame transitions still using direct `SceneManager.LoadScene`.
- Route through flow controller compat path to unify transition behavior.

### 3) Deck handoff parity with monster handoff

- Apply the same single-point pattern for deck confirm action:
  - persist for restore,
  - write runtime session store,
  - write primary runtime pointer used by gameplay systems.

### 4) Add lightweight transition telemetry and error guardrails

- Keep transition logs (`from -> to -> duration`).
- Warn when expected bootstrap/session services are missing.
- Keep fallback for safety during migration, then tighten once stable.

### 5) Lock one clear ownership rule before deeper refactor

- Data that must survive app restart -> persistence layer.
- Data that must survive scene swap only -> session store / game data pointer.
- Data used only inside one board run -> player/runtime state.

## Immediate next implementation checklist

1. Keep menu Start target fixed to `FirstMonsterSelect` and verify existing scene instances are not serialized with stale `GameScene`.
2. Keep `RunSessionStore` monster sync pref-first (never fallback to serialized inspector value) to avoid stale element carry-over.
3. Pilot `DeckManager` bootstrap ownership only, then validate transitions (`FirstDeck -> TestMain -> Intermission -> TestMain`) before moving other managers.
4. Add per-scene smoke script/checklist in Unity for: selected monster, selected deck ids, and opening hand source.


## RunSessionStore responsibility (clarified)

- `RunSessionStore` is for **run-level transient state across scene swaps** (e.g., selected monster id, selected deck ids, round index).
- It should mirror persisted selection (`PlayerPrefs`) into runtime memory and expose values for systems that should not parse prefs repeatedly.
- It is **not** the long-term save system itself; persistence remains in save/profile keys.

