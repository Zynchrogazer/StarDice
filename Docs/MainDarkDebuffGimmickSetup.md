# MainDark Debuff Gimmick Setup (KISS + SOLID)

เอกสารนี้ใช้สำหรับตั้งค่ากิมมิคด่านมืดที่ “สุ่ม debuff จาก pool เดียว” ใส่ผู้เล่น คล้ายแนวคิด light gimmick แต่โฟกัสฝั่งสถานะผิดปกติ

---

## 1) โครงสร้างระบบที่เพิ่ม

- `MainDarkDebuffGimmickController`
  - ดูแลเฉพาะการสุ่ม debuff 1 ชนิดต่อการ trigger และ apply ให้ `PlayerState`
  - รองรับ auto trigger รายเทิร์น

- `MainDarkDebuffGimmickTurnTicker`
  - subscribe `GameTurnManager.OnTurnChanged`
  - เรียก `MainDarkDebuffGimmickController.TickTurn(isAITurn)`

- `GameEventManager`
  - เพิ่ม event key:
    - `maindarkdebuffgimmick`
    - `darkdebuffgimmick`
  - เรียก controller โดยตรง (เหมือนแนว light gimmick)

---

## 2) Unity Setup (Step-by-step)

## Step A — เปิดฉาก

1. เปิดฉาก `MainDark`
2. ตรวจว่ามี:
   - `GameTurnManager`
   - `GameEventManager`

## Step B — สร้างระบบกิมมิค

1. ใน Hierarchy กด **Create Empty**
2. ตั้งชื่อเช่น `MainDarkDebuffGimmickSystem`
3. Add Component:
   - `MainDarkDebuffGimmickController`
   - `MainDarkDebuffGimmickTurnTicker`

> แนะนำให้อยู่ GameObject เดียวกันเพื่อดูแลง่ายตาม KISS

## Step C — ผูก Reference

### บน `MainDarkDebuffGimmickTurnTicker`

- `Dark Debuff Gimmick Controller` → ลาก component `MainDarkDebuffGimmickController`
- `Enable Turn Tick` → เปิด

### บน `GameEventManager`

- ช่อง `Main Dark Debuff Gimmick Controller` → ลาก `MainDarkDebuffGimmickController`

## Step D — ตั้ง Event Tile

บน tile ที่ต้องการให้ทริกเกอร์กิมมิค ให้ใส่ `eventName` เป็น:

- `maindarkdebuffgimmick` (แนะนำ)
หรือ
- `darkdebuffgimmick`

## Step E — ตั้งค่า Pool ใน `MainDarkDebuffGimmickController`

ค่าเริ่มต้นมี 5 debuff:

- Ice
- Burn
- Curse
- Poison
- Sleep

แต่ละรายการปรับได้:

- `turns` = จำนวนเทิร์น (Ice ใช้สถานะ 1 ครั้งตามระบบเดิม)

---

## 3) Recommended Default Config

- `Enable Main Dark Debuff Gimmick` = `true`
- `Main Dark Debuff Only In Main Dark` = `true`
- `Enable Auto Trigger By Turn` = `false` (เริ่มจาก event tile ก่อน)
- ถ้าจะ auto:
  - `Enable Auto Trigger By Turn` = `true`
  - `Auto Trigger Interval Turns` = `4`
  - `Auto Trigger Only Player Turn` = `true`

---

## 4) QA Checklist (พร้อมติ๊ก)

## Scene & References

- [ ] ฉากที่เปิดคือ `MainDark`
- [ ] มี `GameTurnManager` ใน scene
- [ ] มี `GameEventManager` ใน scene
- [ ] มี `MainDarkDebuffGimmickSystem` ใน scene
- [ ] `MainDarkDebuffGimmickSystem` ติด `MainDarkDebuffGimmickController`
- [ ] `MainDarkDebuffGimmickSystem` ติด `MainDarkDebuffGimmickTurnTicker`
- [ ] `MainDarkDebuffGimmickTurnTicker.Dark Debuff Gimmick Controller` ถูกผูกแล้ว
- [ ] `GameEventManager.Main Dark Debuff Gimmick Controller` ถูกผูกแล้ว

## Event Setup

- [ ] tile เป้าหมายตั้ง `eventName = maindarkdebuffgimmick`
- [ ] เดินเหยียบ tile แล้วระบบทำงาน

## Debuff Pool

- [ ] ใน `Debuff Pool` มีอย่างน้อย 1 รายการ
- [ ] รายการ Burn/Curse/Poison/Sleep ตั้ง `turns >= 1`
- [ ] ทุกครั้งที่ trigger จะสุ่ม apply เพียง 1 debuff

## Runtime Behavior

- [ ] ผู้เล่นติด debuff ได้จริง (ดูทั้งผลลัพธ์และ UI)
- [ ] ไม่เกิด error / null reference ระหว่าง trigger

## Auto Trigger (Optional)

- [ ] เปิด `Enable Auto Trigger By Turn`
- [ ] ตั้ง interval ตามดีไซน์
- [ ] เปิด `Auto Trigger Only Player Turn` หากไม่ต้องการติดตอน AI turn
- [ ] ทดสอบครบทั้งเคสถึงรอบและไม่ถึงรอบ

---

## 5) Troubleshooting

- ไม่ติด debuff เมื่อเหยียบ tile
  - ตรวจ `eventName` ให้ตรง (`maindarkdebuffgimmick`)
  - ตรวจว่า `GameEventManager` ผูก controller แล้ว
  - ตรวจว่า player object มี `PlayerState`

- สุ่มไม่ออกเลย
  - ตรวจว่า `Debuff Pool` ไม่ว่าง

- auto trigger ไม่ทำงาน
  - ตรวจ `Enable Auto Trigger By Turn`
  - ตรวจว่ามี `GameTurnManager`
  - ตรวจว่า `MainDarkDebuffGimmickTurnTicker` ถูก enable อยู่

---

## 6) Scope ปรับปรุงรอบต่อไป (Optional)

- แยก Debuff Pool ไป `ScriptableObject` เพื่อแชร์หลายด่าน
- เพิ่ม anti-streak rule (เช่น ห้ามออกซ้ำเดิมเกิน 2 ครั้ง)
- เพิ่ม telemetry (นับสถิติ debuff ต่อ match)
