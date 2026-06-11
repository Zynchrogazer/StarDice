# จุดที่อาจารย์อาจให้ทดสอบด้วยการแก้ Code หน้างาน

เอกสารนี้สรุปจุดใหญ่ที่อาจารย์อาจให้ลองแก้ code เพื่อพิสูจน์ว่าเข้าใจระบบจริง โดยเรียงจากปลอดภัยที่สุดไปเสี่ยงมากขึ้นตามมุม GameDev / KISS

## Script ที่เกี่ยวข้องกับการแก้ Code หน้างาน

| Script | Path | จุดที่น่าถูกให้แก้ |
| --- | --- | --- |
| `AIController.cs` | `Assets/0StarDice0/Scripts/MainGame/NPC/AIController.cs` | ปรับ heuristic, look-ahead, debug log, personality |
| `RouteManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs` | ปรับ `nodeConnections` / graph connection ใน Inspector |
| `PlayerPathWalker.cs` | `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerPathWalker.cs` | อธิบาย flow เดินและจุดเรียก AI แต่ไม่ควรแก้สดถ้าไม่จำเป็น |
| `GameTurnManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameTurnManager.cs` | อธิบาย FSM และ state transition ของเทิร์น |

## 1. ปรับ Heuristic Score ของ AI

**จุดที่อาจให้แก้:** `AIController.GetBaseTileScore` หรือ `AIController.GetPersonalityModifier`

**ตัวอย่างโจทย์:**
- “ถ้าอยากให้ AI ชอบช่อง Heal มากขึ้นต้องแก้ตรงไหน?”
- “ถ้าอยากให้ Hunter ไม่หลบ Star/Treasure ต้องแก้ยังไง?”

**แนวตอบ:**
- แก้ค่าคะแนนของ `TileType` ใน switch
- ค่าบวก = AI อยากไป
- ค่าลบ = AI หลีกเลี่ยง

**เหตุผลที่เหมาะให้ demo:** เปลี่ยนตัวเลขเล็ก ๆ เห็นผลใน decision log ทันที และไม่กระทบ flow เดิน

## 2. ปรับระยะมองล่วงหน้าของ Graph Look-Ahead

**จุดที่อาจให้แก้:** serialized fields ใน `AIController`

- `maxLookAheadSteps`
- `futurePathScoreWeight`
- `maxLookAheadNodes`

**ตัวอย่างโจทย์:**
- “ถ้าอยากให้ AI มองไกลขึ้นต้องแก้ตรงไหน?”
- “ถ้าอยากลดภาระ runtime ต้องแก้ตรงไหน?”

**แนวตอบ:**
- เพิ่ม `maxLookAheadSteps` = มองจำนวนก้าวล่วงหน้ามากขึ้น
- ลด `futurePathScoreWeight` = ให้ความสำคัญกับช่องถัดไปมากกว่าอนาคต
- ลด `maxLookAheadNodes` = จำกัดจำนวน node ที่ค้นหาเพื่อ performance

## 3. เปิด/ปิด Debug Log เพื่ออธิบาย AI Decision

**จุดที่อาจให้แก้:** `logDecisionScores` ใน `AIController`

**ตัวอย่างโจทย์:**
- “พิสูจน์ได้อย่างไรว่า AI ไม่ได้สุ่ม?”

**แนวตอบ:**
- เปิด `logDecisionScores`
- Console จะแสดง `immediate`, `future`, `total`, `lookAhead`
- ใช้ log นี้อธิบายว่า AI เลือกทางที่คะแนนรวมดีที่สุด

## 4. ปรับบุคลิก AI

**จุดที่อาจให้แก้:** `BoardAIPersonality` และ `autoSwitchPersonality`

**ตัวอย่างโจทย์:**
- “ถ้าอยากให้ AI ไล่ผู้เล่นเลือดน้อยตลอดต้องทำยังไง?”

**แนวตอบ:**
- ตั้ง `personality = Hunter`
- ปิด `autoSwitchPersonality` ถ้าต้องการบังคับบุคลิกเอง
- หรือปรับ `hunterHealthThreshold` เพื่อกำหนดจังหวะเปลี่ยนเป็น Hunter

## 5. ปรับ Graph Connection ใน Scene

**จุดที่อาจให้แก้:** `RouteManager.nodeConnections` ใน Inspector

**ตัวอย่างโจทย์:**
- “ถ้าเพิ่มทางแยกใหม่ ต้องแก้อะไร?”

**แนวตอบ:**
- เพิ่ม Transform ของช่องปลายทางใน `connectedNodes`
- ไม่ต้องแก้ `PlayerPathWalker` เพราะ walker อ่านทางเลือกจาก graph อยู่แล้ว
- ถ้าเป็น AI จะส่ง choices เข้า `AIController.ChoosePath`

## 6. จุดที่ควรหลีกเลี่ยงถ้าโดนให้แก้สด

- อย่า refactor battle system ใหญ่ระหว่างนำเสนอ
- อย่าเปลี่ยน coroutine เดินใน `PlayerPathWalker` ถ้าไม่จำเป็น
- อย่าเพิ่ม algorithm ใหม่อย่าง A* หน้างาน เพราะไม่ได้จำเป็นกับ edge น้ำหนักเท่ากันของบอร์ด
- อย่าแก้ save/load หรือ scene flow สด ถ้าไม่ได้เตรียม test case ไว้

## จุดที่ยากที่สุดถ้าอาจารย์ถามลึก

จุดที่ยากสุดคือ `EstimateFuturePathScore` เพราะต้องอธิบายว่า:

1. ใช้ `Queue` สำรวจ node แบบ BFS-style
2. ใช้ `Depth` แทนจำนวนก้าวที่จำลองไปแล้ว
3. ใช้ `visitedSteps` กันการสำรวจ state ซ้ำในกราฟที่มี loop
4. จำกัดด้วย `maxLookAheadNodes` เพื่อไม่ให้ runtime หนัก
5. คืนคะแนนอนาคตที่ดีที่สุดกลับไปผสมกับคะแนนช่องถัดไป

ถ้าถูกถาม ให้ตอบว่า “ส่วนนี้ไม่ได้เปลี่ยนการเดินจริงของเกม แต่เป็นการจำลองทางเลือกเพื่อเลือก branch เท่านั้น”
