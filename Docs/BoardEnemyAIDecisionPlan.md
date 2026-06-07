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

### คะแนนพื้นฐานของ tile (เวอร์ชัน Board Nuisance AI)

> บทบาทของ AI บน board คือ “ป่วนผู้เล่น” ไม่ใช่แข่งเก็บดาว/รางวัลกับผู้เล่น แต่ช่องอันตรายอย่าง Trap / Lava / iceeffect ไม่ควรได้คะแนนสูงแบบไม่มีเงื่อนไข เพราะถ้า AI เดินเหยียบเฉย ๆ โดยไม่ได้กดดันผู้เล่น จะดูเหมือน AI ฆ่าตัวเองหรือเดินมั่ว ดังนั้นคะแนนพื้นฐานของ hazard จะติดลบเล็กน้อย แล้วค่อยได้คะแนนเพิ่มจาก `HazardPressureScore` เฉพาะตอนที่ใช้กดดัน/บังทางผู้เล่นได้จริง

| TileType | คะแนนพื้นฐาน | เหตุผล |
|---|---:|---|
| Teleport | +18 | ทำให้ตำแหน่ง AI เปลี่ยนและเข้าป่วนผู้เล่นได้คาดเดายาก |
| Event / Minigame | +10 | เพิ่มความวุ่นวายบนเส้นทาง แต่ไม่ใช่เป้าหมายหลักในการสะสมแต้ม |
| Normal / Start | +2 ถึง +4 | ปลอดภัย ใช้เป็นทางผ่านหรือรอจังหวะป่วน |
| Heal | -12 | AI แบบซอมบี้ไม่ใช้ HP เป็นเป้าหมาย จึงไม่ต้องวิ่งหา Heal |
| Trap / iceeffect | -6 | ไม่ควรเลือกเพราะเป็นช่องอันตรายโดยตัวมันเอง จะบวกเพิ่มเฉพาะเมื่อกดดันผู้เล่นใกล้ ๆ ได้ |
| Lava | -10 | อันตรายกว่า hazard ทั่วไป จึงไม่ควรเป็นทางเลือกหลักถ้าไม่ได้ช่วยไล่/บังผู้เล่น |
| Draw / Shop | -10 | ลดความสำคัญของ resource ส่วนตัว |
| Star / Treasure | -14 | ไม่ควรไล่เก็บรางวัลแข่งผู้เล่น |
| Monster / Boss / SpecialBoss | -18 | ไม่ควรเล่นเหมือนคู่แข่งที่ฟาร์ม battle/reward |

### Hazard Pressure Score

ส่วนนี้คือคำตอบว่าทำไมก่อนหน้านี้ถึงมีแนวคิดให้ hazard มีคะแนน: ไม่ใช่เพราะ Trap / Lava / iceeffect “ดี” สำหรับ AI แต่เพราะ hazard อาจมีประโยชน์เชิงป่วนเมื่ออยู่ใกล้ผู้เล่น เช่น บังทาง, ไล่ให้ผู้เล่นต้องเปลี่ยนเส้นทาง, หรือบังคับจังหวะปะทะ

กติกาใหม่คือ:

- Trap / Lava / iceeffect จะไม่ได้คะแนนสูงจาก base score
- จะได้คะแนน `HazardPressureScore` เฉพาะถ้า tile นั้นอยู่ใกล้ผู้เล่นในระยะประมาณ `Ambush Distance + 1`
- `Aggressive` ได้ bonus เพิ่มเล็กน้อยเมื่อใช้ hazard กดดันผู้เล่น
- `Hunter` ได้ bonus เล็กน้อยเพราะยังเน้นเข้าใกล้ผู้เล่นมากกว่ายืนบน hazard
- ถ้า hazard อยู่ไกลผู้เล่น คะแนนส่วนนี้เป็น 0

### Zombie / No-HP Board AI

AI บนบอร์ดถูกออกแบบให้เหมือน “ซอมบี้ตัวป่วน” คือยังใช้ `PlayerState` เพื่อเข้ากับระบบเดิม แต่ในเชิง gameplay จะไม่ตัดสินใจจาก HP แล้ว:

- ไม่มี `Low Health Ratio` ใน `AIController`
- ไม่มี `SurvivalSituationScore` ในสูตรเลือกทาง
- HP ต่ำจะไม่บังคับให้เปลี่ยนเป็น `Defensive` และไม่ทำให้ AI วิ่งหา Heal
- ถ้า AI โดน damage / heal ผ่าน `PlayerState.TakeDamage` หรือ `PlayerState.Heal` ระบบจะรีเซ็ต HP กลับเป็นค่า safe เพื่อไม่ให้ตายหรือหลุด turn
- ตอนเริ่มเทิร์น `GameTurnManager` จะเรียก `EnsureBoardAIAlive()` ให้ AI ก่อนเช็ค HP เพื่อกันกรณีค่า HP ถูกแก้ตรง ๆ จาก Inspector หรือระบบอื่น
- AI จึงคอยวิ่งป่วน, ไล่ผู้เล่น, ยึดพื้นที่, หรือใช้ hazard pressure ตามสถานการณ์ แทนการเล่นแบบรักษาชีวิต

### Player Disruption Score

ทุก personality จะดูระยะจากทางเลือกไปยังผู้เล่นมนุษย์ด้วย graph distance:

- ถ้าทางเลือกไปลงช่องเดียวกับผู้เล่น จะได้คะแนนสูงมาก เพราะ `BoardManager.CheckForBattle` จะพาเข้าการปะทะได้
- ถ้าเข้าใกล้ผู้เล่น คะแนนจะเพิ่มตามระยะ ยิ่งใกล้ยิ่งดี
- Hunter จะได้เพดานคะแนนส่วนนี้สูงกว่า personality อื่น เพื่อใช้เป็นโหมดไล่ป่วนผู้เล่นโดยตรง

## Phase 2: เพิ่ม AI Personality

### เป้าหมาย

enemy แต่ละตัวไม่ควรตัดสินใจเหมือนกันทั้งหมด จึงเพิ่ม personality ให้เลือกได้จาก Inspector ใน `AIController`

### Personality ที่มี

| Personality | พฤติกรรมในบทบาทตัวป่วน |
|---|---|
| Balanced | เดินแบบกึ่งสุ่มแต่ยังชอบเส้นทางที่ทำให้บอร์ดวุ่นวายและไม่เน้นรางวัล |
| Aggressive | ยอมใช้ช่องอันตรายเพื่อสร้างแรงกดดันเมื่ออยู่ใกล้ผู้เล่น แต่ไม่เดินเข้า hazard ไกล ๆ แบบไม่มีเหตุผล |
| Greedy | ไม่ได้เก็บแต้มแข่งโดยตรง แต่ชอบไปยึด/บังเส้นทางที่เป็น reward เช่น Star, Treasure, Shop, Draw |
| Defensive | บุคลิกถอยจังหวะ/รีโพสิชัน เช่น Teleport หรือทางปลอดภัย ไม่เกี่ยวกับ HP ต่ำ |
| Hunter | ไล่เข้าใกล้ผู้เล่นมนุษย์และพยายามลงช่องเดียวกันเพื่อบังคับให้เกิดการปะทะ |

### Auto Personality Switch

`AIController` มีตัวเลือก `Auto Switch Personality` เพื่อเปลี่ยนบุคลิกตามสถานการณ์:

1. ถ้าผู้เล่นอยู่ในระยะ `Ambush Distance` จะบังคับเป็น `Hunter`
2. ถ้าไม่มีผู้เล่นใกล้ ๆ จะสุ่ม/เลือกบุคลิก roaming ทุก ๆ `Personality Switch Min/Max Decisions`
3. ถ้าทางแยกมีช่องป่วน จะเอนเอียงไป `Aggressive`; ถ้ามีช่อง reward จะเอนเอียงไป `Greedy` เพื่อไปยึดพื้นที่
4. HP ไม่ใช่เงื่อนไขในการสลับบุคลิกแล้ว เพราะ AI เป็น zombie nuisance ที่ไม่มีวันตายบนบอร์ด

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

AI ใช้ Breadth-First Search (BFS) เพื่อคำนวณระยะจากทางเลือกของตัวเองไปยังผู้เล่นมนุษย์ แล้วให้คะแนนเพิ่มกับทางที่เข้าใกล้ผู้เล่นมากกว่า โดย Hunter จะให้น้ำหนักส่วนนี้สูงกว่า personality อื่น

สูตรแบบง่าย:

```text
PlayerDisruptionScore = clamp(maxScoreByPersonality - graphDistanceToPlayer * falloffByPersonality, 0, maxScoreByPersonality)
```

แปลว่า:

- ยิ่งใกล้ผู้เล่น คะแนนยิ่งสูง
- ถ้าทางเลือกคือช่องเดียวกับผู้เล่น จะได้คะแนนพิเศษสูงมากเพื่อบังคับจังหวะปะทะ
- ถ้าไกลมาก คะแนนส่วนการไล่ผู้เล่นจะค่อย ๆ ลดลงเหลือ 0

## สูตร Utility รวม

ระบบเลือกทางแยกใช้สูตรรวมแนวนี้:

```text
TotalScore(path) =
    NuisanceTileScore(tileType)
  + PlayerDisruptionScore(path, targetPlayer, personality)
  + HazardPressureScore(path, targetPlayer, personality)
  + PersonalityModifier(personality, tileType)
  + RandomNoise
```

หมายเหตุ: `PlayerDisruptionScore` ใช้กับทุก personality แต่ Hunter จะให้ค่าน้ำหนักสูงกว่าเพื่อเน้นไล่ผู้เล่น ส่วน `HazardPressureScore` ใช้เฉพาะ Trap / Lava / iceeffect ที่อยู่ใกล้ผู้เล่นเท่านั้น

## วิธีทดสอบ

### Test Case 1: AI ไม่ตายและไม่วิ่งหา Heal เมื่อ HP ต่ำ

1. เปิด `Auto Switch Personality`
2. ลด `PlayerHealth` ของ AI ให้ต่ำมาก หรือเรียก `TakeDamage()` ใส่ AI
3. AI ควรรีเซ็ต HP กลับค่า safe ผ่าน zombie mode และไม่ trigger defeat flow
4. สร้างทางแยกที่มี Heal กับทางเข้าใกล้ผู้เล่น
5. AI ควรยังเลือกทางป่วน/เข้าใกล้ผู้เล่นมากกว่า Heal เพราะ HP ไม่ใช่เป้าหมายแล้ว

### Test Case 2: Aggressive AI ใช้ hazard เฉพาะเมื่อกดดันผู้เล่นได้

1. ตั้ง AI personality เป็น `Aggressive` หรือเปิด auto switch แล้วสร้างทางแยกที่มีช่องป่วน
2. สร้างทางแยกที่มี Trap/Lava/iceeffect อยู่ไกลผู้เล่น และอีกทางเป็น tile ปกติ/Teleport
3. AI ไม่ควรเลือก hazard ไกล ๆ เพียงเพราะเป็น hazard
4. ย้ายผู้เล่นให้มาใกล้ hazard ในระยะ `Ambush Distance + 1` แล้วทดสอบใหม่
5. AI ควรให้คะแนน hazard สูงขึ้น เพราะตอนนี้ใช้กดดันหรือบังเส้นทางผู้เล่นได้จริง

### Test Case 3: Greedy AI ยึดพื้นที่ reward แทนการแข่งเก็บแต้ม

1. ตั้ง AI personality เป็น `Greedy`
2. สร้างทางแยกที่มี Star/Treasure/Shop/Draw กับ tile อื่น
3. AI สามารถเลือกทาง reward เพื่อไปยึด/บังพื้นที่ได้ แต่คะแนนพื้นฐานยังไม่สูงเท่าช่องป่วนหรือการเข้าใกล้ผู้เล่น

### Test Case 4: Hunter AI ไล่ผู้เล่น

1. ตั้ง AI personality เป็น `Hunter` หรือวางผู้เล่นให้อยู่ในระยะ `Ambush Distance`
2. วางผู้เล่นมนุษย์ไว้บน board
3. สร้างทางแยกที่ทางหนึ่งเข้าใกล้ผู้เล่นกว่าอีกทาง
4. AI ควรเลือกทางที่ graph distance ไปหาผู้เล่นสั้นกว่า และถ้าไปลงช่องเดียวกันควรได้คะแนนสูงสุด

### Test Case 5: Auto Personality Switch

1. เปิด `Auto Switch Personality`
2. ทดสอบ 3 สถานการณ์: ผู้เล่นอยู่ใกล้, ทางแยกมีช่องป่วน, และ roaming ปกติ
3. Console log ควรแสดงการสลับบุคลิกจาก player proximity / roaming เช่น `Balanced -> Hunter` หรือ `Hunter -> Aggressive` ตามเหตุผลในวงเล็บ โดยไม่มีเหตุผลแบบ `low HP` แล้ว

## สรุปก่อน/หลังของ HP Logic

| หัวข้อ | ก่อนปรับ | หลังปรับ |
|---|---|---|
| HP ส่งผลต่อการเลือกทางไหม | ส่งผล: HP ต่ำทำให้เข้า Defensive และหา Heal | ไม่ส่งผล: AI เป็น zombie nuisance ไม่อ่าน HP เพื่อเลือกทาง |
| Heal tile | มีโอกาสได้คะแนนสูงตอนเลือดต่ำ | คะแนนติดลบ เพราะ AI ไม่ต้องรักษาชีวิต |
| การตายของ AI | HP อาจลดจน turn flow มองว่า HP หมดได้ | `TakeDamage`/`Heal` และตอนเริ่มเทิร์นจะคง HP ไว้ที่ค่า safe ไม่ให้ตายบนบอร์ด |
| Personality switch | มีเงื่อนไข `low HP -> Defensive` | เหลือ proximity/roaming: ใกล้ผู้เล่นเป็น Hunter, ทางป่วนเป็น Aggressive, reward เป็น Greedy |
| บทบาทรวม | ตัวป่วนที่ยังพยายามเอาตัวรอด | ซอมบี้ตัวป่วนที่ไม่มีวันตายและคอยวิ่งกดดันผู้เล่น |

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
