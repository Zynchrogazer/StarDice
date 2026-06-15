# Board AI Graph Look-Ahead สำหรับการนำเสนอ

<<<<<<< ours
=======
## Script ที่เกี่ยวข้อง

| Script | Path | หน้าที่ในหัวข้อนี้ |
| --- | --- | --- |
| `AIController.cs` | `Assets/0StarDice0/Scripts/MainGame/NPC/AIController.cs` | จุดหลักของ AI graph look-ahead, heuristic scoring, personality |
| `PlayerPathWalker.cs` | `Assets/0StarDice0/Scripts/MainGame/_Player/PlayerPathWalker.cs` | เรียก `AIController.ChoosePath(choices, stepsRemaining)` ตอน AI เจอทางแยก |
| `RouteManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_RouteManager/RouteManager.cs` | ให้ข้อมูล graph ผ่าน node connection และ tile type |
| `GameTurnManager.cs` | `Assets/0StarDice0/Scripts/MainGame/_GameSystem/GameTurnManager.cs` | คุม state หลักของเทิร์นก่อน/หลังการเดิน |

>>>>>>> theirs
## แนวคิด GameDev + Computer Science

ระบบนี้เพิ่มความรู้สึกแบบ Computer Science ให้กับ flow เดินบนบอร์ดโดยไม่เปลี่ยนลำดับการเล่นเดิม: ทอยเต๋า → เดินทีละช่อง → เจอทางแยก → เลือกทาง → เดินต่อ → trigger event ช่องสุดท้าย

จุดที่เพิ่มคือ AI จะไม่เลือกทางแยกจากช่องถัดไปแบบสั้น ๆ เท่านั้น แต่จะจำลองเส้นทางในกราฟล่วงหน้าตามจำนวนก้าวที่เหลือ แล้วใช้ heuristic score เพื่อเลือกทางที่มีประโยชน์ที่สุด

## หัวข้อที่สามารถพูดกับอาจารย์ได้

- **Graph Search**: บอร์ดถูกมองเป็นกราฟ โดย Node คือช่อง และ Edge คือทางเชื่อมระหว่างช่อง
- **Breadth-First Search (BFS)**: AI ใช้คิวเพื่อไล่ดูเส้นทางล่วงหน้าตาม depth หรือจำนวนก้าวที่เหลือ
- **Heuristic Evaluation**: ช่องแต่ละชนิดมีคะแนน เช่น Star/Treasure ดี, Trap/Lava แย่, Monster/Boss เสี่ยง
- **Decision Making**: AI รวมคะแนนปัจจุบันกับคะแนนปลายทางในอนาคต เพื่อเลือกทางแยก
<<<<<<< ours
=======
- **Performance Budget**: จำกัดทั้งจำนวนก้าวที่มองล่วงหน้า (`maxLookAheadSteps`) และจำนวน node ที่สำรวจ (`maxLookAheadNodes`) เพื่อกันกราฟที่มีลูปทำให้ค้นหามากเกินไป
>>>>>>> theirs
- **KISS**: ยังเป็นระบบเดียวใน `AIController` ไม่แยกคลาสเกินจำเป็น และจำกัด look-ahead เพื่อไม่ให้หนักเครื่อง
- **SOLID แบบพอดีโปรเจกต์**: `PlayerPathWalker` ยังรับผิดชอบการเดิน ส่วน `AIController` รับผิดชอบการตัดสินใจของ AI

## Flow ใหม่แบบย่อ

1. `PlayerPathWalker` เจอทางแยกของ AI
2. ส่ง `choices` และ `stepsRemaining` ไปให้ `AIController`
3. `AIController` ประเมินแต่ละทางเลือก
<<<<<<< ours
4. จำลองเส้นทางต่อด้วย graph look-ahead ตามจำนวนก้าวที่เหลือ
=======
4. จำลองเส้นทางต่อด้วย graph look-ahead ตามจำนวนก้าวที่เหลือ โดยมี budget จำกัดจำนวน node
>>>>>>> theirs
5. เลือกทางที่มีคะแนนรวมดีที่สุด
6. ส่ง node กลับให้ระบบเดินเดิมทำงานต่อ

## สิ่งที่ยังไม่เปลี่ยนเพื่อความปลอดภัย

- ไม่เปลี่ยนระบบทอยเต๋า
- ไม่เปลี่ยน coroutine เดินทีละช่อง
- ไม่เปลี่ยน event ตอนลงช่องสุดท้าย
- ไม่เปลี่ยน UI เลือกทางของผู้เล่นมนุษย์
- ถ้าไม่มี `AIController` ระบบยัง fallback เป็นสุ่มเหมือนเดิม

## ประโยคสำหรับนำเสนอ

> ในโปรเจกต์นี้ ผม/หนูออกแบบบอร์ดเกมให้สามารถมองเป็น Graph ได้ โดย AI ใช้ Breadth-First Search เพื่อจำลองเส้นทางล่วงหน้าตามจำนวนก้าวที่เหลือจากลูกเต๋า จากนั้นใช้ heuristic scoring เพื่อเลือกทางแยกที่เหมาะสมที่สุด ทำให้ระบบดูมีการตัดสินใจเชิงอัลกอริทึมมากขึ้น แต่ยังคง flow การเดินเดิมของเกมไว้ทั้งหมด
<<<<<<< ours
=======

## ถ้าถูกถามเจาะเรื่อง Graph

ให้ตอบสั้น ๆ ว่า Graph ในเกมนี้คือโครงสร้างของบอร์ด: `NodeConnection.node` คือช่อง, `NodeConnection.connectedNodes` คือทางที่เดินต่อได้ และ AI ใช้ Queue สำรวจเส้นทางล่วงหน้าแบบ bounded look-ahead เพื่อเลือกทางแยก ดูคำตอบละเอียดได้ใน `Docs/GraphPresentationQAGuide.md`
>>>>>>> theirs
