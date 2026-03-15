# TryGet Refactor Priority Plan

เอกสารนี้จัดลำดับรีแฟคเตอร์เพื่อลดการพึ่ง `TryGet(...)` ในโปรเจกต์ โดยเน้นลด runtime lookup ในจุดที่เรียกถี่และกระทบ gameplay flow ก่อน

## Scope และวิธีนับ

- นับเฉพาะ `TryGet(` แบบ custom/service access ในโค้ดเกมหลัก
- ใช้ผลจากคำสั่ง `rg -o "TryGet\\s*\\(" ... | awk ... | sort -nr`
- แยกกลุ่ม `TestFight` ออกจาก `MainGame` เพื่อไม่ให้ข้อมูล test-heavy กลบจุดสำคัญฝั่ง production

## Snapshot (ล่าสุด)

- `TryGet(` ใน `Assets/0StarDice0`: **2661 จุด**
- กลุ่ม `MainGame + CodeInterMission + CodeMenu + MainMenu + _Flow` ไฟล์สูงสุด:
  - `MainGame/dice panel/DiceRollerFromPNG.cs` = 6
  - `MainGame/_GameSystem/BoardManager.cs` = 6
  - `MainGame/_Events/GameEventManager.cs` = 6
  - `CodeInterMission/Deck/DeckManager.cs` = 5
  - `MainGame/_Player/PlayerPathWalker.cs` = 4

## ลำดับรีแฟคเตอร์ที่แนะนำ (Priority 1 -> 4)

## Priority 1 — Core flow services (เริ่มก่อน)

ไฟล์เป้าหมาย:
- `Assets/0StarDice0/Scripts/Code/MainGame/_GameSystem/BoardManager.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Events/GameEventManager.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/dice panel/DiceRollerFromPNG.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/Deck/DeckManager.cs`

เหตุผล:
- เป็นตัวกลางของ flow หลัก (board turn/event/deck/dice)
- ถูกเรียกจากหลายระบบและมีโอกาสถูกเรียกต่อเฟรม/ต่อเทิร์น

แนวทาง:
1. เพิ่ม serialized reference หรือ explicit resolver injection ใน `Awake/Start` แล้ว cache local field
2. เก็บ `TryGet` ไว้เป็น fallback เฉพาะ init path (ห้ามเรียกซ้ำใน loop/update)
3. เพิ่ม warning log ครั้งเดียวตอน resolve ไม่ได้ เพื่อให้หา scene wiring ผิดได้ง่าย

Definition of done:
- call site ใน `Update`/hot path เปลี่ยนเป็น field access ทั้งหมด
- `TryGet` เหลือเฉพาะ init และ transition scene

## Priority 2 — Flow boundary / scene transition

ไฟล์เป้าหมาย:
- `Assets/0StarDice0/Scripts/Code/_Flow/SceneFlowController.cs`
- `Assets/0StarDice0/Scripts/Code/CodeInterMission/ShopInterMission/ChangeSceneButton.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_RouteManager/RouteManager.cs`

เหตุผล:
- เป็นรอยต่อ scene และการเปลี่ยน state ที่ทำให้ dependency ซ่อนอยู่ได้ง่าย

แนวทาง:
1. กำหนด owner ชัดเจนว่า object ไหน inject dependency ให้
2. รวมจุด resolve ให้จบที่ entry point ของ scene
3. ตัดการเรียก `TryGet` กระจายหลายจุดใน method chain

## Priority 3 — Player/UI interaction

ไฟล์เป้าหมาย:
- `Assets/0StarDice0/Scripts/Code/MainGame/_Player/PlayerPathWalker.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/_Events/NormaUIManager.cs`
- `Assets/0StarDice0/Scripts/Code/MainGame/CardMain/PlayerCardInventory.cs`
- `Assets/0StarDice0/Scripts/Code/MainMenu/MainMenuController.cs`

เหตุผล:
- เป็น interaction ที่เกิดบ่อยจากผู้เล่น
- ลด latency ยิบย่อยและลด null-guard ซ้ำซ้อนที่ call-site

## Priority 4 — TestFight mass-callsites (ทำทีหลังแบบ batch)

ไฟล์เป้าหมาย:
- กลุ่ม `Assets/0StarDice0/Scripts/Code/Test/TestFight/enemy/**` (หลายไฟล์มี 48 จุดเท่ากัน)

เหตุผล:
- จำนวนมาก แต่ pattern ซ้ำกันสูง เหมาะกับการแก้แบบ template/batch หลัง core flow เสถียร

แนวทาง:
1. สร้าง helper กลางสำหรับ lock/remove/use card ที่รับ dependency ผ่าน parameter
2. แทนที่ call-site ซ้ำด้วย utility เดียว
3. ถ้าต้องคง `TryGet` ให้ย้ายไปชั้นเดียว (facade) แล้วให้ enemy scripts เรียก facade

## Quick wins (ทำได้ทันที)

1. ห้ามเรียก `TryGet` ซ้ำใน method เดียวกัน: resolve ครั้งเดียวแล้ว reuse ตัวแปร
2. ห้ามเรียก `TryGet` ใน loop ที่รู้จำนวนรอบมาก
3. สำหรับ manager สำคัญ ให้มี `SerializeField` อ้างอิงตรงก่อน fallback
4. เพิ่ม unit-ish playmode check อย่างน้อย 1 เคสต่อระบบหลักว่า resolve ได้ตั้งแต่เริ่ม scene

## ตัวชี้วัดหลังรีแฟคเตอร์

- จำนวน `TryGet(` ใน scope production ลดลงอย่างน้อย 30%
- call-site `TryGet` ใน `Update`/turn loop ลดลงเป็น 0
- ไม่มี warning `dependency unresolved` ใน happy path scene หลัก
- เวลาเฉลี่ย frame ในฉาก gameplay ไม่แย่ลงหลังย้าย wiring