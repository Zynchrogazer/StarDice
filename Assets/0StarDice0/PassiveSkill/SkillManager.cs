using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private PlayerStatAggregator playerStatAggregator;

    public HashSet<string> unlockedSkillIDs = new HashSet<string>();

    public int defaultSkillPoints = 5; // เก็บไว้เผื่อระบบเก่า
    private int fallbackAppliedStarBonus = 0;

    private const string UnlockedSkillsSaveKeyPrefix = "PassiveUnlockedSkills";
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

        if (skill.requiredSkills != null)
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

    public void ApplyAllPassiveBonusesToCurrentPlayer()
    {
        if (ResolvePlayerStatAggregator() != null)
        {
            ResolvePlayerStatAggregator().RefreshCurrentPlayerStats();
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
        PassiveSkillData[] allSkills = Resources.LoadAll<PassiveSkillData>("");
        foreach (var passive in allSkills)
        {
            if (passive == null || !IsUnlocked(passive)) continue;
            totals.attackBonus += passive.bonusAttack;
            totals.maxHpBonus += passive.bonusMaxHP;
            totals.starBonus += passive.bonusStar;
        }

        return totals;
    }

    private int GetAvailableCredit()
    {
        if (GameTurnManager.CurrentPlayer != null)
        {
            return GameTurnManager.CurrentPlayer.PlayerCredit;
        }

        if (GameData.Instance?.selectedPlayer != null)
        {
            return GameData.Instance.selectedPlayer.Credit;
        }

        return 0;
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
            if (GameData.Instance?.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.SetCredit(player.PlayerCredit);
            }
            return true;
        }

        if (GameData.Instance?.selectedPlayer != null)
        {
            PlayerData selectedPlayer = GameData.Instance.selectedPlayer;
            if (selectedPlayer.Credit < amount)
            {
                return false;
            }

            selectedPlayer.SetCredit(selectedPlayer.Credit - amount);
            return true;
        }

        return false;
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
        string fallback = "default";
        string playerKey = GameData.Instance?.selectedPlayer != null
            ? (!string.IsNullOrWhiteSpace(GameData.Instance.selectedPlayer.playerName)
                ? GameData.Instance.selectedPlayer.playerName
                : GameData.Instance.selectedPlayer.name)
            : fallback;

        return $"{UnlockedSkillsSaveKeyPrefix}_{playerKey}";
    }

    private PlayerStatAggregator ResolvePlayerStatAggregator()
    {
        if (playerStatAggregator == null)
            playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();

        return playerStatAggregator;
    }

    public System.Action OnSkillTreeUpdated;
}
