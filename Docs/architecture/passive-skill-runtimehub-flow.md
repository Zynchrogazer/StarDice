# Passive Skill Flow เมื่อใช้ RuntimeHub เป็นศูนย์กลาง

## ข้อแนะนำสั้น ๆ
ให้วาง **PassiveSkillManager + SkillManager + PlayerStatAggregator ไว้ใน RuntimeHub** (ฉาก persistent) และให้ทุกฉากที่ถูกโหลดแบบ additive เรียกใช้งานผ่าน service เดียว แทนการมี manager ของตัวเองในแต่ละฉาก

## ตอบคำถามที่เจอบ่อย: ถ้า `PlayerStatAggregator` ต้องเรียก `PassiveSkillManager`/`SkillManager` ควรวางยังไง?
- ให้ทั้ง **สามตัวอยู่ใน RuntimeHub เหมือนกัน** (object คนละตัวได้ แต่ lifecycle เดียวกัน)
- ให้ `PlayerStatAggregator` ถือ reference ไปที่ manager ผ่าน
  - serialized field ที่ผูกจาก RuntimeHub ในตอน bootstrap หรือ
  - dependency injection / service locator ของ RuntimeHub
- **ไม่ควร**ให้ `PlayerStatAggregator` ไปหา manager จาก scene ลูกโดยตรง เพราะจะเสี่ยงเจอ object คนละ lifecycle

> สั้น ๆ: Aggregator อยู่ RuntimeHub และอ้าง manager ใน RuntimeHub เท่านั้น

## Flow การทำงานของแต่ละตัว (จากโค้ดปัจจุบัน)

### 1) `PassiveSkillManager.cs`
หน้าที่หลัก
- ถือ progression ของ passive แบบเลเวล (`starSkillLevel`, `attackSkillLevel`)
- คำนวณราคาอัปเกรด + โบนัส (`GetStarBonusAmount`, `GetAttackBonusAmount`)
- บันทึก/โหลดจาก `PlayerPrefs` แบบแยกตามผู้เล่น

โฟลว์หลัก
1. `Awake()` กัน manager ซ้ำ แล้ว resolve `PlayerStatAggregator`
2. `TryUpgradeStarSkill` / `TryUpgradeAttackSkill`
   - เช็กเครดิต (`TrySpendCurrentPlayerCredit`)
   - เพิ่มเลเวล
   - เรียก `ApplyPassiveBonusToCurrentPlayer()`
   - `SaveData()`
3. `ApplyPassiveBonusToCurrentPlayer()` จะพยายามส่งงานให้ `PlayerStatAggregator.RefreshCurrentPlayerStats()` ก่อน

### 2) `SkillManager.cs`
หน้าที่หลัก
- จัดการ passive skill tree แบบ unlock ราย node (`unlockedSkillIDs`)
- ตรวจเงื่อนไขปลดล็อก (`CanUnlock`) และตัดเครดิต (`TrySpendCredit`)
- รวมโบนัสจากสกิลที่ปลดล็อกทั้งหมด (`GetUnlockedPassiveTotals`)

โฟลว์หลัก
1. `Awake()` resolve `PlayerStatAggregator` + โหลดข้อมูลผู้เล่นปัจจุบัน
2. `Start()` โหลดอีกครั้งเพื่อกัน timing แล้วเรียก `ApplyAllPassiveBonusesToCurrentPlayer()`
3. เมื่อ `TryUnlockSkill()` สำเร็จ
   - เพิ่ม skill id
   - save
   - เรียก `ApplyAllPassiveBonusesToCurrentPlayer()`
   - ยิง `OnSkillTreeUpdated`
4. `ApplyAllPassiveBonusesToCurrentPlayer()` จะส่งงานให้ `PlayerStatAggregator.RefreshCurrentPlayerStats()` ก่อนเช่นกัน

### 3) `PlayerStatAggregator.cs`
หน้าที่หลัก
- เป็น **single authority** ของการรวม stat สุดท้าย
- รวมค่าจาก
  - base stat ของตัวละคร (`GameData.Instance.selectedPlayer`)
  - โบนัสจาก `PassiveSkillManager`
  - โบนัสจาก `SkillManager`
  - runtime modifier ที่เกิดระหว่างเกม (`RuntimeAttackModifier`, `RuntimeMaxHealthModifier`, `RuntimeStarModifier`)

โฟลว์หลัก
1. `Awake()` กัน aggregator ซ้ำ แล้ว resolve manager reference
2. `RefreshCurrentPlayerStats()`
   - เช็ก current player/selected player
   - ดึงโบนัสจาก `PassiveSkillManager` + `SkillManager`
   - รวมเป็นค่า final (attack/maxHP/star)
   - apply กลับลง `PlayerState`
   - ปรับ HP และ Star แบบ delta กันค่าซ้อน
   - `NotifyStatsUpdated()`

## Flow รวมทั้งระบบ (ลำดับ runtime)
1. MainMenu โหลด `RuntimeHub`
2. RuntimeHub bootstrap service (`PassiveSkillManager`, `SkillManager`, `PlayerStatAggregator`, `GameData`)
3. ตอน bootstrap ให้ bind reference `PlayerStatAggregator -> PassiveSkillManager/SkillManager`
4. โหลด Board/Intermission/Battle แบบ additive
5. เมื่อ scene ลูกเริ่มทำงาน ให้เรียก refresh ผ่าน RuntimeHub service
6. เมื่อมีการอัป passive หรือปลดล็อก skill -> manager ที่เกี่ยวข้องบันทึกข้อมูล + เรียก aggregator refresh ทันที
7. เปลี่ยนฉากต่อโดยคง RuntimeHub ไว้

## Trigger points ที่ควรเรียก refresh
- ตอนเข้า run / หลังเลือกตัวละคร
- หลังอัปเกรด passive skill
- หลังปลดล็อก skill tree node
- หลังกลับจาก battle (ก่อนเริ่มเทิร์น/เริ่ม scene รอบถัดไป)
- ตอนสลับ selected player

## Guardrail สำคัญ
- ใน scene ชั่วคราวอย่า instantiate manager เพิ่ม
- ถ้าจำเป็นต้อง `FindFirstObjectByType` ให้ทำในจุดเดียว (RuntimeHub bootstrap) แล้ว cache reference
- จุด apply stat ให้มี single authority (`PlayerStatAggregator`) เพื่อกัน stat ซ้อน
- แยกชัดเจนระหว่าง
  - **Persistent progression state** (skill levels / unlocked skills)
  - **Per-scene presentation state** (UI/animation)

## Mapping กับโค้ดปัจจุบัน
- `SceneFlowController` มีแนวคิด persistent scene (`RuntimeHub`) อยู่แล้ว
- `PassiveSkillManager` มีการกัน manager ซ้ำและมี save/load ตามผู้เล่น
- `SkillManager` จัดการ unlock tree + โบนัสรวมจาก node
- `PlayerStatAggregator` เป็นตำแหน่งที่เหมาะสุดสำหรับ single authority ในการ apply bonus

สรุป: ถ้าถามว่า flow ควรอยู่ที่ไหน — ให้อยู่ที่ **RuntimeHub (system layer)** และให้ scene additive ต่าง ๆ เป็นผู้บริโภค service ไม่ใช่ owner ของ state


## ตัวอย่างละเอียด: โหลด `Upgrade.unity` แบบ additive แล้วกดอัปเกรดทำงานยังไง

### A) ก่อนเข้า `Upgrade.unity` (RuntimeHub ยังอยู่)
1. `RuntimeHub` มี instance เดียวของ `PassiveSkillManager`, `SkillManager`, `PlayerStatAggregator`
2. ทั้งสามตัว resolve reference หากันเสร็จ (หรือ bind มาตั้งแต่ bootstrap)
3. แต่ละ manager `EnsureLoadedForCurrentPlayer()` เพื่อให้ข้อมูลตรงกับ `selectedPlayer`

### B) ตอนโหลด `Upgrade.unity` แบบ additive
1. Scene `Upgrade.unity` ถูกโหลดเข้ามาเฉพาะ UI/interaction layer
2. UI ใน Upgrade scene ไปอ้าง service จาก RuntimeHub (ไม่สร้าง manager ใหม่)
3. UI อ่านข้อมูลมาแสดง เช่น
   - เลเวลปัจจุบัน
   - ราคาอัปเกรดถัดไป (`GetStarUpgradeCost` / `GetAttackUpgradeCost`)
   - เครดิตที่ผู้เล่นมีอยู่

### C) ตอนผู้เล่นกดปุ่มอัปเกรด (ตัวอย่าง passive level)
1. ปุ่มใน UI เรียก `PassiveSkillManager.TryUpgradeStarSkill()` หรือ `TryUpgradeAttackSkill()`
2. ระบบจะเช็กตามลำดับนี้
   - โหลดข้อมูลผู้เล่นปัจจุบันล่าสุด (`EnsureLoadedForCurrentPlayer`)
   - คำนวณราคาอัปเกรด
   - เช็กเครดิตพอหรือไม่ (`TrySpendCurrentPlayerCredit`)
   - เช็กว่ามี `selectedPlayer`/ข้อมูลผู้เล่นอยู่จริง
3. ถ้าผ่านเงื่อนไข
   - หักเครดิต
   - เพิ่มเลเวล passive
   - บันทึก `PlayerPrefs`
   - เรียก `ApplyPassiveBonusToCurrentPlayer()`
4. จากนั้น `PassiveSkillManager` จะส่งต่อให้ `PlayerStatAggregator.RefreshCurrentPlayerStats()`
5. `PlayerStatAggregator` รวม stat ใหม่ทั้งหมด (base + passive + unlocked + runtime modifier) แล้ว apply ลง `PlayerState`
6. `NotifyStatsUpdated()` ถูกเรียก -> UI/ระบบที่ฟัง event อัปเดตค่าบนจอ

### D) ตอนผู้เล่นกดปลดล็อกใน skill tree (node unlock)
1. UI เรียก `SkillManager.TryUnlockSkill(skill)`
2. ระบบจะเช็กตามลำดับนี้
   - skill นั้น valid ไหม / ถูกปลดล็อกไปแล้วยัง
   - เครดิตพอไหม
   - prerequisite ครบไหม (`requiredSkills`)
3. ถ้าผ่านเงื่อนไข
   - หักเครดิต
   - เพิ่ม skill id ลง `unlockedSkillIDs`
   - save
   - เรียก `ApplyAllPassiveBonusesToCurrentPlayer()`
4. `SkillManager` ส่งต่อให้ `PlayerStatAggregator.RefreshCurrentPlayerStats()`
5. ยิง `OnSkillTreeUpdated` เพื่อรีเฟรช UI skill tree

### E) ตอนปิด `Upgrade.unity` แล้วกลับฉากอื่น
1. Unload แค่ scene ลูก (`Upgrade.unity`)
2. `RuntimeHub` ยังอยู่ -> manager/state ไม่หาย
3. รอบถัดไปที่เข้า Upgrade ใหม่ จะเห็นค่าล่าสุดจาก save/runtime state เดิม

### สรุปสิ่งที่ระบบ “เช็ค” สำคัญที่สุดใน flow นี้
- มี current player + selected player หรือยัง
- เครดิตพอสำหรับการอัปเกรด/ปลดล็อกหรือไม่
- prerequisite skill ครบหรือไม่ (กรณี skill tree)
- manager reference ชี้ไปที่ตัวใน RuntimeHub หรือเปล่า (ไม่ใช่ scene ลูก)
- มี single authority ตอน apply stat (`PlayerStatAggregator`) เพื่อไม่ให้โบนัสซ้อน

## แล้วใน `Upgrade.unity` ควรมี script อะไรบ้าง?

> หลักคิด: `Upgrade.unity` เป็น **UI/interaction scene** ไม่ใช่เจ้าของ progression state

### 1) `UpgradeSceneController` (ควรมี)
หน้าที่
- เป็นตัว orchestrate ของ scene นี้
- resolve reference ไปที่ `PassiveSkillManager`, `SkillManager`, `PlayerStatAggregator` จาก RuntimeHub
- ผูก event กับ UI ปุ่มต่าง ๆ
- เรียก refresh UI ตอน `OnEnable` / หลัง action สำเร็จ

เมธอดที่ควรมี
- `InitializeFromRuntimeHub()`
- `RefreshAllUpgradeViews()`
- `OnClickUpgradeStar()`
- `OnClickUpgradeAttack()`
- `OnClickUnlockSkill(PassiveSkillData skill)`

### 2) `UpgradeCostView` / `UpgradeStatPreviewView` (ควรมี)
หน้าที่
- แสดงค่าเลเวลปัจจุบัน, ราคาอัปเกรดถัดไป, เครดิตคงเหลือ
- แสดง preview ก่อน/หลังอัปเกรด (ถ้าต้องการ UX ดีขึ้น)

ข้อสำคัญ
- เป็น view-only: รับข้อมูลจาก controller แล้ว render
- ไม่ควรเรียก `PlayerPrefs` เอง และไม่ควรคำนวณ progression state เอง

### 3) `SkillTreeNodeButton` (ควรมี ถ้ามี skill tree UI)
หน้าที่
- ผูกกับ `PassiveSkillData` ของ node นั้น
- ส่งคำสั่งไปที่ controller เพื่อเรียก `SkillManager.TryUnlockSkill(...)`
- แสดง state ของ node: locked / unlockable / unlocked

ข้อสำคัญ
- logic เช็ก prerequisite ใช้ผลจาก `SkillManager` เท่านั้น
- หลีกเลี่ยงการ duplicate เงื่อนไข unlock ใน UI หลายจุด

### 4) `UpgradeFeedbackPresenter` (แนะนำ)
หน้าที่
- แสดง feedback เช่น "เครดิตไม่พอ", "ปลดล็อกสำเร็จ", "ต้องปลดล็อก node ก่อนหน้า"
- subscribe event เช่น `SkillManager.OnSkillTreeUpdated` แล้วสั่ง refresh บางส่วนของ UI

### 5) `UpgradeNavigationController` (แนะนำ)
หน้าที่
- จัดการปุ่มกลับ/ปิด scene upgrade
- ส่งสัญญาณให้ scene flow unload `Upgrade.unity` แบบ additive

## สิ่งที่ "ไม่ควร" อยู่ใน `Upgrade.unity`
- ไม่ควรมี `PassiveSkillManager` / `SkillManager` / `PlayerStatAggregator` instance ใหม่
- ไม่ควรมี script ที่ save/load progression โดยตรงผ่าน `PlayerPrefs`
- ไม่ควรมี script ที่ apply stat ลง `PlayerState` โดยข้าม `PlayerStatAggregator`

## โครงสั้น ๆ ที่แนะนำในเชิงสถาปัตยกรรม
- RuntimeHub (persistent):
  - `PassiveSkillManager`
  - `SkillManager`
  - `PlayerStatAggregator`
- Upgrade.unity (additive):
  - `UpgradeSceneController`
  - `UpgradeCostView` / `UpgradeStatPreviewView`
  - `SkillTreeNodeButton`
  - `UpgradeFeedbackPresenter`
  - `UpgradeNavigationController`

สรุป: ใน `Upgrade.unity` ให้มี **controller + view + interaction script** เท่านั้น ส่วน logic progression/stat authority ให้อยู่ที่ RuntimeHub ทั้งหมด


## KISS version ที่ใช้ได้ทันทีใน `Upgrade.unity`
ถ้าต้องการเริ่มแบบง่ายที่สุด ให้ใช้ script เดียวคือ `SkillTreeUI` เป็น scene controller โดยให้มันทำแค่ 4 อย่าง:
1. รับปุ่มจาก UI (upgrade star / upgrade attack / back)
2. เรียก `PassiveSkillManager` ใน RuntimeHub เท่านั้น
3. อัปเดตข้อความ UI และปุ่ม interactable
4. ตอนกด back ถ้าเป็น additive ให้ unload scene ปัจจุบัน

แนวคิดนี้ทำให้ scene มี logic น้อย (KISS) และยังรักษาหลัก single authority ของ stat ที่ `PlayerStatAggregator`
