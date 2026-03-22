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

    [Tooltip("Primary max HP value used by runtime systems.")]
    public int maxHP = 100;
    public int attackDamage = 10;
    public int speed = 10;
    public int def = 1;
    public SkillData[] skills = new SkillData[3];
    public SkillData[] allSkills = new SkillData[10];
    public ElementType elementType;

    [Header("Player Stats")]
    [Obsolete("Use maxHP as the source of truth.")]
    public int maxHealth = 100;
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
        NormalizeMaxHpFields();
    }

    private void NormalizeMaxHpFields()
    {
        if (maxHP <= 0 && maxHealth > 0)
        {
            maxHP = maxHealth;
        }

        maxHP = Mathf.Max(1, maxHP);
        maxHealth = maxHP;
        startingLevel = Mathf.Max(1, startingLevel);
        startingCurrentExp = Mathf.Max(0, startingCurrentExp);
        startingMaxExp = Mathf.Max(1, startingMaxExp);
        startingCredit = Mathf.Max(0, startingCredit);
    }

    private void OnValidate()
    {
        NormalizeMaxHpFields();
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
