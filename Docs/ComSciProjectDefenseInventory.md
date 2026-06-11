# StarDice: Computer Science Inventory และแนวทางนำเสนอจบ ป.ตรี

## สรุปสั้น

ถ้ากังวลว่าองค์ประกอบ Computer Science ในโปรเจกต์ยังดูน้อยเกินไป แนวทางที่ปลอดภัยที่สุดคือ **ไม่ควรยัดระบบใหญ่ใหม่จนเสี่ยงพัง** แต่ควรจัดสิ่งที่มีอยู่ให้เป็นหัวข้อเชิงวิชาการ และเพิ่มจุดเล็ก ๆ ที่อธิบายได้ชัดเจน เช่น AI graph look-ahead, state machine, constrained randomization, sorting, scoring formula และ data persistence

ในมุม GameDev โปรเจกต์นี้มีแกน Com Sci เพียงพอสำหรับ ป.ตรี ถ้านำเสนอให้ถูกมุม: “เกม” เป็นผลลัพธ์ ส่วน “ระบบตัดสินใจและการจัดการข้อมูล” คือแกนวิทยาการคอมพิวเตอร์

## Inventory: ของที่เกี่ยวข้องกับ Computer Science ในโปรเจกต์

| หมวด | Script / ระบบ | แนวคิด Com Sci | วิธีพูดตอนนำเสนอ |
| --- | --- | --- | --- |
| Graph data structure | `RouteManager` / `NodeConnection` | บอร์ดเป็นกราฟ: node = ช่อง, edge = ทางเชื่อม | “ผม/หนูไม่ได้ hard-code การเดินเป็นเส้นตรง แต่เก็บบอร์ดเป็น graph ทำให้รองรับทางแยกและ event node ได้” |
| Graph search / AI | `AIController` | Bounded graph look-ahead + heuristic scoring | “AI จำลองทางเลือกตามจำนวนก้าวที่เหลือ แล้วให้คะแนนเส้นทางก่อนเลือก” |
| Shortest-path style search | `AIController.GetGraphDistance` | BFS ด้วย `Queue` และ `Dictionary` | “Hunter AI ประเมินระยะห่างจากเป้าหมายบนกราฟ ไม่ใช่สุ่มเดินอย่างเดียว” |
| State machine | `GameTurnManager` | Finite State Machine สำหรับ turn flow | “เกมแยกสถานะ Preparing, Rolling, Moving, EventProcessing เพื่อลด state bug” |
| Constrained randomization | `TileRandomizer` | Random shuffle + constraint validation + invariant checking | “สุ่มชนิดช่อง แต่คุมจำนวนขั้นต่ำ/สูงสุดเพื่อ balance gameplay” |
| Sorting algorithm | `CardSorter` / `CardComparer` | Merge sort + comparator | “ระบบ deck ใช้การเรียงข้อมูลตาม usable, rarity, name แยก comparator ชัดเจน” |
| Difficulty scaling | `QuickMathManager` | Procedural question generation + adaptive difficulty | “คำถามคณิตถูก generate ตาม difficulty ไม่ใช่คำถาม fixed” |
| Reward balancing | `MiniGameRewardService` | Clamp / scoring curve | “คะแนนถูก map เป็น reward ด้วยสูตรคุม min/max เพื่อ balance economy” |
| Damage formula | `BattleDamageFormula` | Formula abstraction | “แยกสูตร damage ออกจาก battle script เพื่อปรับ balance ได้ง่าย” |
| Data persistence / service layer | `PlayerProgressService` | Service layer, validation, fallback | “ข้อมูล player progress ถูกเข้าถึงผ่าน service ลด coupling ระหว่างระบบ UI/shop/gameplay” |

## Script Path Reference

| Script | Path | หมวดที่เกี่ยวข้อง |
| --- | --- | --- |
| `RouteManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs` | Graph data structure |
| `AIController.cs` | `Assets/0StarDice0/Scripts/MainGame/NPC/AIController.cs` | AI, BFS-style graph look-ahead, heuristic |
| `PlayerPathWalker.cs` | `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerPathWalker.cs` | Movement flow และส่ง `stepsRemaining` ให้ AI |
| `GameTurnManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameTurnManager.cs` | FSM / turn flow |
| `TileRandomizer.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/TileRandomizer.cs` | Constrained randomization |
| `CardSorter.cs` | `Assets/0StarDice0/Scripts/CodeInterMission/Deck/CardSorter.cs` | Merge sort |
| `CardComparer.cs` | `Assets/0StarDice0/Scripts/CodeInterMission/Deck/CardComparer.cs` | Comparator / sorting rule |
| `QuickMathManager.cs` | `Assets/0StarDice0/Scripts/MiniGame/CodeMath/QuickMathManager.cs` | Procedural question / adaptive difficulty |
| `MiniGameRewardService.cs` | `Assets/0StarDice0/Scripts/MiniGame/MiniGameRewardService.cs` | Reward formula / economy balancing |
| `BattleDamageFormula.cs` | `Assets/0StarDice0/Scripts/Test/TestFight/BattleDamageFormula.cs` | Damage formula abstraction |
| `PlayerProgressService.cs` | `Assets/0StarDice0/Player/PlayerProgressService.cs` | Service layer / persistence facade |

## ถ้าอาจารย์ถามว่า “Com Sci อยู่ตรงไหน?”

ตอบเป็น 3 ชั้นจะดูแข็งแรงที่สุด:

1. **Data Structure**: บอร์ดถูกแทนด้วย Graph และข้อมูลการ์ด/ผู้เล่นถูกจัดเก็บเป็น model
2. **Algorithm**: AI ใช้ graph search + heuristic, deck ใช้ sorting, tile randomizer ใช้ constrained randomization
3. **Software Engineering**: แยกระบบด้วย service/state machine เพื่อลด coupling และทำให้ maintain ได้

## แนวทางแนะนำแบบ KISS / ไม่เสี่ยงพัง

### ควรทำก่อนนำเสนอ

1. **เปิด log การตัดสินใจของ AI**
   - ใช้ `logDecisionScores = true`
   - ตอน demo ให้โชว์ Console ว่า AI ประเมิน `immediate`, `future`, `total`, `lookAhead`
   - ข้อดี: เห็น algorithm ทำงานจริงโดยไม่ต้องสร้าง UI ใหม่

2. **เตรียม slide อธิบาย Board เป็น Graph**
   - วาด node 3-5 จุดก็พอ
   - อธิบายว่า `connectedNodes` คือ adjacency list
   - ผูกกับ path decision ของ AI

3. **ทำตาราง Heuristic Score**
   - Star/Treasure = คะแนนดี
   - Trap/Lava/Monster = คะแนนลบ
   - Hunter mode = เพิ่มคะแนนถ้าเข้าใกล้ผู้เล่นเลือดน้อย

4. **ทำ before/after demo**
   - Before: AI เลือกทางจากช่องถัดไปเท่านั้น หรือสุ่ม fallback
   - After: AI มองจำนวนก้าวที่เหลือและประเมินเส้นทางอนาคต

### ควรทำถ้ามีเวลาเพิ่ม 1-2 วัน

1. **AI Debug Panel แบบเล็ก**
   - แสดงข้อความล่าสุดจาก AI decision log บน UI
   - ไม่ต้องทำ graph visualizer ใหญ่ ๆ

2. **Unit-like test scene สำหรับ AI path choice**
   - สร้าง board เล็ก ๆ 5-8 node
   - ตั้ง tile type แล้วดูว่า AI เลือกทางที่คะแนนสูงกว่า

3. **เอกสาร Complexity**
   - Graph look-ahead จำกัดด้วย `maxLookAheadSteps` และ `maxLookAheadNodes`
   - อธิบาย Big-O แบบง่าย: สำรวจตาม branching factor แต่มี budget จึงไม่หนักเครื่อง

### ยังไม่ควรทำถ้าใกล้ส่ง

- ไม่ควรเปลี่ยน battle system ใหญ่
- ไม่ควร refactor enemy script ทั้งหมดก่อนส่ง
- ไม่ควรเพิ่ม ML / neural network เพราะอธิบายยากและเสี่ยง bug
- ไม่ควรทำ pathfinding เต็มรูปแบบ A* ถ้าเกมไม่ได้ต้องการ shortest path จริง ๆ

## ประโยคแนะนำสำหรับพูดในเล่มหรือสไลด์

> โครงสร้างบอร์ดของ StarDice ถูกออกแบบเป็นกราฟ โดยแต่ละช่องเป็น node และทางเดินเป็น edge ทำให้รองรับทางแยกและ event ได้ยืดหยุ่น ส่วน AI ใช้ bounded graph look-ahead ร่วมกับ heuristic scoring เพื่อประเมินทางเลือกตามจำนวนก้าวที่เหลือจากลูกเต๋า จึงไม่ใช่การสุ่มอย่างเดียว แต่เป็นการตัดสินใจจากข้อมูลในเกม ทั้งนี้ระบบถูกจำกัด depth และ node budget เพื่อรักษาประสิทธิภาพตามหลัก KISS

## สรุปคำแนะนำ

ของที่มีอยู่ไม่ได้น้อยเกินไป แต่ยังต้อง “เล่าให้เป็น Com Sci” มากขึ้น โดยยก AI graph look-ahead เป็นหัวข้อหลัก แล้วใช้ state machine, constrained randomization, sorting, formula และ service layer เป็นหัวข้อสนับสนุน จะเหมาะกับโปรเจกต์จบ ป.ตรี มากกว่าการเพิ่มระบบใหญ่ที่เสี่ยงกระทบ flow เกม

## เอกสารเสริมสำหรับคำถามเรื่อง Graph

ถ้าต้องตอบคำถามเจาะเรื่อง Graph หรือ code decision ของ AI ให้ใช้ `Docs/GraphPresentationQAGuide.md` เป็นบทพูดหลัก เพราะแยกไว้เป็นคำถาม-คำตอบและชี้จุด code ที่ควรเปิดให้ดูระหว่างนำเสนอ

## เอกสารเจาะลึก BFS / FSM / Graph

สำหรับการตอบคำถามเชิง Computer Science แบบละเอียด ให้ใช้ `Docs/BfsFsmGraphDeepDiveGuide.md` เพื่ออธิบายว่า Graph คือโครงสร้างบอร์ด, BFS คือการสำรวจเส้นทาง และ FSM คือการคุมลำดับเทิร์น
