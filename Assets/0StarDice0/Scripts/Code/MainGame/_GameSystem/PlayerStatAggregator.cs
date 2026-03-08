using UnityEngine;

public class PlayerStatAggregator : MonoBehaviour
{
    public static PlayerStatAggregator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

        if (PassiveSkillManager.Instance != null)
        {
            passiveAttackBonus += PassiveSkillManager.Instance.GetAttackBonusAmount();
            passiveMaxHealthBonus += PassiveSkillManager.Instance.GetStarBonusAmount();
        }

        if (SkillManager.Instance != null)
        {
            SkillPassiveTotals totals = SkillManager.Instance.GetUnlockedPassiveTotals();
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
