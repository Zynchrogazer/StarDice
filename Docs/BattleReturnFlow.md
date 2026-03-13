# Battle ↔ Board Flow (Bridge-based)

เอกสารนี้สรุป flow ตอนชนะ Battle เมื่อใช้ `BattleHealthSyncBridge` + `BattleResultFlowService` เป็นแกนหลัก

## 1) ตอนเข้า Battle
- `GameEventManager` จะจำชื่อ board scene ปัจจุบันด้วย key `LastBoardSceneName` ก่อน แล้วโหลด battle scene แบบ additive
- ระหว่างโหลด battle scene จะซ่อน root objects ของ board ชั่วคราว เพื่อให้เห็นเฉพาะ battle
- เมื่อ battle scene ถูกโหลด `BattleHealthSyncBridge` จะ sync ค่า state จาก `PlayerState` ไปยัง battle controller เก่า
  - inject `selectedPlayer`
  - sync `playerHP` / HP bar
  - ผูกปุ่มใน win/lose panel ให้เรียก service กลาง (ถ้ายังไม่มี persistent onClick)

## 2) ตอนชนะและกด Claim Reward
- ปุ่มใน win panel จะวิ่งเข้า `BattleResultFlowService.HandleRewardAndReturnToBoard(...)`
- ระบบหา human `PlayerState` แล้วทำ:
  - `RecordBattleWin()` → เพิ่ม `WinCount` และได้ EXP 50 (`GainExp(50)`)
  - สุ่มเครดิตตามช่วง min/max แล้วบวกเข้า `PlayerState.PlayerCredit`
  - sync เครดิตกลับ `GameData.Instance.selectedPlayer.AddCredit(...)` เพื่อ persist
- จากนั้นตั้งธง `PlayerPrefs[PendingBattleReturn] = 1` และเริ่ม coroutine ย้ายฉากกลับ board

## 3) กลับ Board และ unload Battle
- `BattleResultFlowService.ReturnToSceneKeepingRuntimeHub(...)` จะ:
  - โหลด board scene (ถ้ายังไม่ loaded)
  - set active scene เป็น board
  - unload ทุก scene ที่ไม่ใช่ `RuntimeHub` และไม่ใช่ target board (จึงรวม battle scene ที่เพิ่งเล่น)
- ตอน battle scene ถูก unload, `BattleHealthSyncBridge.OnSceneUnloaded(...)` จะอ่าน `playerHP` จาก battle แล้ว sync กลับ `PlayerState.PlayerHealth`

## 4) Board ฟื้นระบบเทิร์นหลังกลับจาก Battle
- `GameEventManager` ยิง `OnBoardSceneReady`
- `GameTurnManager` ฟัง event นี้และเช็กธง `PendingBattleReturn`
  - ถ้าเป็น 1: reset ธง, refresh players/map, reset state machine และเริ่ม `StartTurnRoutine()` ใหม่

## 5) เรื่อง Item / EXP / Credit
- EXP: ได้ผ่าน `RecordBattleWin()` แบบคงที่ 50 ต่อชัยชนะ (runtime ใน board session)
- Credit: ได้จากการสุ่ม reward ใน `BattleResultFlowService`, แล้ว sync เข้า `PlayerData` ผ่าน `AddCredit`
- Bridge จะ sync ค่าที่ battle script เขียนลง `selectedPlayer` (runtime clone) กลับไปยัง board ก่อน destroy clone
  - รองรับ `Credit`, `level`, `currentExp`, `maxExp`
- Item: ใน flow กลางนี้ยัง **ไม่มี** การแจก item โดยตรง
  - ถ้ามี item drop จะต้องมาจากสคริปต์ battle เฉพาะฉากที่เรียก `EquipmentManager.Instance.UnlockItem(...)` เอง

## 6) Win panel ที่เด้งขึ้นมา กดแล้วกลับ board ได้อย่างไร
- ถ้าใช้ `BattleResultPanelController` → ปุ่ม `claimRewardButton` เรียก `OnClaimRewardClicked()` โดยตรง
- ถ้า prefab เก่าไม่มี controller → `BattleHealthSyncBridge` จะ auto-wire ปุ่มใน `winPanel` ให้เรียก `BattleResultFlowService`
- เมื่อกด claim reward แล้ว service จะพากลับ board + unload battle ตามขั้นตอนข้อ 3
