# MainLight Heal Gimmick Setup (Unity)

เอกสารนี้เป็นขั้นตอนตั้งค่าแบบเร็ว สำหรับระบบกิมมิคช่อง Heal ชั่วคราวของด่านธาตุแสง หลังแยก logic ออกจาก `RouteManager` ไปที่ `MainLightHealGimmickController` แล้ว

## 1) เตรียมฉาก
1. เปิดฉากบอร์ดที่ต้องการ (เช่น `MainLight`)
2. ตรวจว่าในฉากมี `RouteManager` อยู่แล้ว และ `nodeConnections` ถูกเซ็ตครบ

## 2) สร้าง GameObject สำหรับ gimmick
1. ใน Hierarchy กด **Create Empty**
2. ตั้งชื่อเช่น `MainLightHealGimmickSystem`
3. กด **Add Component** แล้วเพิ่ม:
   - `MainLightHealGimmickController`
   - `MainLightHealGimmickTurnTicker`

> แนะนำให้รวม 2 component นี้ไว้ object เดียวกันเพื่อดูแลง่าย (KISS)

## 3) ผูก Reference
### MainLightHealGimmickController
- `Route Manager` : ลาก object ที่มี `RouteManager` มาใส่
- ปรับค่าตามเกมดีไซน์:
  - `Enable Main Light Heal Gimmick`
  - `Main Light Heal Only In Main Light`
  - `Main Light Heal Duration Turns`
  - `Main Light Heal Min Tiles`
  - `Main Light Heal Max Tiles`
  - `Enable Auto Trigger By Turn` (เปิดเพื่อให้สุ่มเองทุก N เทิร์น)
  - `Auto Trigger Interval Turns` (ค่าเริ่มต้น 4 เทิร์น / ปรับได้)
  - `Auto Trigger Only Player Turn` (ถ้าเปิดจะไม่นับเทิร์น AI)

### MainLightHealGimmickTurnTicker
- `Heal Gimmick Controller` : ลาก component `MainLightHealGimmickController` มาใส่
- `Enable Turn Tick` : เปิดใช้งาน

## 4) ผูกกับ GameEventManager
1. เลือก object ที่มี `GameEventManager`
2. ในช่อง `Main Light Heal Gimmick Controller` ลาก `MainLightHealGimmickController` มาใส่
3. ตรวจว่า event key บน tile ที่ต้องการเรียกเป็น `mainlighthealgimmick`

## 5) ทดสอบ
1. เข้า Play Mode ในฉาก `MainLight`
2. ทำให้ตัวละครเดินไปช่องที่ trigger event `mainlighthealgimmick`
3. ตรวจผล:
   - มีการสุ่มเปลี่ยน tile เป็น `Heal` ตามช่วง min/max
   - ถ้าเปิด auto trigger จะสุ่มเองทุก `Auto Trigger Interval Turns` (เช่นทุก 4 เทิร์น)
   - ครบจำนวนเทิร์นแล้ว tile กลับค่าเดิม
   - tile ที่ `lockRandomType` จะไม่ถูกเปลี่ยน

## 6) ปัญหาที่พบบ่อย
- Trigger ไม่ทำงาน:
  - ลืมผูก `MainLightHealGimmickController` ใน `GameEventManager`
  - ชื่อ scene ไม่ใช่ `MainLight` ขณะเปิด `Main Light Heal Only In Main Light`
- Tick ไม่เดิน:
  - ลืมเพิ่ม `MainLightHealGimmickTurnTicker`
  - `Enable Turn Tick` ถูกปิด
- Visual ไม่เปลี่ยน:
  - `RouteManager.tileVisualSettings` ไม่มี entry ของ `TileType.Heal`
