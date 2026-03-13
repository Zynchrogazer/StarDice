using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Battle/PlayerData")]

public class PlayerData : ScriptableObject
{
    private const string CreditSaveKeyPrefix = "PLAYER_CREDIT_";

    public string playerName;
    public ElementType element;
    public Sprite playerSprite;

    [Tooltip("Primary max HP value used by runtime systems.")]
    public int maxHP = 100;
    public int attackDamage = 100;
    public int speed = 10;
    public int def = 1;
    public SkillData[] skills = new SkillData[3]; // 3 สกิลพิเศษ
    public SkillData[] allSkills = new SkillData[10]; //สกิลทั้งหมด
    public ElementType elementType;


    [Header("Player Stats")]
    [Obsolete("Use maxHP as the source of truth.")]
    public int maxHealth = 100;
    [FormerlySerializedAs("currentHealth")]
    [SerializeField, HideInInspector]
    private int legacyCurrentHealth;

    [Header("Level System")]
    public int level = 1;      // เลเวลเริ่มต้น
    public int currentExp = 0; // EXP เริ่มต้น
    public int maxExp = 100;   // EXP ที่ต้องใช้ในการอัปเวลครั้งแรก
    // ------------------------------------

    [SerializeField] private int credit = 50;
    

    public event Action<int> OnCreditChanged;
    public event Action OnDied;

    [FormerlySerializedAs("turnsToSkip")]
    [SerializeField, HideInInspector]
    private int legacyTurnsToSkip = 0;

    public int Credit
    {
        get => credit;
        set
        {
            if (credit == value) return;
            credit = Mathf.Max(0, value);
            SaveCredit();
            OnCreditChanged?.Invoke(credit);
        }
    }

    [Obsolete("PlayerData no longer stores runtime HP. Use PlayerState.PlayerHealth instead.")]
    public int CurrentHealth => GetMaxHealth();

    private void OnEnable()
    {
        NormalizeMaxHpFields();
        LoadCredit();
    }

    private void NormalizeMaxHpFields()
    {
        // maxHP is the source of truth. Keep legacy maxHealth synchronized for compatibility.
        if (maxHP <= 0 && maxHealth > 0)
        {
            maxHP = maxHealth;
        }

        maxHP = Mathf.Max(1, maxHP);
        maxHealth = maxHP;
    }

    private void OnValidate()
    {
        NormalizeMaxHpFields();
    }

    private string GetCreditSaveKey()
    {
        string playerKey = string.IsNullOrEmpty(playerName) ? name : playerName;
        return $"{CreditSaveKeyPrefix}{playerKey}";
    }

    private void LoadCredit()
    {
        string saveKey = GetCreditSaveKey();
        if (!PlayerPrefs.HasKey(saveKey))
        {
            return;
        }

        credit = Mathf.Max(0, PlayerPrefs.GetInt(saveKey, credit));
    }

    private void SaveCredit()
    {
        PlayerPrefs.SetInt(GetCreditSaveKey(), credit);
        PlayerPrefs.Save();
    }

    private void Die()
    {
        Debug.LogError($"[PlayerData] {playerName} has died!");
        // TODO: Game over logic, show UI, trigger event ฯลฯ
    }

    public int GetMaxHealth()
    {
        return Mathf.Max(1, maxHP);
    }

    [Obsolete("PlayerData should not store runtime HP. Update PlayerState.PlayerHealth instead.")]
    public void SetHealth(int newHealth)
    {
        Debug.LogWarning($"[PlayerData] Ignored SetHealth({newHealth}) on {playerName}. Runtime HP now belongs to PlayerState.");
    }

    /// <summary>
    /// เมธอดสำหรับตั้งค่าเครดิตโดยตรง และเรียก Event
    /// </summary>
    public void SetCredit(int newAmount)
    {
        this.Credit = newAmount; // ใช้ Property เพื่อให้ Event OnCreditChanged ทำงาน
    }

    public void AddCredit(int amount)
    {
        if (amount <= 0) return;
        Credit += amount;
    }

    public bool TrySpendCredit(int amount)
    {
        if (amount <= 0) return true;
        if (Credit < amount) return false;

        Credit -= amount;
        return true;
    }

    [Obsolete("Use PlayerState runtime unlock methods for per-run skill state.")]
    public void ResetSkillLocksForStageStart(int initiallyUnlockedSkillCount = 3, int currentLevelOverride = -1)
    {
        if (allSkills == null) return;

        int levelToUse = currentLevelOverride >= 0 ? currentLevelOverride : level;
        int levelMilestoneUnlockCount = Mathf.Max(0, levelToUse / 10);
        int unlockedCount = Mathf.Max(0, initiallyUnlockedSkillCount + levelMilestoneUnlockCount);
        unlockedCount = Mathf.Min(unlockedCount, allSkills.Length);

        for (int i = 0; i < allSkills.Length; i++)
        {
            SkillData skill = allSkills[i];
            if (skill == null) continue;

            skill.isLocked = i >= unlockedCount;
        }

        if (skills == null || allSkills.Length < 3 || skills.Length < 3) return;

        skills[0] = allSkills[0];
        skills[1] = allSkills[1];
        skills[2] = allSkills[2];
    }




}
