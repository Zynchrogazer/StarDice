# Player UI Refactor Plan

## Goals

- Make element-to-panel binding explicit instead of relying on scene-wide discovery.
- Keep `PlayerUIController` focused on orchestration only.
- Separate panel references, stat presentation, and debuff presentation by responsibility.
- Support split ownership cleanly:
  - element status panel
  - shared HUD
  - existing owners for shop / quest

## Current Runtime Data Flow

1. `GameData.selectedPlayer` stores the selected `PlayerData`.
2. `PlayerState` loads runtime stats from that `PlayerData`.
3. `PlayerUIController` resolves the selected element and updates the player's UI.

## Target Architecture

### 1. `ElementStatusPanelRegistry`

Responsibility:

- Map `ElementType` -> `PlayerStatusPanelRefs`.
- Provide the explicit UI entry point for each element panel.

Why:

- Removes hidden binding rules from `PlayerUIController`.
- Makes the Unity Inspector setup understandable for designers.

### 2. `PlayerStatusPanelRefs`

Responsibility:

- Hold references for one element's UI sections:
  - status button
- Optionally auto-fill missing references from the configured search roots.

Why:

- One component answers the question: “Which text belongs to this element's UI?”

### 3. `PlayerGlobalHudRefs` + `PlayerGlobalHudPresenter`

Responsibility:

- Hold and render shared HUD values that are not per-element:
  - current HP
  - credit
  - level
  - debuff display / tooltip

Why:

- Shared HUD should not be duplicated across every element panel refs component.

### 4. `PlayerStatsPanelPresenter`

Responsibility:

- Render player stats into a `PlayerStatusPanelRefs`.

Why:

- Keeps element status formatting and display rules out of `PlayerUIController`.

### 5. `PlayerDebuffPresenter`

Responsibility:

- Build and render debuff state.
- Manage sprite icons / fallback rich text / tooltips on the shared HUD.

Why:

- Debuff UI changes should not force unrelated changes in the general player UI controller.

### 6. `PlayerUIController`

Responsibility:

- Resolve the player.
- Resolve the selected element.
- Ask the registry for the matching panel refs.
- Call the element presenter, shared HUD presenter, and debuff presenter.

Why:

- This keeps the controller aligned with SRP and KISS.

## Migration Plan

### Phase A1 - Documentation and naming cleanup

- Rename panel reference fields so Inspector names match actual UI meaning.
- Document the five UI sections clearly.

### Phase B1 - Explicit registry support

- Add `ElementStatusPanelRegistry`.
- Make `PlayerUIController` prefer registry-based lookup.
- Keep current auto-bind fallback for scenes that have not been migrated yet.

### Phase B2 - Scene migration

- Add one `PlayerStatusPanelRefs` per element root in Unity.
- Register each one into `ElementStatusPanelRegistry`.
- Rebind only the element-owned UI explicitly in the Inspector.
- Bind shared HUD and shared debuff once through `PlayerGlobalHudRefs`.
- Leave shop credit with `ShopManager` and quest progress with `NormaUIManager`.

### Phase B3 - Reduce fallback complexity

- Once all scenes use the registry, remove or minimize auto-bind fallback.
- Keep `additionalSearchRoots` only if truly needed.

### Phase B4 - Event-driven refresh

- Replace `Update()`-driven polling with `PlayerState.OnStatsUpdated` where practical.

## Unity Setup Checklist

For each element panel:

1. Add `PlayerStatusPanelRefs`.
2. Bind the element-owned UI sections only: status button.
3. Add the refs component to `ElementStatusPanelRegistry`.

For the main UI controller object:

1. Assign `ElementStatusPanelRegistry`.
2. Assign `PlayerGlobalHudRefs` (HP / Credit / Level / Debuff).
3. Assign debuff sprites / optional prefab.
4. Keep fallback refs only for legacy scenes.
