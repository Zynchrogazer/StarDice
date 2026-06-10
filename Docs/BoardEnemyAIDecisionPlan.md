# Board Enemy AI Decision System

เอกสารนี้สรุประบบตัดสินใจของ enemy AI บน board scene โดยให้ AI เลือกทางแยกจากคะแนนและบุคลิก แทนการสุ่มจากทางเลือกที่เดินได้

## ภาพรวม

ระบบแบ่งหน้าที่ชัดเจนขึ้น:

- `PlayerPathWalker` รับผิดชอบการเดินตาม node และเรียก AI เมื่อต้องเลือกทางแยก
- `AIController` รับผิดชอบการตัดสินใจเลือกเส้นทางของ AI
- `RouteManager` เป็นแหล่งข้อมูล graph ของ board และชนิดของ tile

## Logic หลักตอนเลือกทางแยก

เมื่อ AI เจอทางแยก:

1. `PlayerPathWalker` ตรวจว่าผู้เล่นปัจจุบันเป็น AI หรือไม่
2. ถ้ามี `AIController` จะเรียก `AIController.ChoosePath(choices)`
3. ถ้ามีทางเดียว AI จะเดินทางนั้นเลยโดยไม่ต้องเลือก personality
4. ถ้ามีหลายทาง `AIController` จะเลือก personality สำหรับ decision นี้
5. แปลง node ที่เลือกได้เป็น `tileID` แล้วใช้ `RouteManager.GetNodeData(tileID)` เพื่อดู `TileType`
6. คำนวณคะแนนรวมของแต่ละทางเลือก
7. เลือก node ที่มีคะแนนสูงสุด

## Personality ที่ใช้จริง

ตอนนี้ AI เหลือ 2 personality เท่านั้น:

| Personality | บทบาท | เงื่อนไขที่ใช้ |
|---|---|---|
| Balanced | ค่าเริ่มต้น เป็นสายป่วนผู้เล่นด้วยการไปยึด/บังช่องรางวัลหรือ resource ที่ผู้เล่นอยากได้ เช่น Star, Treasure, Shop, Draw | ใช้เป็น default เมื่อไม่มีผู้เล่น HP ต่ำ หรือเมื่อปิด auto switch แล้วตั้ง Inspector เป็น Balanced |
| Hunter | สายไล่ล่าผู้เล่นเลือดน้อย เลือกทางที่ทำให้ AI ไปตกใกล้ผู้เล่นเป้าหมาย หรือทับจุดเดียวกับผู้เล่นให้มากที่สุด | ใช้เมื่อเปิด auto switch และมีผู้เล่นมนุษย์ HP น้อยกว่าหรือเท่ากับ `Hunter Health Threshold` ซึ่งค่าเริ่มต้นคือ 60% |

## Auto Personality Switch

`AIController` มีตัวเลือก `Auto Switch Personality`:

1. ถ้าปิด `Auto Switch Personality` จะใช้ personality ที่ตั้งไว้ใน Inspector ตลอด
2. ถ้าเปิด `Auto Switch Personality` ระบบจะเช็กผู้เล่นมนุษย์ทุกครั้งที่ AI ต้องเลือกทางแยก
3. ถ้ามีผู้เล่นมนุษย์ที่ `PlayerHealth / MaxHealth <= Hunter Health Threshold` จะเปลี่ยนเป็น `Hunter`
4. ถ้ามีผู้เล่นเลือดต่ำหลายคน จะเลือกคนที่เปอร์เซ็นต์ HP ต่ำที่สุดเป็นเป้าหมาย Hunter
5. ถ้าไม่มีผู้เล่นเลือดต่ำ จะกลับมาใช้ `Balanced` ซึ่งเป็นสาย default

> การเปลี่ยน personality เกิดตอน AI ต้องเลือกทางแยก ไม่ใช่ทุกครั้งที่เริ่มเทิร์น เพราะตอนเริ่มเทิร์น AI แค่ทอยเต๋า ส่วน decision จริงเกิดเมื่อ `PlayerPathWalker` ส่งรายการทางเลือกเข้ามาให้ `AIController.ChoosePath()`

## สูตรคะแนนรวม

```text
TotalScore(path) =
    BaseTileScore(tileType)
  + PlayerTargetScore(path, hunterTarget, personality)
  + PersonalityModifier(personality, tileType)
  + RandomNoise
```

### BaseTileScore

คะแนนพื้นฐานใช้กันทั้ง 2 personality เพื่อกัน AI เดินเข้าช่องที่ไม่ควรสนใจเกินไป:

| TileType | คะแนนพื้นฐาน | เหตุผล |
|---|---:|---|
| Normal / Start | +2 | ทางปลอดภัย ใช้เป็นทางผ่าน |
| Teleport / Event / Minigame | +4 | ทำให้ตำแหน่งหรือสถานการณ์บนบอร์ดวุ่นวายขึ้น |
| Trap / iceeffect | -6 | ไม่ใช่เป้าหมายหลักของ design ใหม่นี้ |
| Lava | -10 | อันตรายกว่า hazard ทั่วไป |
| Heal | -12 | Board AI ไม่เล่นเพื่อรักษาชีวิตตัวเอง |
| Monster / Boss / SpecialBoss | -18 | ไม่ควรเลือกเหมือนคู่แข่งที่ฟาร์ม battle/reward |

### Balanced Scoring

Balanced คือสาย default ที่เน้นป่วนผู้เล่นโดยไปยืนคุมช่องรางวัล/resource:

| TileType | Personality bonus | ความหมาย |
|---|---:|---|
| Star / Treasure | +28 | ช่องรางวัลหลักที่ผู้เล่นมักต้องการ จึงเหมาะกับการไปยึด/บัง |
| Shop / Draw | +22 | ช่อง resource ที่ช่วยผู้เล่น จึงเหมาะกับการกวน flow |
| Teleport | +10 | ช่วย reposition เพื่อป่วนต่อ |
| Event / Minigame | +8 | เพิ่มความวุ่นวายระดับกลาง |
| Heal | -10 | ไม่ใช่เป้าหมายของ AI |
| Trap / Lava / iceeffect | -6 | ไม่ใช่สายเน้น hazard แล้ว |

### Hunter Scoring

Hunter จะเปิดเมื่อมีผู้เล่นเลือดน้อยกว่าหรือเท่ากับ 60% และจะเลือกผู้เล่นที่เปอร์เซ็นต์ HP ต่ำที่สุดเป็น target:

- ถ้าทางเลือกทำให้ AI ไปตกจุดเดียวกับ target จะได้ `PlayerTargetScore` สูงสุด
- ถ้าทางเลือกเข้าใกล้ target มากขึ้น จะได้คะแนนตาม graph distance ยิ่งใกล้ยิ่งดี
- ถ้าทางเลือกไกลจาก target คะแนนส่วนนี้จะลดลงจนเป็น 0
- Hunter ลดความสำคัญของช่องรางวัล เช่น Star/Treasure/Shop/Draw เพื่อไม่ให้เสียจังหวะไล่ผู้เล่นเลือดน้อย

## วิธีทดสอบ

### Test Case 1: Balanced เป็น default

1. เปิด `Auto Switch Personality`
2. ตั้งผู้เล่นมนุษย์ทุกคนให้ HP มากกว่า 60%
3. สร้างทางแยกที่มี Star/Treasure/Shop/Draw กับทางปกติ
4. AI ควรใช้ `Balanced` และเลือกทางที่ไปคุมช่องรางวัล/resource มากกว่าเดินสุ่ม

### Test Case 2: Hunter เมื่อผู้เล่น HP ต่ำ

1. เปิด `Auto Switch Personality`
2. ลด HP ผู้เล่นมนุษย์ให้เหลือน้อยกว่าหรือเท่ากับ 60%
3. สร้างทางแยกที่ทางหนึ่งเข้าใกล้ผู้เล่นเลือดต่ำกว่าอีกทาง
4. AI ควรเปลี่ยนเป็น `Hunter` และเลือกทางที่ graph distance ไปหาผู้เล่นเลือดต่ำสั้นกว่า
5. ถ้ามีทางที่ลงช่องเดียวกับผู้เล่นเลือดต่ำ ทางนั้นควรได้คะแนนสูงสุด

### Test Case 3: ผู้เล่นหลายคนเลือดต่ำ

1. เปิด `Auto Switch Personality`
2. ทำให้ผู้เล่น A เหลือ 55% และผู้เล่น B เหลือ 30%
3. AI ควรเลือกผู้เล่น B เป็น Hunter target เพราะเปอร์เซ็นต์ HP ต่ำกว่า

### Test Case 4: ปิด Auto Switch

1. ปิด `Auto Switch Personality`
2. ตั้ง `Personality` ใน Inspector เป็น `Balanced` หรือ `Hunter`
3. AI ควรใช้ personality ที่ตั้งไว้ ไม่สลับเองจาก HP ผู้เล่น

## สรุปก่อน/หลัง

| หัวข้อ | ก่อนปรับ | หลังปรับ |
|---|---|---|
| จำนวน personality | 3 แบบ: Balanced, Saboteur, Hunter | 2 แบบ: Balanced, Hunter |
| ค่า default | Balanced แต่ยังมี roaming logic ไป Saboteur/Hunter | Balanced ชัดเจน เป็นสาย default |
| เงื่อนไข Hunter | ผู้เล่นอยู่ใกล้ / roaming สุ่ม | ผู้เล่นมนุษย์ HP <= 60% |
| เป้าหมาย Hunter | เข้าใกล้ผู้เล่นทั่วไป | เข้าใกล้หรือทับช่องผู้เล่นเลือดต่ำที่สุด |
| เป้าหมาย Balanced | คุมพื้นที่แบบกว้าง | ป่วนด้วยการไปยึด/บัง Star, Treasure, Shop, Draw |
| Saboteur | มีอยู่และเน้น hazard | ถูกถอดออกเพื่อให้ระบบอ่านง่ายขึ้น |

## แนวทางต่อยอด

- เพิ่ม difficulty level เช่น Easy / Normal / Hard โดยปรับ `Random Score Noise` หรือโอกาสเลือกทางที่ไม่ใช่คะแนนสูงสุด
- เพิ่ม lookahead 2-3 ชั้น เพื่อให้ AI ไม่ดูแค่ tile ถัดไป แต่ดูเป้าหมายในอนาคตด้วย
- แยก score table เป็น ScriptableObject เพื่อจูนคะแนนจาก Inspector ได้โดยไม่ต้องแก้ code
- เพิ่ม UI debug บนหน้าจอเพื่อแสดงคะแนนที่ AI คิดระหว่าง demo

## จุดขายด้าน Computer Science

- Utility AI: ให้คะแนน action/path แล้วเลือกคะแนนสูงสุด
- Graph Representation: board เป็น graph ผ่าน node และ edge
- Breadth-First Search: Hunter ใช้ BFS หา graph distance ไปหาผู้เล่นเลือดต่ำ
- State-Based Decision Making: HP ของผู้เล่นเป็นเงื่อนไขเปลี่ยน personality
- Separation of Concerns: แยก movement (`PlayerPathWalker`) ออกจาก decision making (`AIController`)
