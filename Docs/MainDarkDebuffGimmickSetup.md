# MainDark Debuff Gimmick Setup (KISS + SOLID)

เอกสารนี้ใช้สำหรับตั้งค่ากิมมิคด่านมืดที่ “สุ่ม debuff จาก pool เดียว” ใส่ผู้เล่น คล้ายแนวคิด light gimmick แต่โฟกัสฝั่งสถานะผิดปกติ

---

## 1) โครงสร้างระบบที่เพิ่ม

- `MainDarkDebuffGimmickController`
  - ดูแลเฉพาะการสุ่ม debuff และ apply ให้ `PlayerState`
  - รองรับ weighted random ผ่าน `Debuff Pool`
  - รองรับ auto trigger รายเทิร์น

- `MainDarkDebuffGimmickTurnTicker`
  - subscribe `GameTurnManager.OnTurnChanged`
  - เรียก `MainDarkDebuffGimmickController.TickTurn(isAITurn)`

- `GameEventManager`
  - เพิ่ม event key:
    - `maindarkdebuffgimmick`
    - `darkdebuffgimmick`
  - เรียก controller โดยตรง และ fallback ไป `TriggerRandomDebuff` ถ้าไม่พร้อม

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
- `Simulate Turns If No Turn Manager` → เปิดเฉพาะกรณีเทสต์ scene แยก

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

- `weight` = น้ำหนักสุ่ม (ยิ่งมากยิ่งมีโอกาสออก)
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

- [ ] ใน `Debuff Pool` มีอย่างน้อย 1 รายการที่ `weight > 0`
- [ ] รายการ Burn/Curse/Poison/Sleep ตั้ง `turns >= 1`
- [ ] ทดสอบสุ่มอย่างน้อย 10 ครั้งเพื่อดู distribution คร่าว ๆ

## Runtime Behavior

- [ ] ผู้เล่นติด debuff ได้จริง (ดูทั้งผลลัพธ์และ UI)
- [ ] กรณี controller ไม่พร้อม ระบบ fallback ไป random debuff เดิมได้
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
  - ตรวจว่ามีรายการที่ `weight > 0`

- auto trigger ไม่ทำงาน
  - ตรวจ `Enable Auto Trigger By Turn`
  - ตรวจว่ามี `GameTurnManager`
  - ถ้าเทสต์ scene แยก ให้เปิด `Simulate Turns If No Turn Manager`

---

## 6) Scope ปรับปรุงรอบต่อไป (Optional)

- แยก Debuff Pool ไป `ScriptableObject` เพื่อแชร์หลายด่าน
- เพิ่ม anti-streak rule (เช่น ห้ามออกซ้ำเดิมเกิน 2 ครั้ง)
- เพิ่ม telemetry (นับสถิติ debuff ต่อ match)
