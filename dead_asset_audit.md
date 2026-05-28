# Dead Scene / Dead Script Audit (Phase 2 - higher accuracy)

Date: 2026-05-28 (UTC)

## What changed from Phase 1
Phase 1 used only serialized GUID references. In Phase 2, I added an extra runtime-signal pass to reduce false positives:

1) **Serialized reference scan** (same as Phase 1)
- Extract GUID from each `Assets/0StarDice0/**/*.cs.meta`
- Search GUID usage in `Assets` + `ProjectSettings`

2) **Runtime usage signal scan** (new)
- Search all C# files for script-name usage patterns, e.g.
  - `AddComponent<ScriptName>`
  - `GetComponent<ScriptName>`
  - `typeof(ScriptName)`
  - direct symbol/string mention `ScriptName`

> This does not fully replace Unity Editor reference tools, but significantly reduces over-reporting compared to GUID-only scan.

## Phase 2 result summary
- Total scripts scanned (`Assets/0StarDice0/**/*.cs`): **231**
- Clearly referenced (serialized GUID found): **171**
- Not GUID-referenced but has runtime usage signals: **57**
- No GUID reference and no runtime signal (**highest dead risk**): **3**

## Highest-risk dead script candidates (3)
1. `Assets/0StarDice0/Scripts/MiniGame/CodeFappyBird/ScoreTriggle.cs`
2. `Assets/0StarDice0/Scripts/MainGame/dice panel/LoopingTextScroller.cs`
3. `Assets/0StarDice0/Scripts/Test/TestFight/enemy/enemydark/EnemyDarkBuff.cs`

## Build Settings scene risk notes
Still recommended to manually verify suspicious entries in Build Settings (possible stale/legacy naming):
- `Assets/0StarDice0/Scenes/Etc/FirstDeck.unity`
- `Assets/0StarDice0/Scenes/InterMissionScene/Monster select/FirstMonsterSelect.unity`
- `Assets/0StarDice0/Scenes/fightenemy/dark/boss dark'.unity`

## Practical next actions
1. Open each of the 3 high-risk scripts in Unity and run **Find References In Scene/Project**.
2. If no references, remove in a small PR (1 script at a time) and run playtest for related flow.
3. Keep the 57 “runtime-signal” scripts as **review-needed**, not immediate delete.
