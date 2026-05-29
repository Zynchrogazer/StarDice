# Board Enemy AI Decision System

เอกสารนี้สรุปการเพิ่ม AI ให้ enemy บน board scene โดยต่อยอดจากแนวคิด Utility AI ที่ battle scripts ใช้อยู่แล้ว ให้ enemy บน board เลือกทางแยกจากคะแนนและบุคลิกแทนการสุ่ม

## ภาพรวม

ระบบเดิมของ board AI มีจุดเริ่มต้นพร้อมใช้งานอยู่แล้ว เช่น AI สามารถถูกจัดเป็นผู้เล่นใน turn system และระบบเดินบน board รองรับการแยกทาง แต่จุดเลือกทางแยกยังเป็นการสุ่มจาก list ของ node ที่เดินได้

ระบบใหม่แบ่งหน้าที่ชัดเจนขึ้น:

- `PlayerPathWalker` รับผิดชอบการเดินตาม node และเรียก AI เมื่อต้องเลือกทางแยก
- `AIController` รับผิดชอบการตัดสินใจเลือกเส้นทางของ AI
- `RouteManager` เป็นแหล่งข้อมูล graph ของ board และชนิดของ tile

## Phase 1: เปลี่ยน AI จากสุ่มทางแยกเป็นเลือกจากคะแนน

### เป้าหมาย

แทนที่จะเลือก `choices[Random.Range(...)]` โดยตรง AI จะประเมินคะแนนของแต่ละทางแยกก่อน แล้วเลือกทางที่ได้คะแนนสูงสุด

### Logic หลัก

เมื่อ AI เจอทางแยก:

1. `PlayerPathWalker` ตรวจว่าผู้เล่นปัจจุบันเป็น AI หรือไม่
2. ถ้ามี `AIController` จะเรียก `AIController.ChoosePath(choices)`
3. `AIController` แปลง node ที่เลือกได้เป็น `tileID`
4. ใช้ `RouteManager.GetNodeData(tileID)` เพื่อดู `TileType`
5. คำนวณคะแนนรวมของแต่ละทางเลือก
6. เลือก node ที่มีคะแนนสูงสุด

### คะแนนพื้นฐานของ tile

| TileType | คะแนนพื้นฐาน | เหตุผล |
|---|---:|---|
| Heal | +25 | ฟื้นฟูและปลอดภัย |
| Star | +20 | เพิ่ม resource |
| Treasure | +18 | reward สูง |
| SpecialBoss | +20 | เป้าหมายใหญ่และ reward สูง |
| Boss | +15 | เสี่ยงแต่คุ้ม |
| Monster | +10 | มีโอกาสได้ reward จาก battle |
| Draw | +10 | ได้การ์ด/ตัวเลือกเพิ่ม |
| Shop | +8 | ใช้ resource เพื่อเสริมตัว |
| Event / Teleport / Minigame | +5 | มีโอกาสเกิดผลดี |
| Trap | -25 | อันตราย |
| Lava / iceeffect | -35 | อันตรายสูง |
| Normal / อื่น ๆ | 0 | ไม่มีผลพิเศษ |

### Health Situation Score

ถ้า AI เลือดต่ำกว่า 35%:

- Heal / Start ได้คะแนนเพิ่มมาก
- Trap / Lava / iceeffect / Monster / Boss / SpecialBoss ถูกลดคะแนนมาก

ส่วนนี้ทำให้ AI ไม่เดินชนความเสี่ยงตลอดเวลา และช่วยให้ดูเหมือน AI รู้จักเอาตัวรอด

## Phase 2: เพิ่ม AI Personality

### เป้าหมาย

enemy แต่ละตัวไม่ควรตัดสินใจเหมือนกันทั้งหมด จึงเพิ่ม personality ให้เลือกได้จาก Inspector ใน `AIController`

### Personality ที่มี

| Personality | พฤติกรรม |
|---|---|
| Balanced | ใช้คะแนนพื้นฐานและสถานการณ์ HP เป็นหลัก |
| Aggressive | ชอบ Monster / Boss / SpecialBoss และไม่ค่อยสนใจ Heal |
| Greedy | ชอบ Star / Treasure / Shop / Draw |
| Defensive | ชอบ Heal / Start และเลี่ยง tile อันตรายหรือ battle |
| Hunter | พยายามเลือกทางที่เข้าใกล้ผู้เล่นมนุษย์ โดยใช้ graph distance |

### วิธีใช้งานใน Unity Inspector

1. เลือก GameObject ของ enemy บน board
2. ตรวจว่า GameObject มี `PlayerState` และตั้ง `isAI = true`
3. เพิ่มหรือเลือก component `AIController`
4. ตั้งค่า `Personality`
5. เปิด `Log Decision Scores` ระหว่างทดสอบเพื่อดูคะแนนใน Console
6. ปรับ `Random Score Noise` ถ้าต้องการให้ AI ไม่เดินเหมือนเดิม 100% ทุกครั้ง

## Phase 3: Hunter AI และ Graph Search

### เป้าหมาย

Hunter personality ใช้แนวคิด graph search เพื่อหาเป้าหมายผู้เล่นมนุษย์ที่ใกล้ที่สุด

### วิธีคิด

board ถูกมองเป็น graph:

- Tile = node
- เส้นเชื่อมระหว่าง tile = edge
- `RouteManager.nodeConnections` = adjacency list

Hunter AI ใช้ Breadth-First Search (BFS) เพื่อคำนวณระยะจากทางเลือกของตัวเองไปยังผู้เล่นมนุษย์ แล้วให้คะแนนเพิ่มกับทางที่เข้าใกล้ผู้เล่นมากกว่า

สูตรแบบง่าย:

```text
HunterScore = clamp(40 - graphDistanceToPlayer * 6, 0, 40)
```

แปลว่า:

- ยิ่งใกล้ผู้เล่น คะแนนยิ่งสูง
- ถ้าไกลมาก คะแนนส่วน Hunter จะค่อย ๆ ลดลงเหลือ 0

## สูตร Utility รวม

ระบบเลือกทางแยกใช้สูตรรวมแนวนี้:

```text
TotalScore(path) =
    BaseTileScore(tileType)
  + HealthSituationScore(aiHp, tileType)
  + PersonalityModifier(personality, tileType)
  + HunterScore(path, targetPlayer)
  + RandomNoise
```

หมายเหตุ: `HunterScore` จะใช้เฉพาะ personality แบบ Hunter เท่านั้น

## วิธีทดสอบ

### Test Case 1: AI เลือก Heal ตอนเลือดน้อย

1. ตั้ง AI personality เป็น Balanced หรือ Defensive
2. ลด `PlayerHealth` ของ AI ให้ต่ำกว่า 35% ของ `MaxHealth`
3. สร้างทางแยกที่มี Heal และ Trap/Monster
4. AI ควรเลือก Heal หรือทางที่ปลอดภัยกว่า

### Test Case 2: Aggressive AI เลือก battle tile

1. ตั้ง AI personality เป็น Aggressive
2. สร้างทางแยกที่มี Monster/Boss กับ tile ปกติ
3. AI ควรให้คะแนน Monster/Boss สูงกว่า

### Test Case 3: Greedy AI เลือก resource tile

1. ตั้ง AI personality เป็น Greedy
2. สร้างทางแยกที่มี Star/Treasure/Shop กับ tile อื่น
3. AI ควรเลือก tile ที่ให้ resource มากกว่า

### Test Case 4: Hunter AI ไล่ผู้เล่น

1. ตั้ง AI personality เป็น Hunter
2. วางผู้เล่นมนุษย์ไว้บน board
3. สร้างทางแยกที่ทางหนึ่งเข้าใกล้ผู้เล่นกว่าอีกทาง
4. AI ควรเลือกทางที่ graph distance ไปหาผู้เล่นสั้นกว่า

## แนวทางต่อยอด

- เพิ่ม difficulty level เช่น Easy / Normal / Hard โดยปรับ `Random Score Noise` หรือโอกาสเลือกทางที่ไม่ใช่คะแนนสูงสุด
- เพิ่ม lookahead 2-3 ชั้น เพื่อให้ AI ไม่ดูแค่ tile ถัดไป แต่ดูเป้าหมายในอนาคตด้วย
- แยก score table เป็น ScriptableObject เพื่อจูนคะแนนจาก Inspector ได้โดยไม่ต้องแก้ code
- เพิ่ม UI debug บนหน้าจอเพื่อแสดงคะแนนที่ AI คิดระหว่าง demo
- ใช้ระบบเดียวกันกับ boss บน board เพื่อให้ boss มีพฤติกรรมเฉพาะตัว

## จุดขายด้าน Computer Science

หัวข้อนี้สามารถอธิบายเป็น Computer Science ได้หลายประเด็น:

- Utility AI: ให้คะแนน action/path แล้วเลือกคะแนนสูงสุด
- Graph Representation: board เป็น graph ผ่าน node และ edge
- Breadth-First Search: Hunter ใช้ BFS หา graph distance ไปหาผู้เล่น
- State-Based Decision Making: HP และ personality ส่งผลต่อการตัดสินใจ
- Separation of Concerns: แยก movement (`PlayerPathWalker`) ออกจาก decision making (`AIController`)
