using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[System.Serializable]


public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerStatAggregator playerStatAggregator;
    private const string IntermissionSceneName = "InterMission";

    //public static PlayerState Instance { get; private set; }
    [Header("AI Settings")]
    public bool isAI = false;
    [Header("Player Stats")]
    public int PlayerHealth;   // เปลี่ยนจาก Property เป็น Field เพื่อให้เห็นใน Inspector
    public int MaxHealth;      // ✅ เพิ่ม: เพื่อใช้คุมเพดานเลือด
    [FormerlySerializedAs("PlayerCredit")]
    [SerializeField] private int playerCredit = 0;
    public int PlayerStar = 0;
    public int CurrentAttack;
    public int CurrentSpeed;
    public int CurrentDefense;
    public int RuntimeAttackModifier = 0;
    public int RuntimeMaxHealthModifier = 0;
    public int RuntimeStarModifier = 0;
    public int PassiveStarGainBonus = 0;
    public bool DebuffBurn = false;
    public int backwardCurseTurns = 0;
    public int DebuffBurnTurnsRemaining = 0;
    public bool hasIceEffect = false;
    public int poisonDebuffTurns = 0;
    public int sleepDebuffTurns = 0;
    private int burnDebuffAppliedOrder = 0;
    private int iceDebuffAppliedOrder = 0;
    private static int debuffApplySequence = 0;
    public ElementType PlayerElement { get; set; }

    public int BurnDebuffAppliedOrder => burnDebuffAppliedOrder;
    public int IceDebuffAppliedOrder => iceDebuffAppliedOrder;

    [Header("Battle Stats")]
    public int WinCount = 0;
    

    [Header("Level System")]   // ✅ เพิ่ม: ระบบเลเวล
    public int PlayerLevel = 1;
    public int CurrentExp = 0;
    public int MaxExp = 100;

    [Header("Data & Inventory")]
    public PlayerData selectedPlayerPreset { get; private set; }
    public List<CardData> selectedCards { get; private set; } = new List<CardData>();

    // Snapshot ของค่าถาวรจาก persistence (ใช้รีเซ็ตเมื่อออกจาก Board)
    [SerializeField] private PlayerProgressSnapshot persistentProgressSnapshot = new PlayerProgressSnapshot();

    // Runtime skill unlock state (ใช้เฉพาะรอบ Boardgame เท่านั้น)
    private readonly HashSet<int> runtimeUnlockedSkillIndexes = new HashSet<int>();
    private int runtimeSkillCount = 0;
    private const int DefaultUnlockedSkillCount = 3;

    public event System.Action OnDied;
    public event Action OnStatsUpdated;

[Header("Status Effects")]
    // (พวก DebuffBurn, hasIceEffect เดิมของคุณ...)
    
    // 🟢 เพิ่มตัวแปรนี้เพื่อนับเทิร์นที่ต้องหยุดเดิน
    public int StunTurnsRemaining = 0;

    [Header("Buffs")]
    // 🟢 เพิ่มตัวแปรนี้สำหรับเช็คบัฟดาว x2
    public bool hasDoubleStarBuff = false;

    public int PlayerCredit
    {
        get
        {
            if (TryResolveSelectedProgress(out PlayerProgress progress))
            {
                playerCredit = Mathf.Max(0, progress.Credit);
            }

            return Mathf.Max(0, playerCredit);
        }
        set
        {
            int normalizedValue = Mathf.Max(0, value);
            playerCredit = normalizedValue;

            if (TryResolveSelectedProgress(out PlayerProgress progress))
            {
                progress.SetCredit(normalizedValue);
            }

            OnStatsUpdated?.Invoke();
        }
    }
    public int GetPerGainStarBonus()
    {
        return Mathf.Max(0, PassiveStarGainBonus + RuntimeStarModifier);
    }

    public int AddStars(int baseAmount)
    {
        int clampedBase = Mathf.Max(0, baseAmount);
        if (clampedBase <= 0)
            return 0;

        int totalGain = clampedBase + GetPerGainStarBonus();
        PlayerStar = Mathf.Max(0, PlayerStar + totalGain);
        OnStatsUpdated?.Invoke();
        return totalGain;
    }

    public int RemoveStars(int amount)
    {
        int clampedAmount = Mathf.Max(0, amount);
        int before = PlayerStar;
        PlayerStar = Mathf.Max(0, PlayerStar - clampedAmount);
        int removed = before - PlayerStar;
        OnStatsUpdated?.Invoke();
        return removed;
    }

    private bool isDefeatHandling = false;
    private void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
    }

    private void Start()
    {
        if (!isAI && GameData.Instance != null)
        {
            LoadFromPlayerData(GameData.Instance.selectedPlayer);
            SetSelectedCards(GameData.Instance.selectedCards);
        }
        else if (isAI)
        {
            // (Optional) ถ้าอยากให้บอทเริ่มมาเลือดเต็มตาม MaxHealth ที่ตั้งใน Inspector
            PlayerHealth = MaxHealth;
        }

        OnDied += HandleDefeat;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        OnDied -= HandleDefeat;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private bool TryResolveSelectedProgress(out PlayerProgress progress)
    {
        progress = PlayerStateProgressCoordinator.ResolveSelectedProgress(this);
        return progress != null;
    }

    internal void BindSelectedPlayerPreset(PlayerData data)
    {
        selectedPlayerPreset = data;
    }

    public void LoadFromPlayerData(PlayerData data)
    {
        if (data == null) return;
        selectedPlayerPreset = data;

        // 1. โหลดค่าพลังชีวิต
        MaxHealth = data.GetMaxHealth();
        PlayerHealth = MaxHealth;
        CurrentAttack = data.attackDamage;
        CurrentSpeed = data.speed;
        CurrentDefense = data.def;
        RuntimeAttackModifier = 0;
        RuntimeMaxHealthModifier = 0;
        RuntimeStarModifier = 0;
        PassiveStarGainBonus = 0;
        // 2. โหลดเครดิต/ความก้าวหน้าถาวรจาก PlayerProgress
        if (TryResolveSelectedProgress(out PlayerProgress progress))
        {
            playerCredit = progress.Credit;
            PlayerLevel = progress.Level;
            CurrentExp = progress.CurrentExp;
            MaxExp = progress.MaxExp;
        }
        else
        {
            playerCredit = data.startingCredit;
            PlayerLevel = data.startingLevel;
            CurrentExp = data.startingCurrentExp;
            MaxExp = data.startingMaxExp;
        }

        // กันเหนียว: ถ้า MaxExp เป็น 0 ให้ตั้งค่าเริ่มต้น
        if (MaxExp <= 0) MaxExp = 100;

        PlayerStateProgressCoordinator.CaptureSnapshot(this, data, persistentProgressSnapshot);
        InitializeRuntimeSkillUnlocks(data.allSkills != null ? data.allSkills.Length : 0);
        EnsureRuntimeSkillUnlocksMatchLevel();

        if (ResolvePlayerStatAggregator() != null)
        {
            ResolvePlayerStatAggregator().RefreshCurrentPlayerStats();
        }

        Debug.Log($"[PlayerState] Loaded: Level {PlayerLevel}, HP {PlayerHealth}/{MaxHealth}");
    }

    public void SetSelectedCards(List<CardData> cards)
    {
        selectedCards = cards != null ? new List<CardData>(cards) : new List<CardData>();
    }

    // --- Combat Logic ---

    public void TakeDamage(int dmg)
    {
        PlayerHealth -= dmg;
        Debug.Log($"Took {dmg} damage. HP: {PlayerHealth}/{MaxHealth}");

        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            OnDied?.Invoke();
        }
    }

    public void Heal(int heal)
    {
        PlayerHealth += heal;

        // ✅ เพิ่ม Logic: ห้ามเกิน MaxHealth
        if (PlayerHealth > MaxHealth)
        {
            PlayerHealth = MaxHealth;
        }

        Debug.Log($"Healed {heal}. HP: {PlayerHealth}/{MaxHealth}");

        // (เช็ค <= 0 เผื่อไว้กรณี heal เป็นลบ แต่ปกติไม่ควรเกิด)
        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            OnDied?.Invoke();
        }
    }

    public bool TryConsumeIceDebuff()
    {
        if (!hasIceEffect) return false;
        hasIceEffect = false;
        iceDebuffAppliedOrder = 0;
        OnStatsUpdated?.Invoke();
        return true;
    }

    public void ApplyIceDebuff()
    {
        if (!hasIceEffect)
        {
            iceDebuffAppliedOrder = NextDebuffApplyOrder();
        }

        hasIceEffect = true;
        OnStatsUpdated?.Invoke();
    }

    public void ApplyBurnDebuff(int turns)
    {
        DebuffBurn = turns > 0;
        DebuffBurnTurnsRemaining = Mathf.Max(0, turns);

        if (DebuffBurn)
        {
            if (burnDebuffAppliedOrder <= 0)
                burnDebuffAppliedOrder = NextDebuffApplyOrder();
        }
        else
        {
            burnDebuffAppliedOrder = 0;
        }

        OnStatsUpdated?.Invoke();
    }

    public void ApplySleepDebuff(int turns = 3)
    {
        sleepDebuffTurns = turns;
        Debug.Log($"<color=blue>💤 Zzz... ผู้เล่นติดสถานะหลับ หยุดเดิน {turns} เทิร์น</color>");
        OnStatsUpdated?.Invoke();
    }

    public void ApplyBackwardCurse(int turns = 3)
    {
        backwardCurseTurns = turns;
        Debug.Log($"<color=purple>💀 ติดคำสาป! บังคับเดินถอยหลังจำนวน {turns} เทิร์น</color>");
        OnStatsUpdated?.Invoke();
    }

    public void ApplyPoisonDebuff(int turns = 3)
    {
        poisonDebuffTurns = turns;
        Debug.Log($"<color=green>☠️ ติดสถานะพิษ! จะโดนดาเมจตามจำนวนก้าวเป็นเวลา {turns} เทิร์น</color>");
        OnStatsUpdated?.Invoke();
    }

    public bool TryConsumeBurnDebuff(int burnDamage)
    {
        if (!DebuffBurn || DebuffBurnTurnsRemaining <= 0) return false;

        TakeDamage(burnDamage);

        DebuffBurnTurnsRemaining = Mathf.Max(0, DebuffBurnTurnsRemaining - 1);
        DebuffBurn = DebuffBurnTurnsRemaining > 0;
        if (!DebuffBurn)
            burnDebuffAppliedOrder = 0;
        OnStatsUpdated?.Invoke();
        return true;
    }

    private static int NextDebuffApplyOrder()
    {
        debuffApplySequence++;
        if (debuffApplySequence >= int.MaxValue - 4)
            debuffApplySequence = 1;
        return debuffApplySequence;
    }

    // --- Level & EXP Logic (New) ---

    public void GainExp(int amount)
    {
        CurrentExp += amount;
        if (CurrentExp >= MaxExp)
        {
            LevelUpRPG();
        }

        EnsureRuntimeSkillUnlocksMatchLevel();

        OnStatsUpdated?.Invoke();
    }

    private void LevelUpRPG()
    {
        CurrentExp -= MaxExp;
        PlayerLevel++;
        MaxExp = Mathf.CeilToInt(MaxExp * 1.2f); // เวลต่อไปยากขึ้น 20%

        // Bonus เมื่อเวลอัป (สไตล์ RPG)
        MaxHealth += 5;
        PlayerHealth = MaxHealth; // เลือดเด้งเต็ม
        CurrentAttack += 1;       // ตีแรงขึ้น

        if (PlayerLevel % 2 == 0)
        {
            CurrentDefense += 1; // เพิ่ม DEF ทุก ๆ 2 เลเวล
        }

        if (PlayerLevel % 5 == 0)
        {
            CurrentSpeed += 1; // เพิ่ม SPD ทุก ๆ 5 เลเวล
        }

        Debug.Log($"💪 RPG LEVEL UP! Lv.{PlayerLevel} (HP: {MaxHealth}, ATK: {CurrentAttack}, SPD: {CurrentSpeed}, DEF: {CurrentDefense})");

        // ถ้า EXP ยังเหลือเฟือ ก็ให้เช็คเวลอัปซ้ำ
        if (CurrentExp >= MaxExp) LevelUpRPG();
    }

    public void RecordBattleWin()
    {
        WinCount++;
        
        GainExp(50);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isAI || scene.name != IntermissionSceneName) return;

        // เครดิตเป็นค่าถาวร -> sync กลับข้อมูลหลัก
        PlayerStateProgressCoordinator.SyncPersistentCredit(this);

        // เลเวล/EXP ในบอร์ดเป็นค่าชั่วคราว -> รีเซ็ตและย้ำข้อมูลใน persistence
        PlayerStateProgressCoordinator.RestoreSnapshot(persistentProgressSnapshot);
        ResetInStageProgress();
    }

    public void ResetForNewBoardSession()
    {
        if (isAI)
        {
            PlayerStar = 0;
            WinCount = 0;
            DebuffBurn = false;
            DebuffBurnTurnsRemaining = 0;
            hasIceEffect = false;
            burnDebuffAppliedOrder = 0;
            iceDebuffAppliedOrder = 0;
            OnStatsUpdated?.Invoke();
            return;
        }

        PlayerData sourceData = selectedPlayerPreset;
        if (sourceData == null && GameData.Instance != null)
        {
            sourceData = GameData.Instance.selectedPlayer;
        }

        if (sourceData != null)
        {
            LoadFromPlayerData(sourceData);
        }

        PlayerStar = 0;
        WinCount = 0;
        DebuffBurn = false;
        DebuffBurnTurnsRemaining = 0;
        hasIceEffect = false;
        burnDebuffAppliedOrder = 0;
        iceDebuffAppliedOrder = 0;
        OnStatsUpdated?.Invoke();
    }

    private PlayerStatAggregator ResolvePlayerStatAggregator()
    {
        if (playerStatAggregator == null)
            playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();

        return playerStatAggregator;
    }

    private void HandleDefeat()
    {
        if (isDefeatHandling || isAI) return;
        isDefeatHandling = true;

        // ✅ เก็บเครดิตสะสมทั้งหมดกลับข้อมูลหลัก เพื่อให้คงอยู่หลังออกจากด่าน
        PlayerStateProgressCoordinator.SyncPersistentCredit(this);

        // ✅ รีเซ็ตความก้าวหน้าภายในด่าน (เลเวล/EXP/Win) เมื่อแพ้
        ResetInStageProgress();

        Debug.Log($"[PlayerState] Defeated -> return to {IntermissionSceneName}");
        RequestSceneCompat(IntermissionSceneName);
    }

    private static void RequestSceneCompat(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (!SceneFlowController.TryRequestScene(sceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"[SceneLoader] Cannot load scene '{sceneName}'. Check Build Profiles.");
            }
        }
    }

    private void ResetInStageProgress()
    {
        PlayerData sourceData = selectedPlayerPreset;
        if (sourceData == null && GameData.Instance != null)
        {
            sourceData = GameData.Instance.selectedPlayer;
        }

        if (sourceData != null)
        {
            PlayerStateProgressCoordinator.ApplyPersistentProgressToRuntime(this, sourceData);
            MaxHealth = sourceData.GetMaxHealth();
            PlayerHealth = MaxHealth;
            CurrentAttack = sourceData.attackDamage;
            CurrentSpeed = sourceData.speed;
            CurrentDefense = sourceData.def;
        }
        else
        {
            PlayerStateProgressCoordinator.ApplyPersistentProgressToRuntime(this, null);
            PlayerHealth = Mathf.Max(PlayerHealth, 1);
            CurrentSpeed = Mathf.Max(CurrentSpeed, 0);
            CurrentDefense = Mathf.Max(CurrentDefense, 0);
        }

        WinCount = 0;
        hasIceEffect = false;
        DebuffBurn = false;
        DebuffBurnTurnsRemaining = 0;
        burnDebuffAppliedOrder = 0;
        iceDebuffAppliedOrder = 0;
        RuntimeAttackModifier = 0;
        RuntimeMaxHealthModifier = 0;
        RuntimeStarModifier = 0;
        PassiveStarGainBonus = 0;

        if (sourceData != null && sourceData.allSkills != null)
        {
            InitializeRuntimeSkillUnlocks(sourceData.allSkills.Length);
            EnsureRuntimeSkillUnlocksMatchLevel();
        }
        else
        {
            ResetRuntimeSkillUnlocks();
        }

        if (ResolvePlayerStatAggregator() != null)
        {
            ResolvePlayerStatAggregator().RefreshCurrentPlayerStats();
        }

        OnStatsUpdated?.Invoke();
    }
    public void InitializeRuntimeSkillUnlocks(int totalSkills, int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        runtimeUnlockedSkillIndexes.Clear();
        runtimeSkillCount = Mathf.Max(0, totalSkills);

        int clampedDefault = Mathf.Clamp(defaultUnlockedCount, 0, runtimeSkillCount);
        for (int i = 0; i < clampedDefault; i++)
        {
            runtimeUnlockedSkillIndexes.Add(i);
        }
    }

    public void ResetRuntimeSkillUnlocks()
    {
        runtimeUnlockedSkillIndexes.Clear();
        runtimeSkillCount = 0;
    }

    public void NotifyStatsUpdated()
    {
        OnStatsUpdated?.Invoke();
    }

    public bool IsSkillUnlocked(int skillIndex, int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        if (skillIndex < 0) return false;
        if (skillIndex < defaultUnlockedCount) return true;
        return runtimeUnlockedSkillIndexes.Contains(skillIndex);
    }

    public int GetRuntimeUnlockedSkillCount(int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        int baseCount = Mathf.Clamp(defaultUnlockedCount, 0, runtimeSkillCount);
        int extraCount = 0;

        foreach (int index in runtimeUnlockedSkillIndexes)
        {
            if (index >= defaultUnlockedCount)
            {
                extraCount++;
            }
        }

        return Mathf.Clamp(baseCount + extraCount, 0, runtimeSkillCount);
    }

    public int GetTargetUnlockedSkillCountByLevel(int totalSkills, int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        int target = defaultUnlockedCount + Mathf.Max(0, PlayerLevel / 10);
        return Mathf.Clamp(target, 0, Mathf.Max(0, totalSkills));
    }

    public bool UnlockRandomLockedSkill(int totalSkills, out int unlockedIndex, int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        unlockedIndex = -1;

        if (totalSkills <= 0) return false;

        if (runtimeSkillCount != totalSkills)
        {
            InitializeRuntimeSkillUnlocks(totalSkills, defaultUnlockedCount);
        }

        List<int> lockedIndexes = new List<int>();
        for (int i = defaultUnlockedCount; i < totalSkills; i++)
        {
            if (!runtimeUnlockedSkillIndexes.Contains(i))
            {
                lockedIndexes.Add(i);
            }
        }

        if (lockedIndexes.Count == 0) return false;

        unlockedIndex = lockedIndexes[UnityEngine.Random.Range(0, lockedIndexes.Count)];
        runtimeUnlockedSkillIndexes.Add(unlockedIndex);
        return true;
    }

    private void EnsureRuntimeSkillUnlocksMatchLevel(int defaultUnlockedCount = DefaultUnlockedSkillCount)
    {
        int totalSkills = 0;

        if (selectedPlayerPreset != null && selectedPlayerPreset.allSkills != null)
        {
            totalSkills = selectedPlayerPreset.allSkills.Length;
        }
        else
        {
            totalSkills = runtimeSkillCount;
        }

        if (totalSkills <= 0) return;

        int targetUnlockedCount = GetTargetUnlockedSkillCountByLevel(totalSkills, defaultUnlockedCount);

        while (GetRuntimeUnlockedSkillCount(defaultUnlockedCount) < targetUnlockedCount)
        {
            bool unlocked = UnlockRandomLockedSkill(totalSkills, out int unlockedIndex, defaultUnlockedCount);
            if (!unlocked)
            {
                break;
            }

            SkillData unlockedSkill =
                selectedPlayerPreset != null && selectedPlayerPreset.allSkills != null && unlockedIndex >= 0 && unlockedIndex < selectedPlayerPreset.allSkills.Length
                    ? selectedPlayerPreset.allSkills[unlockedIndex]
                    : null;

            string skillName = unlockedSkill != null ? unlockedSkill.skillName : $"Index {unlockedIndex}";
            Debug.Log($"[PlayerState] Runtime skill unlocked: {skillName} (Lv.{PlayerLevel})");
        }
    }

    // --- Other Methods ---

    public void DropCard()
    {
        // ใส่ Logic ทิ้งการ์ดที่นี่
    }

    public void DropStar()
    {
        // ใส่ Logic ทิ้งดาวที่นี่
    }

    public void CleanseBurn()
    {
        // ถ้า false อยู่แล้ว (ไม่ติด Burn) จะไม่เกิดอะไรขึ้น
        if (!DebuffBurn) 
        {
            Debug.Log("[PlayerState] ไม่ได้ติดสถานะ Burn อยู่แล้ว การ์ดไม่ส่งผลอะไร");
            return; 
        }

        // ถ้า true ให้ล้างสถานะและรีเซ็ตค่าต่างๆ ที่เกี่ยวข้อง
        DebuffBurn = false;
        DebuffBurnTurnsRemaining = 0;
        burnDebuffAppliedOrder = 0; // รีเซ็ต order ด้วยเพื่อความชัวร์
        
        OnStatsUpdated?.Invoke(); // อัปเดต UI
        Debug.Log("<color=green>[PlayerState] ✨ ล้างสถานะ Burn สำเร็จ!</color>");
    }

    public void CleanseIce()
    {
        // ถ้า false อยู่แล้ว (ไม่ติด Ice) จะไม่เกิดอะไรขึ้น
        if (!hasIceEffect) 
        {
            Debug.Log("[PlayerState] ไม่ได้ติดสถานะ Ice อยู่แล้ว การ์ดไม่ส่งผลอะไร");
            return; 
        }

        // ถ้า true ให้ล้างสถานะและรีเซ็ตลำดับ
        hasIceEffect = false;
        iceDebuffAppliedOrder = 0; 
        
        OnStatsUpdated?.Invoke(); // อัปเดต UI ให้ไอคอนน้ำแข็งหายไป
        Debug.Log("<color=cyan>[PlayerState] ❄️ ล้างสถานะแช่แข็ง (Ice) สำเร็จ!</color>");
    }
}
