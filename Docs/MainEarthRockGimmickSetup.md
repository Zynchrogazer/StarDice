# MainEarth Rock Gimmick (SOLID + KISS) — Overview & Setup

เอกสารนี้สรุป 2 เรื่อง:
1) ประเมินว่าโครงสร้างระบบสุ่มหินของด่าน `MainEarth` เข้าแนวคิด **SOLID + KISS** แค่ไหน
2) ขั้นตอน **Setup/Verify** แบบทำตามได้ทันที

---

## 1) โครงสร้างระบบ (ภาพรวม)

ระบบหินของด่าน Earth ถูกแยกหน้าที่หลักเป็น 3 ส่วน

- `RockObstacleSpawnByTurn`
  - รับผิดชอบการ “นับเทิร์น” และ “ตัดสินใจว่าเมื่อไรจะ spawn หิน”
  - subscribe `GameTurnManager.OnTurnChanged`
  - ครบทุก `N` เทิร์นจึงเรียก `RouteManager.TrySpawnRandomRockObstacle()`

- `RouteManager`
  - รับผิดชอบข้อมูลบอร์ดและการจัดการสถานะหิน
  - เลือก candidate tile ที่วางหินได้
  - activate/break rock obstacle

- `PlayerPathWalker`
  - รับผิดชอบพฤติกรรมตอนผู้เล่นเดินชนหิน
  - ถ้าชนหิน: ทำลายหิน + เด้งกลับช่องก่อนหน้า

- `RockObstacleTrigger2D` (Legacy / Optional)
  - เก็บไว้เพื่อรองรับฉากเก่า
  - ค่าเริ่มต้นปิด `enableLegacyTriggerBreak = false` เพื่อไม่ให้ซ้ำกับ `PlayerPathWalker`

---

## 2) ประเมิน SOLID + KISS (อัปเดตล่าสุด)

## ✅ จุดที่เข้าหลัก KISS/SOLID แล้ว

- **SRP (Single Responsibility) ดีขึ้นชัดเจน**
  - logic การนับเทิร์นเพื่อ spawn หิน ถูกแยกจาก `RouteManager` ออกมาเป็น `RockObstacleSpawnByTurn`
  - `RouteManager` โฟกัสเรื่อง data/activation ของเส้นทางและ obstacle

- **KISS (เรียบง่าย ตรงไปตรงมา)**
  - flow หลักสั้นมาก: OnTurnChanged → นับเทิร์น → เช็ก scene/filter → spawn
  - guard clauses เยอะ อ่านง่าย ดูแลรักษาง่าย

- **Low coupling แบบ practical**
  - มี `TryGet` fallback ทั้ง `GameTurnManager` และ `RouteManager`
  - ทำให้ setup ไม่เปราะเมื่อ reference ยังไม่ถูกลากใน inspector

## ⚠️ จุดที่ยังพัฒนาให้ SOLID ขึ้นได้อีก

- **DIP ยังไม่เต็มรูปแบบ**
  - ตอนนี้พึ่งพา concrete class (`RouteManager`, `GameTurnManager`) โดยตรง
  - ถ้าต้องรองรับหลายรูปแบบบอร์ด แนะนำ interface เช่น `IRockObstacleService`

สรุป: **แนวทางตอนนี้ถือว่า KISS และ SRP ดีมากแล้ว** โดยแยก owner ของการตั้งค่า spawn interval/scene filter ไปไว้ที่ `RockObstacleSpawnByTurn` และตั้งค่าเริ่มต้นให้ใช้เส้นทางชนหินผ่าน `PlayerPathWalker` ทางเดียว

---

## 3) Setup แบบเร็ว (MainEarth)

## 3.1) ผลการ Scan จาก `MainEarth.unity` (อัปเดตล่าสุด)

- มี GameObject ชื่อ `RouteManager` และมี component `RouteManager` อยู่ในฉาก
- มี GameObject ชื่อ `_GameTurnManager` อยู่ในฉาก
- เพิ่ม component `RockObstacleSpawnByTurn` ลงบน GameObject `RouteManager` แล้ว
  - `enableRandomRockSpawnByTurn = true`
  - `randomRockSpawnIntervalTurns = 3`
  - `randomRockOnlyInMainEarth = true`
  - `mainEarthSceneName = "MainEarth"`
- ฝั่ง `RouteManager` มีค่า:
  - `rockObstaclePrefab` ถูกผูกแล้ว
  - `initialRockTileIDs` ถูกตั้งไว้แล้ว (6, 51, 24, 37, 47)
  - ลบ field spawn-cycle เก่าออกจาก scene config แล้ว

## Step A — ตรวจวัตถุหลักใน Scene

1. เปิดฉาก `MainEarth`
2. ต้องมี `GameTurnManager`
3. ต้องมี `RouteManager`

## Step B — ตั้งค่า `RouteManager`

ในส่วน Rock Obstacle:

- `rockObstaclePrefab` : prefab ของหิน
- `initialRockTileIDs` : tile ที่ต้องการวางหินตั้งแต่เริ่ม
- `activeRockObstacles` : runtime state ที่ sync กับ inspector

## Step C — เพิ่มตัวนับเทิร์นกิมมิค

1. สร้าง GameObject เช่น `MainEarthRockGimmickSystem`
2. Add Component: `RockObstacleSpawnByTurn`
3. ผูก `routeManager` (แนะนำให้ลากตรง)
4. ตั้งค่า
   - `enableRandomRockSpawnByTurn = true`
   - `randomRockSpawnIntervalTurns = 3~5`
   - `randomRockOnlyInMainEarth = true`
   - `mainEarthSceneName = MainEarth`
   - `verboseLog = true` (เปิดชั่วคราวตอน debug)

## Step D — ปิด Legacy Trigger เป็นค่าเริ่มต้น (สำคัญ)

ถ้าในฉากมี `RockObstacleTrigger2D` อยู่ ให้ตั้ง `enableLegacyTriggerBreak = false`  
เพื่อไม่ให้ซ้ำกับ logic หลักใน `PlayerPathWalker`

> ค่าแนะนำเริ่มต้น: `randomRockSpawnIntervalTurns = 3` หรือ `5`
> (ถ้าเป็น `1` เกมจะ spawn ถี่มาก)

---

## 4) Verify การทำงาน

1. เข้า Play Mode ที่ฉาก `MainEarth`
2. เดินให้ครบจำนวนเทิร์นตาม interval
3. ตรวจ log ว่ามีการสุ่มหิน
4. ให้ผู้เล่นชนหิน
5. คาดหวังผล:
   - หินแตก
   - ตัวละครเด้งกลับช่องก่อนหน้า
6. ทดสอบครบ 2 เงื่อนไข:
   - รอบที่ไม่ถึง interval: ต้อง **ไม่ spawn**
   - รอบที่ถึง interval: ต้อง **spawn 1 ก้อน** (ถ้ามี candidate)

---

## 5) Troubleshooting

- ไม่ spawn หินเลย
  - ไม่มี `RockObstacleSpawnByTurn` ในฉาก
  - `enableRandomRockSpawnByTurn` ปิดอยู่
  - ชื่อ scene ไม่ตรงกับ `mainEarthSceneName`
  - ไม่มี candidate tile เหลือให้วาง

- ชนหินแล้วไม่เด้ง
  - tile นั้นไม่ได้ active เป็น rock obstacle จริง
  - `PlayerPathWalker` ไม่ได้เชื่อมกับ `RouteManager` ตัวเดียวกัน
  - ถ้าใช้ฉากเก่าและเปิด `RockObstacleTrigger2D` ให้ตรวจว่าตั้ง `enableLegacyTriggerBreak` ตามที่ต้องการ

- spawn ถี่/ยากเกินไป
  - ปรับ `randomRockSpawnIntervalTurns`
  - ปรับจำนวน `initialRockTileIDs`

---

## 6) ข้อเสนอปรับปรุง (Optional)

- เพิ่ม `RockGimmickConfig` (ScriptableObject) สำหรับรวม config จุดเดียว
- เพิ่ม debug panel เล็ก ๆ แสดง counter / next spawn turn
- ถ้าจะ scale หลายด่าน ค่อยแยก interface service เพื่อ DIP

---

## 7) Legacy Trigger (เฉพาะกรณีจำเป็น)

ค่าแนะนำปัจจุบันคือ **ปิด** `RockObstacleTrigger2D.enableLegacyTriggerBreak` และให้ใช้ `PlayerPathWalker` เป็นทางหลัก  
ถ้าต้องรองรับ behavior ฉากเก่าจริง ๆ ค่อยเปิดเฉพาะจุดที่จำเป็น แล้วทดสอบไม่ให้เกิดการ break ซ้ำ
