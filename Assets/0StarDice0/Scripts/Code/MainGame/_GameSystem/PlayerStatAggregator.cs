using UnityEngine;

public class PlayerStatAggregator : MonoBehaviour
{
    [SerializeField] private PassiveSkillManager passiveSkillManager;
    [SerializeField] private SkillManager skillManager;

    private void Awake()
    {
        PlayerStatAggregator[] aggregators = FindObjectsByType<PlayerStatAggregator>(FindObjectsSortMode.None);
        if (aggregators.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        ResolveManagers();
    }

    private void ResolveManagers()
    {
        ResolvePassiveSkillManager();
        ResolveSkillManager();
    }

    private PassiveSkillManager ResolvePassiveSkillManager()
    {
        if (passiveSkillManager == null)
            passiveSkillManager = FindFirstObjectByType<PassiveSkillManager>();

        return passiveSkillManager;
    }

    private SkillManager ResolveSkillManager()
    {
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        return skillManager;
    }

    public void RefreshCurrentPlayerStats()
    {
        if (GameTurnManager.CurrentPlayer == null || GameData.Instance?.selectedPlayer == null)
        {
            return;
        }

        PlayerState player = GameTurnManager.CurrentPlayer;
        PlayerData baseData = GameData.Instance.selectedPlayer;

        int passiveAttackBonus = 0;
        int passiveMaxHealthBonus = 0;
        int passiveStarBonus = 0;

        PassiveSkillManager passiveManager = ResolvePassiveSkillManager();
        if (passiveManager != null)
        {
            passiveAttackBonus += passiveManager.GetAttackBonusAmount();
            passiveMaxHealthBonus += passiveManager.GetStarBonusAmount();
        }

        SkillManager resolvedSkillManager = ResolveSkillManager();
        if (resolvedSkillManager != null)
        {
            SkillPassiveTotals totals = resolvedSkillManager.GetUnlockedPassiveTotals();
            passiveAttackBonus += totals.attackBonus;
            passiveMaxHealthBonus += totals.maxHpBonus;
            passiveStarBonus += totals.starBonus;
        }

        int totalAttack = baseData.attackDamage + passiveAttackBonus + player.RuntimeAttackModifier;
        int totalMaxHealth = Mathf.Max(1, baseData.maxHP + passiveMaxHealthBonus + player.RuntimeMaxHealthModifier);
        int totalStarBonus = passiveStarBonus + player.RuntimeStarModifier;

        int previousMaxHealth = player.MaxHealth;

        player.CurrentAttack = totalAttack;
        player.MaxHealth = totalMaxHealth;

        int hpDelta = player.MaxHealth - previousMaxHealth;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);

        int starDelta = totalStarBonus - player.AppliedStarBonusTotal;
        player.PlayerStar = Mathf.Max(0, player.PlayerStar + starDelta);
        player.AppliedStarBonusTotal = totalStarBonus;

        player.NotifyStatsUpdated();
    }
}

public struct SkillPassiveTotals
{
    public int attackBonus;
    public int maxHpBonus;
    public int starBonus;
}
