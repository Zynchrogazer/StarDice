# พูดเจาะลึก Graph, BFS และ FSM แบบเข้าใจง่าย

เอกสารนี้ใช้เป็นบทพูดสำรองเวลาถูกอาจารย์ถามลึกขึ้น โดยยังยึดหลัก GameDev, SOLID และ KISS: อธิบายจากปัญหาในเกมก่อน แล้วค่อยโยงเป็นศัพท์ Computer Science

## Script ที่เกี่ยวข้อง

| Script | Path | หน้าที่ในหัวข้อนี้ |
| --- | --- | --- |
| `RouteManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs` | เก็บ `NodeConnection` และ `connectedNodes` ซึ่งเป็น graph ของบอร์ด |
| `PlayerPathWalker.cs` | `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerPathWalker.cs` | ตรวจทางแยกและส่งทางเลือกให้ AI ตอนเดิน |
| `AIController.cs` | `Assets/0StarDice0/Scripts/MainGame/NPC/AIController.cs` | เลือกทางด้วย heuristic score และ BFS-style look-ahead |
| `GameTurnManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameTurnManager.cs` | คุม FSM ของ turn flow ว่าตอนไหนรอทอย/เดิน/ประมวลผล event |

## 1. ภาพรวมที่ควรเปิดก่อน

> ในโปรเจกต์ StarDice ผม/หนูใช้แนวคิด Computer Science หลัก ๆ 3 ส่วนครับ/ค่ะ: Graph ใช้อธิบายโครงสร้างกระดาน, BFS ใช้ค้นหา/จำลองเส้นทางใน Graph และ FSM ใช้ควบคุมลำดับเทิร์นของเกมให้ไม่หลุด state

ให้พูดเป็นลำดับนี้:

1. **Graph = โครงสร้างกระดาน**
2. **BFS = วิธีเดินสำรวจ Graph**
3. **FSM = วิธีคุม flow ของเกม**

## 2. Graph พูดอย่างไรให้เข้าใจง่าย

### คำอธิบายสั้น

> Graph คือวิธีเก็บข้อมูลที่เหมาะกับบอร์ดที่มีทางแยก โดยช่องแต่ละช่องคือ Node และทางเดินระหว่างช่องคือ Edge ใน code ผม/หนูเก็บผ่าน `NodeConnection`: `node` คือช่องปัจจุบัน และ `connectedNodes` คือ list ของช่องที่เดินต่อได้

### Mapping กับ Code

| Graph term | ในเกม | ใน code |
| --- | --- | --- |
| Node | ช่องบนบอร์ด | `NodeConnection.node` |
| Edge | ทางเดินไปช่องถัดไป | สมาชิกใน `connectedNodes` |
| Adjacency List | list ว่าช่องนี้ไปไหนได้บ้าง | `List<Transform> connectedNodes` |
| Node data | ประเภทช่อง/ID/event | `tileID`, `type`, `eventName` |

### คำถามที่อาจโดนถาม

**ถาม:** ทำไมต้องใช้ Graph ไม่ใช้ array ธรรมดา?

**ตอบ:** เพราะบอร์ดมีทางแยกและอาจมี loop ถ้าใช้ array จะเหมาะกับเส้นตรง แต่ Graph ทำให้แต่ละช่องกำหนดช่องถัดไปได้หลายทางผ่าน `connectedNodes` และยังเพิ่ม/ลดทางแยกใน Inspector ได้ง่ายกว่า

**ถาม:** Graph นี้เป็น directed หรือ undirected?

**ตอบ:** ใน gameplay หลักมันทำงานเหมือน directed graph ตามที่เราใส่ `connectedNodes`: ถ้า A ใส่ B แปลว่าเดินจาก A ไป B ได้ ส่วน B จะเดินกลับ A ได้หรือไม่ขึ้นกับว่าเราใส่ A ใน connectedNodes ของ B ด้วยไหม

## 3. BFS พูดอย่างไรให้เข้าใจง่าย

### คำอธิบายสั้น

> BFS หรือ Breadth-First Search คือการสำรวจ Graph แบบเป็นชั้น ๆ เริ่มจาก node แรก แล้วดูเพื่อนบ้านระยะ 1 ก้าว จากนั้นค่อยดูระยะ 2 ก้าวต่อไป ในเกมนี้ใช้ Queue เพื่อจำลองว่า ถ้า AI เลือกทางนี้และยังเหลือก้าวเดิน จะไปเจอช่องอะไรในอนาคต

### Mapping กับ Code AI

| BFS concept | ใน code |
| --- | --- |
| Queue/frontier | `Queue<PathSearchStep> frontier` |
| State ที่กำลังสำรวจ | `PathSearchStep` มี `Node` และ `Depth` |
| Depth | จำนวนก้าวที่จำลองไปแล้ว |
| Visited | `HashSet<PathSearchStep> visitedSteps` |
| Stop condition | `remainingSteps`, `maxLookAheadNodes` |
| Evaluation | `EvaluatePathChoice` ให้คะแนนช่องปลายทาง |

### Script อธิบาย BFS ในโปรเจกต์

> ใน `EstimateFuturePathScore` ผม/หนูเริ่มจาก node ที่ AI กำลังพิจารณา แล้วใส่ลง Queue เป็น depth 0 จากนั้น dequeue ออกมาดูว่า ถ้ายังไม่ครบจำนวนก้าวที่ต้องมองล่วงหน้า ก็เอา connected node ถัดไปใส่ Queue ต่อ กระบวนการนี้ทำให้ AI มองเส้นทางเป็นชั้น ๆ ตามจำนวนก้าวจากลูกเต๋า และเมื่อถึง depth ที่กำหนด จะประเมินคะแนนช่องนั้นกลับมาเป็น future score

### ทำไมใช้ BFS-style ไม่ใช้ DFS?

**ตอบ:** เพราะเกมนี้สนใจ “จำนวนก้าวจากลูกเต๋า” ซึ่งเหมือนการไล่ระดับ depth ทีละชั้น BFS จึงอธิบายง่ายกว่า DFS และสัมพันธ์กับ gameplay มากกว่า

### ทำไมไม่ใช้ A*?

**ตอบ:** A* เหมาะกับการหา shortest path ไปเป้าหมายชัดเจนและมี cost/heuristic ระยะทาง แต่กรณีนี้ AI ไม่ได้ต้องไปเป้าหมายเดียวเสมอไป แค่ต้องเลือกทางแยกที่คะแนนดีที่สุดตาม tile type และก้าวที่เหลือ ดังนั้น BFS-style look-ahead + heuristic scoring เพียงพอและ KISS กว่า

### Complexity ตอบอย่างไร

ตอบแบบเข้าใจง่าย:

> ถ้าไม่จำกัด การค้นหาใน Graph ที่มีทางแยกอาจโตตามจำนวนทางเลือกในแต่ละ depth แต่ในเกมนี้ผม/หนูจำกัดสองชั้น คือ `maxLookAheadSteps` จำกัดความลึก และ `maxLookAheadNodes` จำกัดจำนวน node ที่สำรวจ ทำให้ runtime คุมได้

## 4. FSM พูดอย่างไรให้เข้าใจง่าย

### คำอธิบายสั้น

> FSM หรือ Finite State Machine คือการกำหนดว่าเกมตอนนี้อยู่สถานะไหน และอนุญาตให้ทำอะไรได้ในสถานะนั้น เช่น รอทอยเต๋า, กำลังทอย, กำลังเดิน, กำลังประมวลผล event หรือจบเทิร์น วิธีนี้ช่วยกัน bug เช่น กดทอยตอนกำลังเดิน หรือ trigger event ก่อนเดินจบ

### Mapping กับ Code

| FSM state | ความหมายในเกม |
| --- | --- |
| `Idle` | ยังไม่เริ่มหรือพัก state |
| `Preparing` | เตรียมข้อมูลผู้เล่นเทิร์นนี้ |
| `TurnAnnouncement` | UI ประกาศเทิร์น |
| `TurnGimmickProcessing` | ประมวลผล debuff/gimmick ก่อนทอย |
| `WaitingForRoll` | รอผู้เล่น/AI ทอยเต๋า |
| `Rolling` | กำลังทอยเต๋า |
| `Moving` | ตัวละครกำลังเดิน |
| `EventProcessing` | ลงช่องแล้วประมวลผล event |
| `Ending` | จบเทิร์นและส่งต่อคนถัดไป |

### Script อธิบาย FSM ในโปรเจกต์

> ใน `GameTurnManager` ผม/หนูใช้ enum `GameState` เป็น FSM ของ turn flow ทุกครั้งที่เปลี่ยนขั้นตอนจะเรียก `SetState` เช่น เริ่มเทิร์นเป็น `Preparing`, แสดง UI เป็น `TurnAnnouncement`, รอทอยเป็น `WaitingForRoll`, เมื่อทอยแล้วเปลี่ยนเป็น `Moving`, และเมื่อจบเทิร์นเปลี่ยนเป็น `Ending` วิธีนี้ทำให้ระบบอื่น เช่น UI หรือ event manager รู้ว่าเกมกำลังอยู่ขั้นไหน และลดโอกาสที่ action จะเกิดผิดเวลา

### คำถามที่อาจโดนถาม

**ถาม:** FSM ช่วยอะไรในเกมจริง?

**ตอบ:** ช่วยล็อก flow ของเกม เช่น `OnDiceRolled` รับผลทอยเฉพาะตอน `WaitingForRoll` หรือ `Rolling` เท่านั้น ถ้าเกมอยู่ state อื่นจะ return ออกไป ทำให้ทอยซ้ำตอนกำลังเดินไม่ได้

**ถาม:** ทำไมไม่ใช้ bool หลายตัวแทน state?

**ตอบ:** bool หลายตัวเสี่ยงขัดกัน เช่น `isMoving=true` แต่ `isRolling=true` พร้อมกันได้ FSM บังคับให้มี `currentState` หลักตัวเดียว จึง debug และอธิบายง่ายกว่า

## 5. คำตอบสั้นเมื่อโดนถามรวม Graph + BFS + FSM

> Graph คือโครงสร้างกระดาน, BFS คือวิธีสำรวจเส้นทางในกระดานเพื่อให้ AI มองอนาคต และ FSM คือระบบควบคุมลำดับเทิร์นให้เกมทำงานถูกช่วงเวลา ทั้งสามตัวช่วยให้เกมไม่ใช่แค่สุ่มหรือ hard-code แต่มี data structure, algorithm และ control flow ที่อธิบายเชิง Computer Science ได้

## 6. จุดที่ควรเปิด Code ให้ดู

1. `RouteManager.NodeConnection` — แสดง graph structure
2. `AIController.ChoosePath` — จุดตัดสินใจทางแยก
3. `AIController.EstimateFuturePathScore` — BFS-style look-ahead
4. `AIController.GetGraphDistance` — BFS หา distance ของ Hunter mode
5. `GameTurnManager.GameState` และ `SetState` — FSM ของ turn flow

## 7. จุดที่ควรระวังเวลาอธิบาย

- อย่าบอกว่าเป็น pathfinding เต็มรูปแบบแบบ A* เพราะจริง ๆ เป็น bounded look-ahead เพื่อเลือก branch
- อย่าบอกว่า BFS นี้หาเส้นทางที่ดีที่สุดทั้งเกม แต่ให้พูดว่า “จำลองเส้นทางในระยะจำกัดตามก้าวที่เหลือ”
- อย่าบอกว่า FSM กัน bug ได้ทั้งหมด ให้พูดว่า “ช่วยลด state bug และทำให้ flow ตรวจสอบง่ายขึ้น”
- ย้ำว่าใช้ KISS: ทำเท่าที่ gameplay ต้องการ ไม่เพิ่มระบบซับซ้อนเกินจำเป็น
