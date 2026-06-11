using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Battle/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string playerId;
    public string playerName;
    public ElementType element;
    public Sprite playerSprite;

    [Header("Base Combat Stats")]
    [Tooltip("Single source of truth for the character base max HP before level, passive, equipment, and runtime bonuses.")]
    public int maxHP = 100;
    [Tooltip("Single source of truth for the character base ATK before level, passive, equipment, and runtime bonuses.")]
    public int attackDamage = 10;
    [Tooltip("Single source of truth for the character base SPD before level, passive, equipment, and runtime bonuses.")]
    public int speed = 10;
    [Tooltip("Single source of truth for the character base DEF before level, passive, equipment, and runtime bonuses.")]
    public int def = 1;

    [Header("Legacy Base Stat Mirrors")]
    [SerializeField, HideInInspector]
    private int maxHPbase = 100;
    [SerializeField, HideInInspector]
    private int attackDamagebase = 10;
    [SerializeField, HideInInspector]
    private int speedbase = 10;
    [SerializeField, HideInInspector]
    private int defbase = 1;

    public SkillData[] skills = new SkillData[3];
    public SkillData[] allSkills = new SkillData[10];
    public ElementType elementType;

    [Header("Player Stats")]
    [SerializeField, HideInInspector]
    private int maxHealth = 100;
    [FormerlySerializedAs("currentHealth")]
    [SerializeField, HideInInspector]
    private int legacyCurrentHealth;

    [Header("Persistent Progress Defaults")]
    [FormerlySerializedAs("level")]
    public int startingLevel = 1;
    [FormerlySerializedAs("currentExp")]
    public int startingCurrentExp = 0;
    [FormerlySerializedAs("maxExp")]
    public int startingMaxExp = 100;
    [FormerlySerializedAs("credit")]
    public int startingCredit = 0;

    public event Action<int> OnCreditChanged;

    [FormerlySerializedAs("turnsToSkip")]
    [SerializeField, HideInInspector]
    private int legacyTurnsToSkip = 0;

    public int Credit
    {
        get => ResolveProgress()?.Credit ?? Mathf.Max(0, startingCredit);
        set
        {
            PlayerProgress progress = ResolveOrCreateProgress();
            if (progress == null) return;
            progress.SetCredit(value);
        }
    }

    [Obsolete("Use PlayerProgress.Level or PlayerState.PlayerLevel instead.")]
    public int level
    {
        get => ResolveProgress()?.Level ?? Mathf.Max(1, startingLevel);
        set
        {
            PlayerProgress progress = ResolveOrCreateProgress();
            if (progress == null) return;
            progress.SetLevelProgress(value, currentExp, maxExp);
        }
    }

    [Obsolete("Use PlayerProgress.CurrentExp or PlayerState.CurrentExp instead.")]
    public int currentExp
    {
        get => ResolveProgress()?.CurrentExp ?? Mathf.Max(0, startingCurrentExp);
        set
        {
            PlayerProgress progress = ResolveOrCreateProgress();
            if (progress == null) return;
            progress.SetLevelProgress(level, value, maxExp);
        }
    }

    [Obsolete("Use PlayerProgress.MaxExp or PlayerState.MaxExp instead.")]
    public int maxExp
    {
        get => ResolveProgress()?.MaxExp ?? Mathf.Max(1, startingMaxExp);
        set
        {
            PlayerProgress progress = ResolveOrCreateProgress();
            if (progress == null) return;
            progress.SetLevelProgress(level, currentExp, value);
        }
    }

    [Obsolete("PlayerData no longer stores runtime HP. Use PlayerState.PlayerHealth instead.")]
    public int CurrentHealth => GetMaxHealth();

    private void OnEnable()
    {
        NormalizeBaseStatFields();
    }

    private void NormalizeBaseStatFields()
    {
        // KISS: keep one editable stat set (maxHP/attackDamage/speed/def) and mirror
        // legacy fields so old serialized assets keep loading without becoming a
        // second source of truth.
        if (maxHP <= 0 && maxHealth > 0)
        {
            maxHP = maxHealth;
        }
        if (maxHP <= 0 && maxHPbase > 0)
        {
            maxHP = maxHPbase;
        }
        if (attackDamage <= 0 && attackDamagebase > 0)
        {
            attackDamage = attackDamagebase;
        }
        if (speed <= 0 && speedbase > 0)
        {
            speed = speedbase;
        }
        if (def <= 0 && defbase > 0)
        {
            def = defbase;
        }

        maxHP = Mathf.Max(1, maxHP);
        attackDamage = Mathf.Max(0, attackDamage);
        speed = Mathf.Max(0, speed);
        def = Mathf.Max(0, def);

        maxHealth = maxHP;
        maxHPbase = maxHP;
        attackDamagebase = attackDamage;
        speedbase = speed;
        defbase = def;

        startingLevel = Mathf.Max(1, startingLevel);
        startingCurrentExp = Mathf.Max(0, startingCurrentExp);
        startingMaxExp = Mathf.Max(1, startingMaxExp);
        startingCredit = Mathf.Max(0, startingCredit);
    }

    private void OnValidate()
    {
        NormalizeBaseStatFields();
    }

    private PlayerProgress ResolveProgress()
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer == this)
        {
            GameData.Instance.EnsureSelectedPlayerProgressLoaded();
            return GameData.Instance.SelectedPlayerProgress;
        }

        return PlayerProgressService.LoadForPlayer(this);
    }

    private PlayerProgress ResolveOrCreateProgress()
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer == this)
        {
            GameData.Instance.EnsureSelectedPlayerProgressLoaded();
            return GameData.Instance.SelectedPlayerProgress;
        }

        return PlayerProgressService.LoadForPlayer(this);
    }

    public int GetMaxHealth()
    {
        return Mathf.Max(1, maxHP);
    }

    public int GetBaseAttack()
    {
        return Mathf.Max(0, attackDamage);
    }

    public int GetBaseSpeed()
    {
        return Mathf.Max(0, speed);
    }

    public int GetBaseDefense()
    {
        return Mathf.Max(0, def);
    }

    internal void NotifyCreditChangedFromProgress(int newCredit)
    {
        OnCreditChanged?.Invoke(Mathf.Max(0, newCredit));
    }

    [Obsolete("PlayerData should not store runtime HP. Update PlayerState.PlayerHealth instead.")]
    public void SetHealth(int newHealth)
    {
        Debug.LogWarning($"[PlayerData] Ignored SetHealth({newHealth}) on {playerName}. Runtime HP now belongs to PlayerState.");
    }

    public void SetCredit(int newAmount)
    {
        PlayerProgress progress = ResolveOrCreateProgress();
        if (progress == null) return;
        int previousCredit = progress.Credit;
        progress.SetCredit(newAmount);
        if (previousCredit != progress.Credit)
        {
            NotifyCreditChangedFromProgress(progress.Credit);
        }
    }

    public void AddCredit(int amount)
    {
        if (amount <= 0) return;
        SetCredit(Credit + amount);
    }

    public bool TrySpendCredit(int amount)
    {
        if (amount <= 0) return true;

        PlayerProgress progress = ResolveOrCreateProgress();
        if (progress == null) return false;
        if (!progress.TrySpendCredit(amount)) return false;

        NotifyCreditChangedFromProgress(progress.Credit);
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
