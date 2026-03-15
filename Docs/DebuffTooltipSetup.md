# ตั้งค่า Debuff Icon + Tooltip ใน Unity (Step by Step)

เอกสารนี้อธิบายวิธีตั้งค่า UI ให้ระบบ Debuff ใหม่ใช้งานได้ครบ:
- แสดง icon debuff เรียงซ้าย → ขวาตามลำดับที่ติดสถานะ
- เอาเมาส์ไปชี้ที่ icon แล้วขึ้น tooltip อธิบายสถานะ + เวลาที่เหลือ

> สคริปต์ที่ใช้: `PlayerUIController` + `DebuffTooltipHoverHandler`

---

## 1) เตรียม EventSystem

1. เปิด Scene ที่ใช้เล่นบอร์ดหลัก
2. ดูใน Hierarchy ว่ามี `EventSystem` แล้วหรือยัง
3. ถ้ายังไม่มี ให้สร้าง: `GameObject > UI > Event System`

เหตุผล: ระบบ hover ของ tooltip ใช้ event pointer จาก EventSystem

---

## 2) เตรียม Debuff Icon Text

1. ไปที่ Panel สถานะผู้เล่น (status panel)
2. สร้าง `Text - TextMeshPro` ใหม่ 1 ตัว
3. ตั้งชื่อแนะนำ: `DebuffIconText`
4. ตั้งค่าพื้นฐาน:
   - เปิด `Rich Text`
   - เปิด `Raycast Target` (สำคัญมาก)
   - จัดตำแหน่งให้อยู่ในแถวสถานะ

หมายเหตุ:
- ถ้าอยากผูกแบบ auto-find ให้ชื่อ object มีคำว่า `debuff`
- แต่แนะนำให้ลากผูกใน Inspector โดยตรงเพื่อชัวร์ที่สุด

---

## 3) เตรียม Tooltip Panel

1. ใต้ Canvas สร้าง Panel ใหม่ชื่อ `DebuffTooltip`
2. ใส่ลูกเป็น `Text - TextMeshPro` ชื่อ `DebuffTooltipText`
3. ตั้งค่า `DebuffTooltip`:
   - ปิด active ไว้ตั้งแต่เริ่มเกม (`SetActive(false)`)
   - ใส่พื้นหลังอ่านง่าย
4. ตั้งค่า `DebuffTooltipText`:
   - เปิด Word Wrap
   - ปรับขนาดให้รองรับ 2 บรรทัดขึ้นไป

---

## 4) ผูก Reference ใน PlayerUIController

เลือก GameObject ที่มี `PlayerUIController` แล้วตั้งค่าดังนี้:

1. `Debuff Text`  → ลาก `DebuffIconText`
2. `Debuff Tooltip Root` → ลาก `DebuffTooltip`
3. `Debuff Tooltip Text` → ลาก `DebuffTooltipText`

> ถ้าไม่ผูก tooltip ก็ยังเห็น icon ได้ แต่จะไม่ขึ้นกล่องอธิบาย

---

## 5) ตรวจ PlayerState / Event ว่าต่อครบ

ระบบนี้ต้องใช้ flow ด้าน gameplay ครบตามนี้:

1. Ice ต้องเข้าผ่าน `ApplyIceDebuff()` (ไม่เซ็ต flag ตรง ๆ)
2. Burn ใช้ `ApplyBurnDebuff(turns)`
3. ตอน consume ต้องลด/ล้างสถานะตามเทิร์น

ในโค้ดล่าสุดของโปรเจกต์มีการต่อไว้แล้ว:
- Ice event เรียก `ApplyIceDebuff()`
- PlayerState เก็บลำดับการติด debuff เพื่อใช้เรียง icon

---

## 6) วิธีเทสใน Play Mode

1. เข้า Play Mode
2. ทำให้ผู้เล่นติด Burn ก่อน แล้วติด Ice ตาม
3. เช็กว่า icon เรียงเป็น `🔥  ❄️`
4. เอาเมาส์ชี้ที่ `🔥` ต้องเห็นข้อความ Burn + เทิร์นคงเหลือ
5. เอาเมาส์ชี้ที่ `❄️` ต้องเห็นข้อความ Ice + คงเหลือ 1 ครั้ง
6. รอ Burn หมด แล้วเช็กว่า `❄️` เลื่อนมาอยู่ซ้าย (แทนที่ตำแหน่งเดิม)

---

## 7) Troubleshooting (ปัญหาที่เจอบ่อย)

### ไม่ขึ้น tooltip ตอน hover
- เช็ก `EventSystem` มีอยู่จริง
- เช็ก `DebuffIconText` เปิด `Raycast Target`
- เช็ก Canvas/GraphicRaycaster ยังทำงาน
- เช็กว่า `Debuff Tooltip Root` และ `Debuff Tooltip Text` ถูกลากผูก

### เห็นแต่เครื่องหมาย `-`
- หมายถึงตอนนั้นยังไม่มี debuff ที่ active
- ลอง trigger event ที่ทำให้ติด Burn/Ice แล้วดูอีกครั้ง

### icon ไม่เรียงตามลำดับที่ติด
- ตรวจว่า Ice ถูก apply ผ่าน `ApplyIceDebuff()`
- ตรวจว่าไม่มีจุดอื่นไปเซ็ต `hasIceEffect = true` ตรง ๆ

---

## 8) Checklist ส่งงานให้ทีม

- [ ] มี EventSystem ในฉาก
- [ ] มี DebuffIconText และเปิด Raycast Target
- [ ] มี DebuffTooltip + DebuffTooltipText
- [ ] ผูก 3 ช่องใน PlayerUIController ครบ
- [ ] เทส Burn/Ice hover ผ่านใน Play Mode
