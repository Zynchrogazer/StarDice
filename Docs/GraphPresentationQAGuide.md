# แนวทางพูดเรื่อง Graph และตอบคำถาม Code

## Script ที่เกี่ยวข้อง

| Script | Path | หน้าที่ในหัวข้อนี้ |
| --- | --- | --- |
| `RouteManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs` | เก็บ `NodeConnection` และ `connectedNodes` ซึ่งเป็น graph ของบอร์ด |
| `PlayerPathWalker.cs` | `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerPathWalker.cs` | ตรวจทางแยกและส่งทางเลือกให้ AI ตอนเดิน |
| `AIController.cs` | `Assets/0StarDice0/Scripts/MainGame/NPC/AIController.cs` | เลือกทางด้วย heuristic score และ BFS-style look-ahead |
| `GameTurnManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameTurnManager.cs` | คุม FSM ของ turn flow ว่าตอนไหนรอทอย/เดิน/ประมวลผล event |

## พูดเรื่อง Graph อย่างไรให้เข้าใจง่าย

ให้เริ่มจากภาพรวมแบบ GameDev ก่อน ไม่ต้องเปิดด้วยศัพท์ยาก:

> กระดานของเกมไม่ได้เป็นเส้นตรงอย่างเดียว แต่มีทางแยกและช่องหลายประเภท ผม/หนูจึงมองกระดานเป็น Graph โดยช่องแต่ละช่องคือ Node และทางเดินระหว่างช่องคือ Edge เมื่อผู้เล่นหรือ AI อยู่ที่ช่องหนึ่ง ระบบจะดูว่าช่องนี้เชื่อมไปช่องไหนได้บ้างจาก `connectedNodes`

จากนั้นค่อยเชื่อมกับ Computer Science:

1. **Node** = ช่องบนบอร์ด เช่น Start, Star, Trap, Monster
2. **Edge** = เส้นทางที่เดินไปได้ระหว่างช่อง
3. **Adjacency List** = `connectedNodes` ใน `NodeConnection`
4. **Graph Search** = AI ใช้ `Queue` สำรวจเส้นทางล่วงหน้าตามจำนวนก้าวที่เหลือ
5. **Heuristic** = AI ให้คะแนนช่อง เช่น Star/Treasure ดี, Trap/Lava แย่

## Script พูดนำเสนอ 45-60 วินาที

> ส่วนที่เป็น Computer Science หลักของเกมคือระบบกระดานที่ออกแบบเป็น Graph ครับ/ค่ะ ใน code แต่ละช่องถูกเก็บเป็น `NodeConnection` ซึ่งมี `node` เป็นตำแหน่งของช่อง และ `connectedNodes` เป็น list ของช่องถัดไปที่เดินได้ โครงสร้างนี้ทำให้กระดานรองรับทางแยกได้โดยไม่ต้อง hard-code ทุกเส้นทาง
>
> ตอน AI เจอทางแยก `PlayerPathWalker` จะส่งทางเลือกและจำนวนก้าวที่เหลือไปให้ `AIController` จากนั้น AI จะให้คะแนนช่องถัดไป และใช้ bounded graph look-ahead สำรวจเส้นทางอนาคตด้วย Queue คล้าย BFS โดยจำกัดทั้งจำนวนก้าวและจำนวน node เพื่อไม่ให้หนักเครื่อง สุดท้าย AI เลือกทางที่คะแนนรวมดีที่สุด ซึ่งเป็นการตัดสินใจจากข้อมูลในเกม ไม่ใช่สุ่มอย่างเดียว

## ถ้าอาจารย์ถามถึง Code ควรชี้ตรงไหน

### 1. Graph เก็บอยู่ตรงไหน?

ตอบว่าอยู่ที่ `RouteManager` และ `NodeConnection`:

- `node` คือ Transform ของช่อง
- `connectedNodes` คือ adjacency list ของช่องที่เดินต่อได้
- `tileID` และ `type` คือข้อมูล gameplay ของ node นั้น

### 2. AI ใช้ Graph ตอนไหน?

ตอบว่าใช้ตอนเจอทางแยก:

- `PlayerPathWalker` เช็คว่ามี `choices.Count > 1`
- ถ้าเป็น AI จะเรียก `AIController.ChoosePath(choices, stepsRemaining)`
- AI ประเมินทุก choice แล้วเลือกคะแนนดีที่สุด

### 3. BFS / Queue อยู่ตรงไหน?

ตอบว่ามี 2 จุด:

- `EstimateFuturePathScore` ใช้ `Queue<PathSearchStep>` เพื่อมองเส้นทางล่วงหน้าตามก้าวที่เหลือ
- `GetGraphDistance` ใช้ `Queue<Transform>` และ `Dictionary<Transform, int>` เพื่อหาระยะห่างจากเป้าหมายใน Hunter mode

### 4. ทำไมไม่ใช้ A*?

ตอบแบบ KISS:

> เกมนี้ไม่ได้ต้องการหา shortest path แบบมีน้ำหนักหรือมี obstacle ซับซ้อน ทุก edge บนบอร์ดนับเป็น 1 ก้าวเท่ากัน และ AI ต้องแค่เลือกทางแยกตามจำนวนก้าวจากลูกเต๋า ดังนั้น BFS-style look-ahead + heuristic scoring เพียงพอ เข้าใจง่าย และเสี่ยง bug น้อยกว่า A*

### 5. ทำไมต้องจำกัด `maxLookAheadSteps` และ `maxLookAheadNodes`?

ตอบว่าเพราะบอร์ดอาจมี loop:

> ถ้า graph มีทางวน การค้นหาโดยไม่จำกัดอาจสำรวจซ้ำมากเกินไป จึงจำกัด depth ตามก้าวที่เหลือ และจำกัดจำนวน node เพื่อคุม performance ใน runtime

## ส่วนของ Code ที่ยากที่สุดหากถูกถาม

ส่วนที่น่าจะถูกถามยากที่สุดคือ **AI graph look-ahead** ใน `AIController` เพราะต้องอธิบายพร้อมกัน 4 เรื่อง:

1. โครงสร้าง graph ของบอร์ด
2. การใช้ Queue สำรวจ node ตาม depth
3. การให้คะแนนด้วย heuristic
4. การคุม performance ด้วย depth/node budget

รองลงมาคือ `GetGraphDistance` เพราะใช้ BFS เพื่อหา distance ระหว่าง AI กับผู้เล่นเป้าหมายใน Hunter mode

## จุดที่ควรเตรียมตอบให้ได้

| คำถาม | คำตอบสั้น |
| --- | --- |
| ทำไม Graph เหมาะกับเกมนี้? | เพราะบอร์ดมีทางแยก จึงแทนช่องเป็น node และทางเดินเป็น edge ได้ตรงกับ gameplay |
| AI ฉลาดขึ้นอย่างไร? | มองก้าวที่เหลือและประเมินช่องอนาคต ไม่เลือกจากช่องถัดไปอย่างเดียว |
| Heuristic คืออะไร? | กฎให้คะแนนโดยประมาณ เช่น Star ดี Trap แย่ Hunter เข้าใกล้ผู้เล่นเลือดน้อย |
| Complexity เป็นอย่างไร? | จำกัดด้วย `maxLookAheadSteps` และ `maxLookAheadNodes` จึงไม่ปล่อยให้ค้นหาทั้ง graph |
| ทำไมไม่ทำซับซ้อนกว่านี้? | ตาม KISS เพราะเป้าหมายคือทำให้ AI ตัดสินใจดีขึ้นโดยไม่เสี่ยงกระทบ flow เดินเดิม |

## ถ้าต้องปรับเพิ่มแบบไม่เสี่ยง

1. เปิด `logDecisionScores` แล้วโชว์ Console ตอน demo
2. วาดรูป graph เล็ก ๆ ใน slide และ map กับ `NodeConnection.connectedNodes`
3. ทำตาราง heuristic score ของ tile type
4. ทำ test scene เล็ก ๆ 5-8 node เพื่อโชว์ว่า AI เลือก Star มากกว่า Trap
5. ถ้ามีเวลาค่อยเพิ่ม UI debug panel แสดง choice/score ล่าสุด แต่ไม่จำเป็นต้องทำถ้าใกล้ส่ง

## ถ้าอาจารย์ให้แก้ Code ให้ดู

ให้เริ่มจากจุดที่ปลอดภัยก่อน เช่น ปรับคะแนน heuristic, ปรับ `maxLookAheadSteps`, เปิด `logDecisionScores`, หรือเปลี่ยน `hunterHealthThreshold` เพราะเป็นการแก้ parameter/score ที่ไม่กระทบ flow เดินหลัก รายละเอียด checklist อยู่ใน `Docs/AdvisorCodeEditTestGuide.md`

## เอกสารเจาะลึก BFS / FSM / Graph

ถ้าต้องตอบลึกกว่าคำถามพื้นฐาน ให้ใช้ `Docs/BfsFsmGraphDeepDiveGuide.md` เป็นบทพูดเสริม เพราะแยก Graph, BFS และ FSM พร้อม mapping กับ code และคำถามที่อาจโดนถาม
