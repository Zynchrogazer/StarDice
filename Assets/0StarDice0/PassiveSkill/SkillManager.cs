using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private PlayerStatAggregator playerStatAggregator;

    public HashSet<string> unlockedSkillIDs = new HashSet<string>();

    public int defaultSkillPoints = 5; /// เก็บไว้เผื่อระบบเก่า
    private int fallbackAppliedStarBonus = 0;

    private const string UnlockedSkillsSaveKey = "PassiveUnlockedSkills_SHARED";
    private string loadedSaveKey = string.Empty;

    private void Awake()
    {
        ResolvePlayerStatAggregator();
        EnsureLoadedForCurrentPlayer();
    }


    private void Start()
    {
        EnsureLoadedForCurrentPlayer();
        ApplyAllPassiveBonusesToCurrentPlayer();
        OnSkillTreeUpdated?.Invoke();
    }

    public bool IsUnlocked(PassiveSkillData skill)
    {
        EnsureLoadedForCurrentPlayer();
        return skill != null && unlockedSkillIDs.Contains(skill.skillID);
    }

    public bool CanUnlock(PassiveSkillData skill)
    {
        EnsureLoadedForCurrentPlayer();
        if (skill == null) return false;
        if (IsUnlocked(skill)) return false;

        if (GetAvailableCredit() < skill.costPoint) return false;

        if (skill.useRequiredSkills && skill.requiredSkills != null)
        {
            foreach (var req in skill.requiredSkills)
            {
                if (req != null && !IsUnlocked(req))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool TryUnlockSkill(PassiveSkillData skill)
    {
        EnsureLoadedForCurrentPlayer();
        if (!CanUnlock(skill))
        {
            return false;
        }

        if (!TrySpendCredit(skill.costPoint))
        {
            return false;
        }

        unlockedSkillIDs.Add(skill.skillID);
        SaveUnlockedSkills();

        ApplyAllPassiveBonusesToCurrentPlayer();

        OnSkillTreeUpdated?.Invoke();
        return true;
    }

    public bool CanRefundSkill(PassiveSkillData skill)
    {
        EnsureLoadedForCurrentPlayer();
        if (skill == null || !IsUnlocked(skill)) return false;

        PassiveSkillData[] allSkills = PassiveSkillCatalog.GetAll();
        for (int i = 0; i < allSkills.Length; i++)
        {
            PassiveSkillData candidate = allSkills[i];
            if (candidate == null || SkillIdsEqual(candidate, skill) || !IsUnlocked(candidate))
            {
                continue;
            }

            if (!candidate.useRequiredSkills || candidate.requiredSkills == null)
            {
                continue;
            }

            for (int reqIndex = 0; reqIndex < candidate.requiredSkills.Count; reqIndex++)
            {
                if (SkillIdsEqual(candidate.requiredSkills[reqIndex], skill))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool TryRefundSkill(PassiveSkillData skill)
    {
        EnsureLoadedForCurrentPlayer();
        if (!CanRefundSkill(skill))
        {
            return false;
        }

        if (!unlockedSkillIDs.Remove(skill.skillID))
        {
            return false;
        }

        RefundCredit(skill.costPoint);
        SaveUnlockedSkills();
        ApplyAllPassiveBonusesToCurrentPlayer();

        OnSkillTreeUpdated?.Invoke();
        return true;
    }

    public void ApplyAllPassiveBonusesToCurrentPlayer()
    {
        PlayerStatAggregator aggregator = ResolvePlayerStatAggregator();
        if (aggregator != null)
        {
            aggregator.RefreshCurrentPlayerStats();
            return;
        }

        if (GameTurnManager.CurrentPlayer == null || GameData.Instance?.selectedPlayer == null)
        {
            return;
        }

        PlayerState player = GameTurnManager.CurrentPlayer;
        PlayerData data = GameData.Instance.selectedPlayer;
        SkillPassiveTotals totals = GetUnlockedPassiveTotals();

        int oldMaxHp = player.MaxHealth;

        player.CurrentAttack = data.attackDamage + totals.attackBonus;
        player.MaxHealth = data.maxHP + totals.maxHpBonus;
        int starDelta = totals.starBonus - fallbackAppliedStarBonus;
        player.PlayerStar = Mathf.Max(0, player.PlayerStar + starDelta);
        fallbackAppliedStarBonus = totals.starBonus;

        int hpDelta = player.MaxHealth - oldMaxHp;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);
    }

    public SkillPassiveTotals GetUnlockedPassiveTotals()
    {
        EnsureLoadedForCurrentPlayer();

        SkillPassiveTotals totals = new SkillPassiveTotals();
        PassiveSkillData[] allSkills = PassiveSkillCatalog.GetAll();
        foreach (var passive in allSkills)
        {
            if (passive == null || !IsUnlocked(passive)) continue;
            totals.attackBonus += passive.bonusAttack;
            totals.maxHpBonus += passive.bonusMaxHP;
            totals.starBonus += passive.bonusStar;
            totals.speedBonus += passive.bonusSpeed;
            totals.defenseBonus += passive.bonusDefense;
        }

        return totals;
    }

    private int GetAvailableCredit()
    {
        if (GameTurnManager.CurrentPlayer != null)
        {
            return GameTurnManager.CurrentPlayer.PlayerCredit;
        }

        // The upgrade scene can be opened without a selected monster. Credit is stored in
        // PlayerProgress as a shared wallet, so read through the service even when
        // GameData.Instance or GameData.selectedPlayer is missing. This lets slots still
        // show whether each skill is affordable/unlockable.
        return PlayerProgressService.GetSelectedPlayerCredit(GameData.Instance);
    }

    private bool TrySpendCredit(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (GameTurnManager.CurrentPlayer != null)
        {
            PlayerState player = GameTurnManager.CurrentPlayer;
            if (player.PlayerCredit < amount)
            {
                return false;
            }

            player.PlayerCredit -= amount;
            return true;
        }

        // Spend from the same shared wallet used by the HUD/upgrade credit text.
        // PlayerProgressService falls back to PlayerProgress.TrySpendSharedCredit when
        // no monster has been selected, so upgrades still work from Upgrade.unity.
        return PlayerProgressService.TrySpendSelectedPlayerCredit(GameData.Instance, amount);
    }

    private void RefundCredit(int amount)
    {
        if (amount <= 0) return;

        if (GameTurnManager.CurrentPlayer != null)
        {
            GameTurnManager.CurrentPlayer.PlayerCredit += amount;
            return;
        }

        PlayerProgressService.AddSelectedPlayerCredit(GameData.Instance, amount);
    }

    private static bool SkillIdsEqual(PassiveSkillData a, PassiveSkillData b)
    {
        return a != null && b != null && a.skillID == b.skillID;
    }

    private void SaveUnlockedSkills()
    {
        loadedSaveKey = GetUnlockedSkillsSaveKey();
        string serializedSkills = string.Join("|", unlockedSkillIDs);
        PlayerPrefs.SetString(loadedSaveKey, serializedSkills);
        PlayerPrefs.Save();
    }

    private void LoadUnlockedSkills()
    {
        unlockedSkillIDs.Clear();

        loadedSaveKey = GetUnlockedSkillsSaveKey();
        string serializedSkills = PlayerPrefs.GetString(loadedSaveKey, string.Empty);
        if (string.IsNullOrEmpty(serializedSkills))
        {
            return;
        }

        string[] split = serializedSkills.Split('|');
        foreach (string skillID in split)
        {
            if (!string.IsNullOrWhiteSpace(skillID))
            {
                unlockedSkillIDs.Add(skillID);
            }
        }
    }

    private void EnsureLoadedForCurrentPlayer()
    {
        string targetKey = GetUnlockedSkillsSaveKey();
        if (targetKey == loadedSaveKey) return;
        LoadUnlockedSkills();
    }

    private string GetUnlockedSkillsSaveKey()
    {
        return UnlockedSkillsSaveKey;
    }

    public static void ClearSavedUnlockedSkills()
    {
        PlayerPrefs.DeleteKey(UnlockedSkillsSaveKey);

        SkillManager[] managers = FindObjectsByType<SkillManager>(FindObjectsSortMode.None);
        for (int i = 0; i < managers.Length; i++)
        {
            SkillManager manager = managers[i];
            if (manager == null)
            {
                continue;
            }

            manager.unlockedSkillIDs.Clear();
            manager.fallbackAppliedStarBonus = 0;
            manager.loadedSaveKey = string.Empty;
            manager.ApplyAllPassiveBonusesToCurrentPlayer();
            manager.OnSkillTreeUpdated?.Invoke();
        }
    }

    private PlayerStatAggregator ResolvePlayerStatAggregator()
    {
        if (playerStatAggregator == null)
            playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();

        return playerStatAggregator;
    }

    public System.Action OnSkillTreeUpdated;
}
