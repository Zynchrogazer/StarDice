# Script Usage Scan (static)

Method: match each C# script GUID against YAML assets (`.unity/.prefab/.asset/...`) under `Assets`.

Important: scripts attached dynamically via `AddComponent` or loaded via reflection may appear unused in this scan.

Total scanned scripts: **190**
Scripts with no serialized GUID reference found: **46**

## Candidates (manual verify before delete)

- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/CardComparer.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/CardDisplayOrder.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/CardSorter.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckData.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckSlotRightClick.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/character code/OpenSkillPanelButton.cs`
- `Assets/0StarDice0/Scripts/Code/CodeMenu/CharacterSelectMenu.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/BattleRewardButton.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/CardMain/BoardGameCamera.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/CardMain/TileClickable.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/EffectPanel/rotatingwarp.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/NPC/AIController.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/NPC/SimpleAI.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/PlayerHealth.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/SceneController.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/Temp/SkillConnection.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/Temp/SkillTreeUI.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_BossController/BossSceneController.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_CameraManager/CameraLookAtOrigin.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Events/EventsList/WarpTo.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Events/ITileEffect.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Events/TileEffectSO.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/BattleHealthSyncBridge.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/GameSystem.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/PlayerStatAggregator.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Player/DisplayStatus.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Player/PlayerController.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Player/PlayerInventory.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Player/TestMovementController.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_RouteManager/FindNodeByTileID.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_RouteManager/RouteSelector.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_RouteManager/TileData.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/box/HealNode.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/box/PopupNode.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/box/TrapNode.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/dice panel/LoopingTextScroller.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/dice panel/LoopingTextScrollerSwitch.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/status/StatusPanelClickBlocker.cs`
- `Assets/0StarDice0/Scripts/Code/MiniGame/CodeFappyBird/PipeSpawner.cs`
- `Assets/0StarDice0/Scripts/Code/MiniGame/CodeFappyBird/ScoreTriggle.cs`
- `Assets/0StarDice0/Scripts/Code/MiniGame/MiniGameRewardService.cs`
- `Assets/0StarDice0/Scripts/Code/Test/TestFight/BattleAudioUtility.cs`
- `Assets/0StarDice0/Scripts/Code/Test/TestFight/BattleCardHandResolver.cs`
- `Assets/0StarDice0/Scripts/Code/Test/TestFight/BattleState.cs`
- `Assets/0StarDice0/Scripts/Code/Test/TestFight/StartBattle.cs`
- `Assets/0StarDice0/Scripts/Code/Test/TestFight/enemy/enemydark/EnemyDarkBuff.cs`
