using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public HashSet<string> unlockedSkillIDs = new HashSet<string>();

    public int defaultSkillPoints = 5; // เก็บไว้เผื่อระบบเก่า
    private int appliedPassiveStarBonus = 0;

    private const string UnlockedSkillsSaveKey = "PassiveUnlockedSkills";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadUnlockedSkills();
    }


    private void Start()
    {
        ApplyAllPassiveBonusesToCurrentPlayer();
        OnSkillTreeUpdated?.Invoke();
    }

    public bool IsUnlocked(PassiveSkillData skill)
    {
        return skill != null && unlockedSkillIDs.Contains(skill.skillID);
    }

    public bool CanUnlock(PassiveSkillData skill)
    {
        if (skill == null) return false;
        if (IsUnlocked(skill)) return false;

        if (GetAvailableMoney() < skill.costPoint) return false;

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
        if (!CanUnlock(skill))
        {
            return false;
        }

        if (!TrySpendMoney(skill.costPoint))
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
        if (GameTurnManager.CurrentPlayer == null || GameData.Instance?.selectedPlayer == null)
        {
            return;
        }

        PlayerState player = GameTurnManager.CurrentPlayer;
        PlayerData data = GameData.Instance.selectedPlayer;

        int bonusAtk = 0;
        int bonusHp = 0;
        int bonusStar = 0;

        PassiveSkillData[] allSkills = Resources.LoadAll<PassiveSkillData>("");
        foreach (var passive in allSkills)
        {
            if (passive == null || !IsUnlocked(passive)) continue;
            bonusAtk += passive.bonusAttack;
            bonusHp += passive.bonusMaxHP;
            bonusStar += passive.bonusStar;
        }

        int oldMaxHp = player.MaxHealth;
        int oldAppliedStarBonus = appliedPassiveStarBonus;

        player.CurrentAttack = data.attackDamage + bonusAtk;
        player.MaxHealth = data.maxHP + bonusHp;
        int starDelta = bonusStar - oldAppliedStarBonus;
        player.PlayerStar = Mathf.Max(0, player.PlayerStar + starDelta);
        appliedPassiveStarBonus = bonusStar;

        int hpDelta = player.MaxHealth - oldMaxHp;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);
    }

    private int GetAvailableMoney()
    {
        if (GameTurnManager.CurrentPlayer != null)
        {
            return GameTurnManager.CurrentPlayer.PlayerMoney;
        }

        if (GameData.Instance?.selectedPlayer != null)
        {
            return GameData.Instance.selectedPlayer.Money;
        }

        return 0;
    }

    private bool TrySpendMoney(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (GameTurnManager.CurrentPlayer != null)
        {
            PlayerState player = GameTurnManager.CurrentPlayer;
            if (player.PlayerMoney < amount)
            {
                return false;
            }

            player.PlayerMoney -= amount;
            if (GameData.Instance?.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.SetMoney(player.PlayerMoney);
            }
            return true;
        }

        if (GameData.Instance?.selectedPlayer != null)
        {
            PlayerData selectedPlayer = GameData.Instance.selectedPlayer;
            if (selectedPlayer.Money < amount)
            {
                return false;
            }

            selectedPlayer.SetMoney(selectedPlayer.Money - amount);
            return true;
        }

        return false;
    }

    private void SaveUnlockedSkills()
    {
        string serializedSkills = string.Join("|", unlockedSkillIDs);
        PlayerPrefs.SetString(UnlockedSkillsSaveKey, serializedSkills);
        PlayerPrefs.Save();
    }

    private void LoadUnlockedSkills()
    {
        unlockedSkillIDs.Clear();

        string serializedSkills = PlayerPrefs.GetString(UnlockedSkillsSaveKey, string.Empty);
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

    public System.Action OnSkillTreeUpdated;
}
