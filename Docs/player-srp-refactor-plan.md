# Player SRP Refactor Plan

## Goal
Separate responsibilities clearly across the player-related runtime flow.

- **Static data** -> `PlayerData`
- **Session selection/context** -> `GameData`
- **Runtime state** -> `PlayerState`
- **Persistent progress** -> `PlayerProgress`

## Current status
The codebase now has the core ownership split in place:
- `PlayerProgress` owns persistent credit and level/EXP values.
- `GameData` tracks the selected player and routes selected-player progress calls through `PlayerProgressService`.
- `PlayerState` reads selected-player persistent progress through `PlayerStateProgressCoordinator` and keeps battle/runtime values local.

### Verified phase progress (code review)
- **Phase 1 - Complete for the progress refactor target.** Selected-player credit reads/writes now primarily go through `GameData` APIs, while `PlayerData` wrappers remain only as compatibility shims for legacy callers and bridge code.
- **Phase 2 - Complete.** `GameData` is now a session container/facade and delegates progress loading, saving, resetting, and selected-player credit/level orchestration to `PlayerProgressService`.
- **Phase 3 - Complete.** `PlayerState` no longer owns persistence orchestration directly; snapshot/restore and selected-progress resolution now go through `PlayerStateProgressCoordinator`.
- **Phase 4 - Remaining.** The main remaining work is thinning manager/presenter classes and reducing legacy fallback chains in scene/UI code.

### Evidence used for the review
- `GameData` exposes service-backed selected-player APIs (`GetSelectedPlayerCredit`, `SetSelectedPlayerCredit`, `AddSelectedPlayerCredit`, `TrySpendSelectedPlayerCredit`, `SetSelectedPlayerLevelProgress`) and no longer contains direct persistence logic.
- `PlayerProgressService` now owns selected-player progress loading, writes, level sync, and reset-to-default behavior.
- `PlayerState` resolves persistent progress via `PlayerStateProgressCoordinator`, captures snapshots through the coordinator, and restores the snapshot on board reset/defeat flow.
- `PlayerData` still exposes compatibility wrappers (`Credit`, `SetCredit`, `AddCredit`, `TrySpendCredit`) but these delegate into `PlayerProgress` instead of owning progress state directly.

## Phase 1 - Remove direct progress usage from call sites
**Status:** Complete for the current migration scope.

**Target:** stop using `GameData.selectedPlayer.Credit`, `SetCredit`, `AddCredit`, `TrySpendCredit` in gameplay/UI code where a selected-player context already exists.

### Work items
1. Replace selected-player credit reads with `GameData.GetSelectedPlayerCredit()`.
2. Replace selected-player credit writes with `GameData.SetSelectedPlayerCredit()` / `AddSelectedPlayerCredit()` / `TrySpendSelectedPlayerCredit()`.
3. Keep `PlayerData` compatibility wrappers temporarily so unchanged legacy code still works.

### Exit criteria
- Most session-scoped code no longer treats `PlayerData` as the progress owner.
- `PlayerData` wrappers remain only for backward compatibility.

### Notes
- Remaining `PlayerData` progress access is concentrated in compatibility-oriented code paths such as legacy battle bridging and a few presenters/managers that still need Phase 4 cleanup.
- Those remaining accesses no longer make `PlayerData` the source of truth because the wrappers forward into `PlayerProgress`.

## Phase 2 - Extract progress coordination from `GameData`
**Status:** Complete.

**Target:** make `GameData` a session container only.

### Work items
1. Introduce a dedicated `PlayerProgressService` or `PlayerProgressRepository`.
2. Move load/save/reset logic from `GameData` into that service.
3. Let `GameData` hold only selected references and run/session selections.

### Exit criteria
- `GameData` no longer owns persistence orchestration.
- Progress load/save behavior is testable outside scene objects.

### Notes
- `GameData` still exposes convenience methods, but they are now facade methods that delegate to `PlayerProgressService`.
- Persistent progress reset is centralized through `PlayerProgressService.ResetProgressToDefaults()`.

## Phase 3 - Extract persistence lifecycle from `PlayerState`
**Status:** Complete.

**Target:** keep `PlayerState` focused on runtime state only.

### Work items
1. Move persistence snapshot/restore logic out of `PlayerState`.
2. Create a coordinator for board start, board reset, defeat, and intermission transitions.
3. Keep `PlayerState` limited to HP, attack/defense/speed, stars, debuffs, and runtime unlock state.

### Exit criteria
- `PlayerState` does not talk directly to persistence APIs.
- Scene transition sync is coordinated elsewhere.

### Notes
- `PlayerState` still stores cached runtime copies of credit/level fields for gameplay/UI usage, but selected-player persistence resolution and snapshot lifecycle are coordinated externally.
- Legacy battle sync is still handled by `BattleHealthSyncBridge`, which is a Phase 4 concern.

## Phase 4 - Thin managers/presenters
**Status:** Remaining.

**Target:** reduce mixed UI + domain logic in manager classes.

### Candidates
- `ShopUIManager`
- `GameSetupManager`
- `BattleHealthSyncBridge`
- `CharacterSelectManager`

### Work items
1. Separate UI display from data resolution.
2. Move orchestration into services/facades.
3. Remove fallback chains where a clear owner can be injected.

## Reference checklist by phase (Phase 1 -> Latest)

Use this section as a copy/paste reference when tracking rollout status in notes, tickets, or QA sheets.

### Phase 1 checklist - Replace direct progress usage in call sites
**Code checklist**
- [ ] Replace selected-player credit reads with `GameData.GetSelectedPlayerCredit()`.
- [ ] Replace selected-player credit writes with `GameData.SetSelectedPlayerCredit()`, `AddSelectedPlayerCredit()`, or `TrySpendSelectedPlayerCredit()`.
- [ ] Remove new direct usages of `GameData.selectedPlayer.Credit`, `SetCredit`, `AddCredit`, `TrySpendCredit` from gameplay/UI code.
- [ ] Keep `PlayerData` compatibility wrappers only for unchanged legacy code paths.

**Files/features to re-check**
- [ ] shop and intermission credit display
- [ ] skill purchase / passive skill spending
- [ ] random unlock / shop pack spending
- [ ] HUD credit display
- [ ] mini-game reward credit grant

**QA checklist**
- [ ] Spend credit in shop and verify persistent credit decreases once.
- [ ] Earn credit from mini-game and verify board/intermission show the same value.
- [ ] Re-enter scene and verify selected-player credit stays consistent.

### Phase 2 checklist - Extract progress coordination from `GameData`
**Code checklist**
- [ ] `GameData` exposes facade methods only; it should not own raw `PlayerPrefs` logic.
- [ ] `PlayerProgressService` owns selected-player load/save/reset behavior.
- [ ] Reset/default-progress flow goes through the service, not ad-hoc scene code.
- [ ] Selected-player progress can be reloaded by calling `EnsureSelectedPlayerProgressLoaded()`.

**Files/features to re-check**
- [ ] `GameData` lifecycle (`Awake`, selected-player assignment, scene persistence)
- [ ] `PlayerProgressService` load/reset paths
- [ ] main menu reset/new-run flow

**QA checklist**
- [ ] Start game from normal entry scene and verify `GameData` survives scene changes.
- [ ] Reset progress and verify selected player returns to starting credit/level/EXP.
- [ ] Swap selected player and verify the newly selected player's progress loads correctly.

### Phase 3 checklist - Extract persistence lifecycle from `PlayerState`
**Code checklist**
- [ ] `PlayerState` resolves persistent progress via `PlayerStateProgressCoordinator`.
- [ ] Snapshot capture/restore uses `PlayerProgressSnapshot` instead of mutating `PlayerData` directly.
- [ ] Runtime HP/stats stay local to `PlayerState`.
- [ ] Defeat / board reset / scene return flow restores level/EXP through the coordinator path.

**Files/features to re-check**
- [ ] `PlayerState` load and reset flow
- [ ] `PlayerStateProgressCoordinator` resolve / capture / restore flow
- [ ] battle return and defeat handling

**QA checklist**
- [ ] Enter board scene and verify level/EXP load from persistent progress.
- [ ] Lose/exit/reset and verify snapshot restore behaves as expected.
- [ ] Return from battle and confirm runtime HP sync does not corrupt persistent level/EXP.

### Phase 4 checklist - Thin managers/presenters
**Code checklist**
- [ ] `ShopUIManager` becomes a thin view/presenter host only.
- [ ] `GameSetupManager` becomes a scene adapter that calls one setup/sync service.
- [ ] `CharacterSelectManager` / `CharacterSelectUI` use a dedicated selection service.
- [ ] `BattleHealthSyncBridge` is split into smaller adapters/services and reduced reflection-heavy paths.
- [ ] `PlayerGlobalHudPresenter` / `PlayerController` stay as thin runtime adapters only.

**Files/features to re-check**
- [ ] shop HUD binding
- [ ] board setup flow
- [ ] character selection flow
- [ ] battle result panel binding
- [ ] legacy battle scene integration

**QA checklist**
- [ ] All scenes still work without relying on hidden fallback chains.
- [ ] UI refresh is event-driven where practical; no unnecessary polling remains.
- [ ] New scene setup is understandable from Inspector wiring alone.

### Latest manual regression checklist (use before sign-off)
- [ ] Select a player from the main character select flow.
- [ ] Enter a board scene and verify HUD HP / credit / level.
- [ ] Spend credit in intermission/shop and confirm value persists.
- [ ] Earn credit from at least one mini-game and verify the same selected player receives it.
- [ ] Enter a battle and return to the board.
- [ ] Verify battle HP sync works and reward credit/EXP/level sync back correctly.
- [ ] Restart the game / re-enter Play Mode and verify the same selected-player progress reloads.
- [ ] Run isolated-scene fallback tests only where explicitly supported (shop, character select, board setup).

## Concrete refactor backlog by script

### Priority 1 - `ShopUIManager`
**Current issue**
- Mixes UI rendering, selected-player resolution, fallback resolution, and progress writes in one MonoBehaviour.

**Refactor target**
- Keep `ShopUIManager` as a thin Unity view/component holder only.
- Extract a presenter/service pair, for example `ShopCreditPresenter` + `ISelectedPlayerCreditService`.

**Concrete steps**
1. Move credit read/write logic out of `ShopUIManager.PlayerCredit`.
2. Keep only serialized UI refs (`TMP_Text`, optional fallback test refs) on the MonoBehaviour.
3. Let a presenter subscribe/unsubscribe to credit changes and push formatted text into the view.
4. Remove polling in `Update()` and replace it with explicit bind/rebind hooks.

**Done when**
- `ShopUIManager` no longer calls `GameData.Instance` or `PlayerData.SetCredit()` directly.
- UI refresh happens through a single presenter/service path.

### Priority 2 - `GameSetupManager`
**Current issue**
- Resolves selected player, fallback player, and runtime-to-persistent sync in one class.

**Refactor target**
- Turn it into a scene entry adapter that calls one application service, for example `BoardPlayerSetupService`.

**Concrete steps**
1. Extract player selection resolution into `SelectedPlayerContextResolver`.
2. Extract runtime progress save-back into `BoardProgressSyncService`.
3. Keep `GameSetupManager` responsible only for Unity lifecycle (`OnEnable`) and logging.
4. Remove direct knowledge of credit/level/exp fields from the MonoBehaviour.

**Done when**
- `GameSetupManager` does not directly read `GameTurnManager.CurrentPlayer.PlayerCredit` / level / EXP.
- Setup and sync behavior can be called from tests without a scene object.

### Priority 3 - `CharacterSelectManager` + `CharacterSelectUI`
**Current issue**
- The selection flow is still tightly scene-coupled and uses `GameData` directly.

**Refactor target**
- Split selection orchestration into a `SelectCharacterService` and keep UI classes focused on button/display behavior.

**Concrete steps**
1. Move `GameData.Instance.SetSelectedPlayer(...)` orchestration into a dedicated service.
2. Keep `CharacterSelectManager` as a bridge from button click -> service call.
3. Keep `CharacterSelectUI` focused on visuals/highlight state only.
4. Add a small fallback/test bootstrap object for isolated scene play instead of repeating fallback logic in multiple components.

**Done when**
- Character-select UI no longer needs to know how selected-player persistence is stored.
- Scene play and isolated test play use the same selection service.

### Priority 4 - `BattleHealthSyncBridge`
**Current issue**
- Handles scene events, reflection-based field injection, runtime `PlayerData` cloning, selected-player override, HP sync, and result button wiring all in one static class.

**Refactor target**
- Split battle bridging into smaller adapters/services and reduce reflection-heavy behavior.

**Concrete steps**
1. Extract runtime `PlayerData` clone creation into `BattleRuntimePlayerFactory`.
2. Extract reward/level/EXP sync-back into `BattleProgressSyncService`.
3. Extract button wiring into a dedicated `BattleResultPanelBinder` or explicit component on result panels.
4. Replace field-name reflection (`playerHP`, `selectedPlayer`, `winPanel`, `losePanel`) with explicit interfaces/components where possible.

**Done when**
- The bridge no longer owns reward sync + panel wiring + runtime clone creation in one file.
- New battle scenes can opt into explicit components instead of naming conventions.

### Priority 5 - `PlayerGlobalHudPresenter` + `PlayerController`
**Current issue**
- These are thinner now, but still act as compatibility glue around runtime state.

**Refactor target**
- Keep them as small adapters only and avoid adding more domain logic to them.

**Concrete steps**
1. Keep HUD formatting in presenter code only.
2. Avoid reading persistence fallback chains from the controller/presenter when a single owner can be injected.
3. Keep `PlayerController` bound to runtime `PlayerState` events only.

**Done when**
- Both scripts remain small, scene-facing adapters with no persistence orchestration.

## Scripts touched by the player progress refactor

### Core persistence and player domain
- `Assets/0StarDice0/Player/GameData.cs`
- `Assets/0StarDice0/Player/PlayerData.cs`
- `Assets/0StarDice0/Player/PlayerProgress.cs`
- `Assets/0StarDice0/Player/PlayerProgressService.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/PlayerProgressSnapshot.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/PlayerState.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/PlayerStateProgressCoordinator.cs`

### Board / battle / runtime systems
- `Assets/0StarDice0/Item/EquipmentManager.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/BattleHealthSyncBridge.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/BattleResultFlowService.cs`
- `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameSetupManager.cs`
- `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerController.cs`
- `Assets/0StarDice0/Scripts/MainGame/_UIManager/PlayerGlobalHudPresenter.cs`

### Shop / intermission / menu flow
- `Assets/0StarDice0/Scripts/CodeInterMission/InterMission/IntermissionCreditUI.cs`
- `Assets/0StarDice0/Scripts/CodeInterMission/ShopInterMission/ChangeSceneButton.cs`
- `Assets/0StarDice0/Scripts/CodeInterMission/ShopInterMission/RandomUnlock.cs`
- `Assets/0StarDice0/Scripts/CodeInterMission/ShopInterMission/ShopPackManager.cs`
- `Assets/0StarDice0/Scripts/MainGame/CardMain/ShopManager.cs`
- `Assets/0StarDice0/Scripts/ShopUIManager.cs`
- `Assets/0StarDice0/Scripts/MainMenu/MainMenuController.cs`

### Mini-games and rewards
- `Assets/0StarDice0/Scripts/MiniGame/CodeCard/GameManagerLevel3.cs`
- `Assets/0StarDice0/Scripts/MiniGame/CodeFappyBird/GameManager.cs`
- `Assets/0StarDice0/Scripts/MiniGame/CodeMath/QuickMathManager.cs`
- `Assets/0StarDice0/Scripts/MiniGame/CodeSpot/MemoryGameManager.cs`
- `Assets/0StarDice0/Scripts/MiniGame/MiniGameRewardService.cs`

### Passive / skill / test helpers
- `Assets/0StarDice0/PassiveSkill/PassiveSkillManager.cs`
- `Assets/0StarDice0/PassiveSkill/SkillManager.cs`
- `Assets/0StarDice0/PassiveSkill/SkillTreeUI.cs`
- `Assets/0StarDice0/Scripts/Test/BoardgameTestAddCreditButton.cs`
- `Assets/0StarDice0/Scripts/Test/TestFight/CharacterSelectManager.cs`
- `Assets/0StarDice0/Scripts/Test/TestFight/CharacterSelectUI.cs`

### Documentation
- `Docs/player-srp-refactor-plan.md`

## Unity checklist for remaining work and scene wiring
Use this checklist when wiring scenes/prefabs in Unity after the refactor. The goal is to keep Phases 1-3 stable while finishing Phase 4 cleanup.

### A. Global progress/runtime setup
1. **Locate the persistent `GameData` object** in the bootstrap scene or any scene that is marked `DontDestroyOnLoad`.
2. **Confirm `GameData` has the `GameData` component only once** in the runtime flow.
3. **Do not create a separate scene object for `PlayerProgressService` or `PlayerStateProgressCoordinator`.** Both are static/service-style code and require no Unity component.
4. **Do not create a separate scene object for `PlayerProgressSnapshot`.** It is a serializable field owned by `PlayerState`.
5. Enter Play Mode from the normal game entry scene and verify `GameData.Instance` survives scene loads.

### B. Character select scene checklist
1. Open the character select scene/prefab.
2. Select the object that owns `CharacterSelectManager`.
3. In the Inspector, assign `defaultSelectedPlayer` to the fallback `PlayerData` asset that should be used when entering the scene directly in isolation.
4. Open each UI button or card used for character selection.
5. Verify the button click/event calls the character selection flow that eventually invokes `CharacterSelectManager.SelectCharacter(PlayerData)`.
6. Test both flows:
   - normal flow from the main menu with `GameData` already alive,
   - isolated scene Play Mode using only `defaultSelectedPlayer`.

### C. Board scene checklist
1. Open each board scene (`TestMain`, element boards, etc.).
2. Select the runtime player object.
3. Verify it has a `PlayerState` component.
4. Verify the same object has a `PlayerStatAggregator` reference assigned, or assign the `PlayerStatAggregator` component if the scene expects stat refresh support.
5. Check that `GameTurnManager.CurrentPlayer` resolves to this runtime `PlayerState` during gameplay.
6. Select the board HUD object.
7. Verify it has `PlayerGlobalHudRefs` and that these references are assigned:
   - `currentHpText`
   - `creditText`
   - `levelText`
   - debuff UI refs if the scene uses debuff display.
8. Play the scene and confirm HP / Credit / Level update correctly after board events and when returning from mini-games or battles.

### D. Intermission / shop scene checklist
1. Open the intermission/shop scene.
2. Check every object using credit display or credit spending logic:
   - `IntermissionCreditUI`
   - `ShopUIManager`
   - `ShopPackManager`
   - `ChangeSceneButton`
   - `RandomUnlock`
3. For each of those components, verify serialized UI references are still assigned after the refactor.
4. If a component exposes a fallback `PlayerData`, assign it only for direct scene testing. In the normal game flow, prefer the selected player from `GameData`.
5. Click through purchase/unlock actions in Play Mode and confirm credit updates once per action and persists when changing scenes.

### E. Mini-game checklist
1. Open each mini-game scene (`CodeCard`, `FappyBird`, `Math`, `Spot`, etc.).
2. No new reward manager object is required for `MiniGameRewardService`; it is static.
3. Verify the end-of-game script still calls the reward flow (`TryGrantCreditReward` or `TryGrantFixedCreditReward`) and then returns to the board scene.
4. Start a mini-game from the board, finish it, and confirm:
   - reward credit is granted,
   - the board scene reloads correctly,
   - the same selected player's persistent progress is updated.

### F. Battle scene checklist
1. Open each legacy battle scene.
2. Identify the battle controller object(s) that still rely on injected `selectedPlayer` or `playerHP` fields.
3. Verify those fields still exist and are serialized/accessible as expected by `BattleHealthSyncBridge`.
4. Inspect win/lose result panels and their child buttons.
5. Make sure reward/continue/restart buttons are present and active in the prefab hierarchy the bridge scans.
6. Run a battle from the board and verify:
   - HP syncs from the board into the battle scene,
   - battle reward credit/EXP/level changes sync back to the selected player on return,
   - restart/close/reward buttons behave correctly.
7. If any scene requires special-case manual binding, note it in scene-specific documentation before further refactoring `BattleHealthSyncBridge`.

### G. Manual regression checklist before closing Phase 4
1. Select a player from the character select scene.
2. Enter a board scene and confirm HUD values match persistent progress.
3. Spend credit in intermission/shop.
4. Earn credit from a mini-game.
5. Enter and leave a battle.
6. Return to intermission and confirm credit/level/EXP still match the same selected player.
7. Restart the app / re-enter Play Mode and confirm the same selected player's progress reloads from persistence.

## Non-goals for now
- Full rewrite of battle legacy controllers.
- Removing every compatibility API in one step.
- Splitting into many micro-classes without reducing coupling.

## Principles
- Prefer **one clear owner** per data category.
- Keep transitional compatibility only where needed to reduce breakage.
- Refactor incrementally with behavior-preserving changes first.
