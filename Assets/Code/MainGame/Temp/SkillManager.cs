using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public HashSet<string> unlockedSkillIDs = new HashSet<string>();

    public int defaultSkillPoints = 5; // เก็บไว้เผื่อระบบเก่า
    private int appliedPassiveStarBonus = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        PlayerState player = GameTurnManager.CurrentPlayer;
        if (player == null || player.PlayerMoney < skill.costPoint) return false;

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

        PlayerState player = GameTurnManager.CurrentPlayer;
        player.PlayerMoney -= skill.costPoint;

        if (GameData.Instance?.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetMoney(player.PlayerMoney);
        }

        unlockedSkillIDs.Add(skill.skillID);

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

    public System.Action OnSkillTreeUpdated;
}
