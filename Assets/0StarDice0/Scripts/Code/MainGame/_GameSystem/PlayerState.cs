using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]


public class PlayerState : MonoBehaviour
{
    private const string IntermissionSceneName = "InterMission";

    //public static PlayerState Instance { get; private set; }
    [Header("AI Settings")]
    public bool isAI = false;
    [Header("Player Stats")]
    public int PlayerHealth;   // เปลี่ยนจาก Property เป็น Field เพื่อให้เห็นใน Inspector
    public int MaxHealth;      // ✅ เพิ่ม: เพื่อใช้คุมเพดานเลือด
    public int PlayerCredit = 0;
    public int PlayerStar = 0;
    public int CurrentAttack;
    public bool DebuffBurn = false;
    public int DebuffBurnTurnsRemaining = 0;
    public bool hasIceEffect = false;
    public ElementType PlayerElement { get; set; }

    [Header("Battle Stats")]
    public int WinCount = 0;
    

    [Header("Level System")]   // ✅ เพิ่ม: ระบบเลเวล
    public int PlayerLevel = 1;
    public int CurrentExp = 0;
    public int MaxExp = 100;

    [Header("Data & Inventory")]
    public PlayerData selectedPlayerPreset { get; private set; }
    public List<CardData> selectedCards { get; private set; } = new List<CardData>();

    // Snapshot ของค่าถาวรจาก PlayerData (ใช้รีเซ็ตเมื่อออกจาก Board)
    private int persistentLevelSnapshot = 1;
    private int persistentCurrentExpSnapshot = 0;
    private int persistentMaxExpSnapshot = 100;

    // Runtime skill unlock state (ใช้เฉพาะรอบ Boardgame เท่านั้น)
    private readonly HashSet<int> runtimeUnlockedSkillIndexes = new HashSet<int>();
    private int runtimeSkillCount = 0;
    private const int DefaultUnlockedSkillCount = 3;

    public event System.Action OnDied;
    public event Action OnStatsUpdated;
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

    public void LoadFromPlayerData(PlayerData data)
    {
        if (data == null) return;
        selectedPlayerPreset = data;

        // 1. โหลดค่าพลังชีวิต
        MaxHealth = data.GetMaxHealth();
        PlayerHealth = MaxHealth;
        CurrentAttack = data.attackDamage;
        // 2. โหลดเครดิต (ถ้าต้องการใช้ค่าเริ่มต้นจาก Data)
        PlayerCredit = data.Credit;

        // 3. ✅ โหลดข้อมูล Level จาก PlayerData
        PlayerLevel = data.level;
        CurrentExp = data.currentExp;
        MaxExp = data.maxExp;

        // กันเหนียว: ถ้า MaxExp เป็น 0 ให้ตั้งค่าเริ่มต้น
        if (MaxExp <= 0) MaxExp = 100;

        CachePersistentProgressSnapshot(data);
        InitializeRuntimeSkillUnlocks(data.allSkills != null ? data.allSkills.Length : 0);
        EnsureRuntimeSkillUnlocksMatchLevel();

        Debug.Log($"[PlayerState] Loaded: Level {PlayerLevel}, HP {PlayerHealth}/{MaxHealth}");
    }

    public void SetSelectedCards(List<CardData> cards)
    {
        selectedCards = new List<CardData>(cards);
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
        OnStatsUpdated?.Invoke();
        return true;
    }

    public void ApplyBurnDebuff(int turns)
    {
        DebuffBurn = turns > 0;
        DebuffBurnTurnsRemaining = Mathf.Max(0, turns);
        OnStatsUpdated?.Invoke();
    }

    public bool TryConsumeBurnDebuff(int burnDamage)
    {
        if (!DebuffBurn || DebuffBurnTurnsRemaining <= 0) return false;

        TakeDamage(burnDamage);

        DebuffBurnTurnsRemaining = Mathf.Max(0, DebuffBurnTurnsRemaining - 1);
        DebuffBurn = DebuffBurnTurnsRemaining > 0;
        OnStatsUpdated?.Invoke();
        return true;
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
        MaxHealth += 20;
        PlayerHealth = MaxHealth; // เลือดเด้งเต็ม
        CurrentAttack += 2;          // ตีแรงขึ้น

        Debug.Log($"💪 RPG LEVEL UP! Lv.{PlayerLevel} (HP: {MaxHealth}, ATK: {CurrentAttack})");

        // ถ้า EXP ยังเหลือเฟือ ก็ให้เช็คเวลอัปซ้ำ
        if (CurrentExp >= MaxExp) LevelUpRPG();
    }

    public void RecordBattleWin()
    {
        WinCount++;
        
        GainExp(50);
    }

    private void CachePersistentProgressSnapshot(PlayerData data)
    {
        if (data == null) return;

        persistentLevelSnapshot = Mathf.Max(1, data.level);
        persistentCurrentExpSnapshot = Mathf.Max(0, data.currentExp);
        persistentMaxExpSnapshot = data.maxExp > 0 ? data.maxExp : 100;
    }

    private void RestorePersistentProgressToPlayerData()
    {
        if (selectedPlayerPreset == null) return;

        selectedPlayerPreset.level = persistentLevelSnapshot;
        selectedPlayerPreset.currentExp = persistentCurrentExpSnapshot;
        selectedPlayerPreset.maxExp = persistentMaxExpSnapshot;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isAI || scene.name != IntermissionSceneName) return;

        // เครดิตเป็นค่าถาวร -> sync กลับข้อมูลหลัก
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetCredit(PlayerCredit);
        }

        // เลเวล/EXP ในบอร์ดเป็นค่าชั่วคราว -> รีเซ็ตและย้ำข้อมูลใน PlayerData
        RestorePersistentProgressToPlayerData();
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
        OnStatsUpdated?.Invoke();
    }

    private void HandleDefeat()
    {
        if (isDefeatHandling || isAI) return;
        isDefeatHandling = true;

        // ✅ เก็บเครดิตสะสมทั้งหมดกลับข้อมูลหลัก เพื่อให้คงอยู่หลังออกจากด่าน
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetCredit(PlayerCredit);
        }

        // ✅ รีเซ็ตความก้าวหน้าภายในด่าน (เลเวล/EXP/Win) เมื่อแพ้
        ResetInStageProgress();

        Debug.Log($"[PlayerState] Defeated -> return to {IntermissionSceneName}");
        SceneManager.LoadScene(IntermissionSceneName);
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
            PlayerLevel = sourceData.level;
            CurrentExp = sourceData.currentExp;
            MaxExp = sourceData.maxExp > 0 ? sourceData.maxExp : 100;
            MaxHealth = sourceData.GetMaxHealth();
            PlayerHealth = MaxHealth;
            CurrentAttack = sourceData.attackDamage;
        }
        else
        {
            PlayerLevel = 1;
            CurrentExp = 0;
            MaxExp = Mathf.Max(MaxExp, 100);
            PlayerHealth = Mathf.Max(PlayerHealth, 1);
        }

        WinCount = 0;
        hasIceEffect = false;
        DebuffBurn = false;
        DebuffBurnTurnsRemaining = 0;

        if (sourceData != null && sourceData.allSkills != null)
        {
            InitializeRuntimeSkillUnlocks(sourceData.allSkills.Length);
            EnsureRuntimeSkillUnlocksMatchLevel();
        }
        else
        {
            ResetRuntimeSkillUnlocks();
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
}
