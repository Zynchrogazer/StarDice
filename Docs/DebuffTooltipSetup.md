# Debuff Icon แบบ Image Sprite ใน Unity (KISS / Step by Step)

<<<<<<< ours
เอกสารนี้อธิบายวิธีตั้งค่า UI ให้ระบบ Debuff ใหม่ใช้งานได้ครบ โดยตอนนี้ระบบรองรับ 2 โหมด:
- **โหมดแนะนำ:** แสดง debuff เป็น **Sprite Image icon** จาก Unity Store / sprite asset ของทีม
- **โหมด fallback:** แสดง debuff เป็น **TextMeshPro icon** (`🔥`, `❄️`) แบบเดิม
- ทั้งสองโหมดรองรับ tooltip ตอนเอาเมาส์ชี้

> สคริปต์ที่ใช้: `PlayerUIController` + `DebuffTooltipHoverHandler` + `DebuffSpriteIconHoverHandler`
=======
เอกสารนี้เน้นแบบ **เรียบง่าย ใช้ได้จริง** โดยโฟกัสที่การใช้ **Image Sprite** เป็น debuff icon
และอธิบายว่าโค้ดตอนนี้ทำงานอย่างไร เพื่อให้ทีมตั้งค่าใน Unity ได้เร็วที่สุด

> สคริปต์หลักที่เกี่ยวข้อง
> - `PlayerUIController`
> - `DebuffSpriteIconHoverHandler`
> - `DebuffTooltipHoverHandler` (ใช้เฉพาะโหมดเก่าแบบ text fallback)
>>>>>>> theirs

---

## เป้าหมายของระบบ

สิ่งที่เราต้องการมีแค่ 3 อย่าง:

1. ผู้เล่นติด Burn / Ice แล้วมี **icon ขึ้นบน UI**
2. icon ต้องเรียงตาม **ลำดับที่ติด debuff จริง**
3. เอาเมาส์ชี้ที่ icon แล้วมี **tooltip** ขึ้นอธิบาย

ตอนนี้ระบบรองรับแล้ว โดยถ้าเราผูก `Debuff Icon Container` ใน `PlayerUIController`
ระบบจะใช้ **Image Sprite mode** ทันที

ถ้าไม่ผูก container ระบบจะ fallback ไปใช้ **TextMeshPro icon** แบบเดิม (`🔥`, `❄️`)

---

## โค้ดทำงานยังไงแบบสั้นที่สุด

### 1) Gameplay เป็นคนบอกว่าใครติด debuff
- Burn เข้าผ่าน `ApplyBurnDebuff(turns)`
- Ice เข้าผ่าน `ApplyIceDebuff()`
- `PlayerState` เก็บลำดับการติดไว้ เพื่อให้ UI เรียง icon ถูกต้อง

### 2) `PlayerUIController` อ่านสถานะ debuff ทุก frame
ถ้ามี debuff:
- สร้าง list ของ debuff ที่ active
- ใส่ sprite ของ Burn / Ice ลงไป
- sort ตามลำดับที่ติด

### 3) ถ้ามี `Debuff Icon Container`
`PlayerUIController` จะ:
- สร้าง object icon ใต้ container อัตโนมัติ
- ใส่ `Image.sprite`
- bind tooltip ให้แต่ละ icon
- ซ่อน text fallback ให้อัตโนมัติ

### 4) ตอน hover
`DebuffSpriteIconHoverHandler` จะ:
- รับ pointer enter / move / exit
- เปิด `DebuffTooltip`
- เอาข้อความไปใส่ใน `DebuffTooltipText`
- เลื่อน tooltip ให้ตามเมาส์

สรุปสั้น ๆ:
**Gameplay ติด debuff → PlayerUIController สร้าง icon → Hover handler แสดง tooltip**

---

# Setup ใน Unity แบบ Step by Step

## Step 1) เช็กว่ามี EventSystem

1. เปิด scene ที่จะใช้เล่นจริง
2. ดูใน Hierarchy ว่ามี `EventSystem` หรือยัง
3. ถ้ายังไม่มี ให้สร้าง `GameObject > UI > Event System`

> ถ้าไม่มีอันนี้ hover tooltip จะไม่ทำงาน

---

## Step 2) สร้างพื้นที่สำหรับวาง icon

ไปที่ status panel ของผู้เล่น แล้วทำแบบนี้

1. สร้าง UI object ใหม่ชื่อ `DebuffIconContainer`
2. ให้ object นี้อยู่ในตำแหน่งที่อยากให้ debuff icon แสดง
3. ใส่ component:
   - `Horizontal Layout Group`
   - `Content Size Fitter`

### ค่าที่แนะนำ

#### Horizontal Layout Group
- Spacing = `6` หรือ `8`
- Child Alignment = Middle Left
- Child Force Expand Width = Off
- Child Force Expand Height = Off
- Child Control Width = On
- Child Control Height = On

#### Content Size Fitter
- Horizontal Fit = Preferred Size
- Vertical Fit = Preferred Size

> KISS: ใช้แค่ container เดียวพอ ไม่ต้องสร้าง icon ลูกไว้ล่วงหน้า
> ระบบจะ spawn ให้เองตอนมี debuff

---

<<<<<<< ours
## 2) วิธีที่แนะนำ: ตั้งค่า Debuff เป็น Sprite Image Icon

### 2.1 สร้าง Container สำหรับ icon

1. ไปที่ Panel สถานะผู้เล่น (status panel)
2. สร้าง GameObject UI ใหม่ชื่อ `DebuffIconContainer`
3. แนะนำให้ใส่ component เหล่านี้:
   - `Horizontal Layout Group`
   - `Content Size Fitter` (Horizontal Fit = Preferred Size)
4. ตั้ง spacing ตามต้องการ เช่น `6 - 12`

> ระบบจะสร้าง icon ใต้ container นี้ให้อัตโนมัติ

### 2.2 เตรียม Sprite icon ที่ได้จาก Unity Store

1. Import icon asset เข้ามาในโปรเจกต์
2. เลือกไฟล์รูปที่ต้องการใช้ เช่น Burn / Ice
3. ใน Inspector ตั้งค่า Texture ให้ใช้งานกับ UI ได้ตามปกติ
4. ลาก Sprite ที่ต้องการไปผูกใน `PlayerUIController`

ช่องที่ต้องใช้:
- `Burn Debuff Sprite`
- `Ice Debuff Sprite`

### 2.3 (Optional) ถ้าอยากใช้ prefab icon ของตัวเอง

ถ้าทีมมี prefab UI icon ที่จัด layout/outline/shadow มาแล้ว ให้สร้าง prefab 1 ชิ้นแล้วใส่:
- `Image`
- `DebuffSpriteIconHoverHandler`

จากนั้นลากใส่ช่อง `Debuff Icon Prefab`

> ถ้าไม่ใส่ prefab ระบบจะสร้าง `Image` ให้เองอัตโนมัติ

### 2.4 ผูก Reference ใน PlayerUIController

เลือก GameObject ที่มี `PlayerUIController` แล้วตั้งค่าดังนี้:

1. `Debuff Icon Container` → ลาก `DebuffIconContainer`
2. `Burn Debuff Sprite` → ลาก sprite ของ Burn
3. `Ice Debuff Sprite` → ลาก sprite ของ Ice
4. `Debuff Tooltip Root` → ลาก `DebuffTooltip`
5. `Debuff Tooltip Text` → ลาก `DebuffTooltipText`
6. (Optional) `Debuff Icon Prefab` → ลาก prefab icon ของทีม
7. (Optional) `Debuff Icon Size` → ปรับขนาด icon กรณีให้ระบบสร้างอัตโนมัติ

เมื่อผูก `Debuff Icon Container` แล้ว ระบบจะใช้ **Sprite Image mode** ทันที และจะซ่อน text fallback ให้อัตโนมัติ
=======
## Step 3) เตรียม tooltip

ใต้ Canvas ให้สร้าง:

1. Panel ชื่อ `DebuffTooltip`
2. ใต้ panel สร้าง `Text - TextMeshPro` ชื่อ `DebuffTooltipText`

### ค่าแนะนำ

#### DebuffTooltip
- ปิด active ตั้งแต่เริ่ม (`SetActive(false)`)
- ใส่พื้นหลังสีเข้มโปร่งนิด ๆ
- ขนาดพอประมาณ เช่น 220x80

#### DebuffTooltipText
- เปิด Word Wrap
- สีตัวอักษรอ่านง่าย
- padding/spacing พอให้อ่านสบาย

> KISS: tooltip ใช้ panel เดียวพอ ทุก icon ใช้ร่วมกัน
>>>>>>> theirs

---

## Step 4) เตรียม sprite icon จาก Unity Store

1. import asset icon เข้ามาในโปรเจกต์
2. เลือกรูปที่ต้องการใช้เป็น Burn / Ice
3. ให้แน่ใจว่าไฟล์นั้นใช้เป็น `Sprite (2D and UI)` ได้
4. เตรียมอย่างน้อย 2 รูป:
   - รูป Burn
   - รูป Ice

> ตอนนี้โค้ดรองรับ Burn กับ Ice โดยตรงก่อน

---

<<<<<<< ours
## 4) โหมด fallback: ถ้ายังไม่ใช้ Sprite ให้ใช้ TMP Text แบบเดิม

ถ้ายังไม่พร้อมทำ icon sprite สามารถใช้แบบเดิมได้:

1. ไปที่ Panel สถานะผู้เล่น (status panel)
2. สร้าง `Text - TextMeshPro` ใหม่ 1 ตัว
3. ตั้งชื่อแนะนำ: `DebuffIconText`
4. ตั้งค่าพื้นฐาน:
   - เปิด `Rich Text`
   - เปิด `Raycast Target` (สำคัญมาก)
   - จัดตำแหน่งให้อยู่ในแถวสถานะ
5. ลากไปผูกที่ช่อง `Debuff Text`

หมายเหตุ:
- ถ้าอยากผูกแบบ auto-find ให้ชื่อ object มีคำว่า `debuff`
- แต่แนะนำให้ลากผูกใน Inspector โดยตรงเพื่อชัวร์ที่สุด
- ถ้ามี `Debuff Icon Container` อยู่แล้ว ระบบจะใช้ sprite icon ก่อน และซ่อน text fallback อัตโนมัติ
=======
## Step 5) ผูกใน PlayerUIController

เลือก GameObject ที่ติด `PlayerUIController` แล้วลาก reference ตามนี้

### ช่องที่ต้องผูกจริง ๆ
1. `Debuff Icon Container` → ลาก `DebuffIconContainer`
2. `Burn Debuff Sprite` → ลากรูป Burn
3. `Ice Debuff Sprite` → ลากรูป Ice
4. `Debuff Tooltip Root` → ลาก `DebuffTooltip`
5. `Debuff Tooltip Text` → ลาก `DebuffTooltipText`

### ช่องที่ไม่จำเป็นแต่ใช้ได้
6. `Debuff Icon Prefab` → ใส่เฉพาะกรณีคุณมี prefab icon สำเร็จรูปอยู่แล้ว
7. `Debuff Icon Size` → ใช้ตอน **ไม่ได้** ใส่ prefab และอยากกำหนดขนาด icon ที่ระบบสร้างให้เอง

> ถ้าคุณอยากให้ setup ง่ายที่สุด: **ไม่ต้องใช้ prefab**
> แค่ผูก container + sprite + tooltip ก็พอ
>>>>>>> theirs

---

## Step 6) ถ้าอยากใช้ prefab icon ของตัวเอง

กรณีคุณมี asset UI สวย ๆ และอยากให้ icon มี frame / outline / glow
ให้ทำ prefab 1 ชิ้นชื่ออะไรก็ได้ เช่น `DebuffIconPrefab`

ใน prefab ควรมีอย่างน้อย:
- `Image`
- `DebuffSpriteIconHoverHandler`

แค่นี้พอ

จากนั้นลาก prefab นี้ไปใส่ในช่อง `Debuff Icon Prefab`

<<<<<<< ours
ในโค้ดล่าสุดของโปรเจกต์มีการต่อไว้แล้ว:
- Ice event เรียก `ApplyIceDebuff()`
- Burn event เรียก `ApplyBurnDebuff(3)`
- PlayerState เก็บลำดับการติด debuff เพื่อใช้เรียง icon
=======
> ถ้าไม่ใส่ prefab ระบบจะสร้าง `Image` ธรรมดาให้อัตโนมัติ
> ดังนั้นเริ่มจาก “ไม่ใช้ prefab” ก่อนก็ได้ ง่ายสุด
>>>>>>> theirs

---

## Step 7) วิธีทำงานตอน runtime

เมื่อเข้าเกมแล้ว:

1. ถ้าผู้เล่นติด Burn
   - ระบบจะเพิ่ม Burn เข้า list
   - ใส่ sprite Burn
2. ถ้าผู้เล่นติด Ice
   - ระบบจะเพิ่ม Ice เข้า list
   - ใส่ sprite Ice
3. ระบบจะ sort ตามลำดับการติด
4. icon จะถูกสร้าง/เปิดใต้ `DebuffIconContainer`
5. เมื่อเอาเมาส์ชี้ icon
   - tooltip จะเปิด
   - ข้อความจะเปลี่ยนตาม debuff นั้น

---

## Step 8) วิธีเทสที่ง่ายที่สุด

1. เข้า Play Mode
<<<<<<< ours
2. ทำให้ผู้เล่นติด Burn ก่อน แล้วติด Ice ตาม
3. ถ้าใช้ Sprite mode:
   - เช็กว่า icon Burn กับ Ice โผล่ตาม sprite ที่ผูกไว้
   - เช็กว่าเรียงซ้าย → ขวาตามลำดับที่ติด
4. ถ้าใช้ Text fallback:
   - เช็กว่า icon เรียงเป็น `🔥  ❄️`
5. เอาเมาส์ชี้ที่ Burn icon ต้องเห็นข้อความ Burn + เทิร์นคงเหลือ
6. เอาเมาส์ชี้ที่ Ice icon ต้องเห็นข้อความ Ice + คงเหลือ 1 ครั้ง
7. รอ Burn หมด แล้วเช็กว่า Ice เลื่อนมาอยู่ซ้าย (แทนที่ตำแหน่งเดิม)
=======
2. ทำให้ผู้เล่นติด Burn ก่อน
3. ดูว่า icon Burn ขึ้นไหม
4. เอาเมาส์ชี้ Burn icon
5. ดูว่า tooltip ขึ้นไหม
6. จากนั้นทำให้ผู้เล่นติด Ice เพิ่ม
7. ดูว่า icon Ice ขึ้นไหม
8. เช็กว่า icon เรียงตามลำดับที่ติด
9. รอ Burn หมด
10. เช็กว่า Ice ยังอยู่ และตำแหน่งขยับมาแทนได้ถูกต้อง
>>>>>>> theirs

---

# โครงสร้าง Hierarchy ที่แนะนำ (แบบง่าย)

<<<<<<< ours
### ไม่ขึ้น tooltip ตอน hover
- เช็ก `EventSystem` มีอยู่จริง
- ถ้าใช้ Text fallback ให้เช็ก `DebuffIconText` เปิด `Raycast Target`
- ถ้าใช้ Sprite mode ให้เช็กว่า icon object มี `Image` และรับ raycast ได้
- เช็ก Canvas/GraphicRaycaster ยังทำงาน
- เช็กว่า `Debuff Tooltip Root` และ `Debuff Tooltip Text` ถูกลากผูก

### เห็นแต่เครื่องหมาย `-`
- หมายถึงตอนนั้นยังไม่มี debuff ที่ active
- ข้อความ `-` จะแสดงเฉพาะใน Text fallback mode
- ถ้าใช้ Sprite mode แล้วไม่มี debuff ระบบจะซ่อน icon ทั้งหมดแทน

### icon ไม่ขึ้นใน Sprite mode
- เช็กว่าได้ลาก `Debuff Icon Container` แล้ว
- เช็กว่าได้ลาก `Burn Debuff Sprite` / `Ice Debuff Sprite` แล้ว
- ถ้าใช้ prefab เอง ให้เช็กว่ามี component `Image`

### icon ไม่เรียงตามลำดับที่ติด
- ตรวจว่า Ice ถูก apply ผ่าน `ApplyIceDebuff()`
- ตรวจว่า Burn ถูก apply ผ่าน `ApplyBurnDebuff(turns)`
- ตรวจว่าไม่มีจุดอื่นไปเซ็ต `hasIceEffect = true` ตรง ๆ
=======
```text
Canvas
├── PlayerStatusPanel
│   ├── HPText
│   ├── CreditText
│   ├── StarText
│   └── DebuffIconContainer
└── DebuffTooltip
    └── DebuffTooltipText
```

> KISS: ใช้แค่นี้พอสำหรับ sprite mode

---

# ถ้าไม่อยากใช้ sprite ตอนนี้

ยังใช้แบบเก่าได้ โดยสร้าง `Text - TextMeshPro` ชื่อ `DebuffIconText`
แล้วลากไปใส่ช่อง `Debuff Text`

แต่ถ้ามี `Debuff Icon Container` อยู่แล้ว ระบบจะเลือกใช้ **sprite mode ก่อน**
>>>>>>> theirs

---

# Troubleshooting แบบสั้น

## icon ไม่ขึ้น
เช็ก 4 อย่างนี้ก่อน:
1. ผูก `Debuff Icon Container` แล้วหรือยัง
2. ผูก `Burn Debuff Sprite` / `Ice Debuff Sprite` แล้วหรือยัง
3. debuff ถูก apply เข้า `PlayerState` จริงหรือยัง
4. object status panel ถูก active อยู่หรือไม่

## tooltip ไม่ขึ้น
เช็ก 4 อย่างนี้ก่อน:
1. มี `EventSystem`
2. ผูก `Debuff Tooltip Root`
3. ผูก `Debuff Tooltip Text`
4. icon มี `Image` และ raycast ใช้งานได้

## อยากเริ่มแบบง่ายที่สุด
ให้ใช้เซ็ตนี้ก่อน:
- `DebuffIconContainer`
- `DebuffTooltip`
- `DebuffTooltipText`
- Burn sprite
- Ice sprite
- ไม่ต้องใช้ prefab

---

<<<<<<< ours
- [ ] มี EventSystem ในฉาก
- [ ] มี DebuffTooltip + DebuffTooltipText
- [ ] ถ้าใช้ Sprite mode: มี DebuffIconContainer
- [ ] ถ้าใช้ Sprite mode: ผูก Burn Debuff Sprite / Ice Debuff Sprite ครบ
- [ ] ถ้าใช้ Text fallback: มี DebuffIconText และเปิด Raycast Target
- [ ] ผูกช่องใน PlayerUIController ครบ
- [ ] เทส Burn/Ice hover ผ่านใน Play Mode
=======
# Checklist ส่งงาน

- [ ] มี `EventSystem`
- [ ] มี `DebuffIconContainer`
- [ ] มี `DebuffTooltip`
- [ ] มี `DebuffTooltipText`
- [ ] ผูก Burn sprite แล้ว
- [ ] ผูก Ice sprite แล้ว
- [ ] ผูก reference ใน `PlayerUIController` ครบ
- [ ] เทส hover ผ่าน
- [ ] เทส Burn ก่อน Ice ผ่าน
>>>>>>> theirs
