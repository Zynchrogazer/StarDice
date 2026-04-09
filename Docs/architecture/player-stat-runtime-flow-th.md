# Player Stat Runtime Flow (TH)

เอกสารนี้สรุปว่า **ตอนนี้ต้อง setup อะไรใน Unity เพิ่มไหม** และ **แต่ละสคริปต์ดึง/เขียนค่าจากแหล่งไหน** ใน flow ของ status (ATK/HP/SPD/DEF/Star bonus)

---

## 1) ต้อง setup เพิ่มเติมใน Unity ไหม?

### สรุปสั้น
- ถ้าในโปรเจกต์มี `PlayerStatAggregator`, `PassiveSkillManager`, `SkillManager`, `PlayerDataManager`, `GameTurnManager`, `PlayerState` อยู่แล้วและทำงานอยู่:  
  **ไม่ต้องเพิ่ม component ใหม่จากการแก้ล่าสุด**  
- แต่ควรตรวจว่า reference พื้นฐานครบตาม checklist ด้านล่าง

### Checklist แนะนำ
1. ใน runtime ต้องมี `PlayerStatAggregator` เพียง 1 ตัว (โค้ดกันซ้ำใน `Awake`).  
2. `PassiveSkillManager` และ `SkillManager` ควรอยู่ใน lifecycle เดียวกับ aggregator (เช่น RuntimeHub/persistent scene) เพื่อให้ `FindFirstObjectByType` resolve เจอตัวเดียวกันสม่ำเสมอ  
3. Board scene ต้องมี `PlayerState` ของผู้เล่นจริง (non-AI) และ `GameTurnManager` ต้องสามารถชี้ `CurrentPlayer` ได้  
4. UI status panel ต้อง bind ไปที่ `PlayerState` ผ่าน `PlayerUIController`/`PlayerStatsPanelPresenter`

---

## 2) Data source ของแต่ละค่า (ดึงจากไหน)

> หลักการ: **Single authority ตอน apply ค่าสุดท้ายอยู่ที่ `PlayerStatAggregator`**

### Base stat
- มาจาก `PlayerData`:
  - ATK: `attackDamage`
  - HP: `maxHP`
  - SPD: `speed`
  - DEF: `def`

### Passive level (สายอัปเลเวล)
- มาจาก `PassiveSkillManager`:
  - ATK bonus: `GetAttackBonusAmount()`
  - Star per gain bonus: `GetStarGainBonusAmount()`

### Passive tree (unlock node)
- มาจาก `SkillManager.GetUnlockedPassiveTotals()`:
  - `attackBonus`, `maxHpBonus`, `starBonus`, `speedBonus`, `defenseBonus`

### Equipment
- มาจาก `PlayerDataManager.equippedItems`
  - `attackBonus`, `speedBonus`, `defenseBonus`

### Runtime modifier (ระหว่างเกม)
- มาจาก `PlayerState` เอง:
  - `RuntimeAttackModifier`, `RuntimeMaxHealthModifier`, `RuntimeStarModifier`

---

## 3) การคำนวณ final value

`PlayerStatAggregator.RefreshPlayerStats(player, baseData)` จะรวมเป็น:

- `finalAttack = base ATK + passive ATK + tree ATK + equip ATK + RuntimeAttackModifier`
- `finalMaxHealth = base HP + tree HP + RuntimeMaxHealthModifier`
- `finalSpeed = base SPD + tree SPD + equip SPD`
- `finalDefense = base DEF + tree DEF + equip DEF`
- `PassiveStarGainBonus = passive star + tree star` (clamp ไม่ติดลบ)

แล้วเขียนกลับลง `PlayerState`:
- `CurrentAttack`, `MaxHealth`, `CurrentSpeed`, `CurrentDefense`, `PassiveStarGainBonus`
- จากนั้นเรียก `NotifyStatsUpdated()` เพื่อให้ UI refresh

---

## 4) Flow ตั้งแต่เริ่มจน UI แสดงผล

1. `PlayerState.LoadFromPlayerData(data)` เซ็ตค่า base ก่อน  
2. `PlayerState` เรียก `RequestAggregatedStatRefresh(data)`  
3. ถ้าเจอ aggregator แล้ว -> refresh ทันที  
4. ถ้ายังไม่เจอ -> subscribe `PlayerStatAggregator.OnAggregatorAvailable` และเก็บ pending source data  
5. เมื่อ `PlayerStatAggregator.Awake()` เสร็จ จะ emit `OnAggregatorAvailable`  
6. `PlayerState.HandleAggregatorAvailable(...)` รับ event แล้วเรียก `RefreshPlayerStats(...)`  
7. `PlayerState.NotifyStatsUpdated()` ถูกเรียก -> UI ฝั่ง `PlayerStatsPanelPresenter` อ่านค่าจาก `PlayerState` มาแสดง

---

## 5) คำตอบเรื่อง “ทำไมอัปแล้ว ATK ไม่เปลี่ยน”

จาก architecture ปัจจุบัน ถ้าเจออาการนี้ ให้เช็คตามลำดับ:
1. มี `PlayerStatAggregator` จริงใน runtime หรือไม่ (และไม่โดนทำลายซ้ำจนหาย)  
2. `GameTurnManager.CurrentPlayer` ชี้ตัวผู้เล่นถูกต้องตอนที่ refresh current player หรือไม่  
3. `PassiveSkillManager`/`SkillManager` อยู่ instance เดียวกับที่ aggregator หาเจอหรือไม่  
4. UI panel bind มาที่ `PlayerState` คนเดียวกับที่ถูก refresh หรือไม่

ถ้าข้อ 1-4 ถูกต้อง ค่า ATK บน panel ควรตรงกับ `PlayerState.CurrentAttack` ที่คำนวณใหม่แล้ว
