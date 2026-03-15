﻿using UnityEngine;

public class PlayerStatAggregator : MonoBehaviour
{
    [SerializeField] private PassiveSkillManager passiveSkillManager;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private PlayerDataManager playerDataManager;

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
        ResolvePlayerDataManager();
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

    private PlayerDataManager ResolvePlayerDataManager()
    {
        if (playerDataManager == null)
            playerDataManager = FindFirstObjectByType<PlayerDataManager>();

        return playerDataManager;
    }

    private EquipmentStatTotals GetEquippedStatTotals()
    {
        PlayerDataManager dataManager = ResolvePlayerDataManager();
        if (dataManager == null || dataManager.equippedItems == null)
            return default;

        EquipmentStatTotals totals = new EquipmentStatTotals();
        for (int i = 0; i < dataManager.equippedItems.Length; i++)
        {
            EquipmentData item = dataManager.equippedItems[i];
            if (item == null) continue;

            totals.attackBonus += item.attackBonus;
            totals.speedBonus += item.speedBonus;
            totals.defenseBonus += item.defenseBonus;
        }

        return totals;
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
        int passiveSpeedBonus = 0;
        int passiveDefenseBonus = 0;
        int equipmentAttackBonus = 0;
        int equipmentSpeedBonus = 0;
        int equipmentDefenseBonus = 0;

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
            passiveSpeedBonus += totals.speedBonus;
            passiveDefenseBonus += totals.defenseBonus;
        }

        EquipmentStatTotals equipmentTotals = GetEquippedStatTotals();
        equipmentAttackBonus += equipmentTotals.attackBonus;
        equipmentSpeedBonus += equipmentTotals.speedBonus;
        equipmentDefenseBonus += equipmentTotals.defenseBonus;

        int finalAttack = baseData.attackDamage + passiveAttackBonus + equipmentAttackBonus + player.RuntimeAttackModifier;
        int finalMaxHealth = Mathf.Max(1, baseData.maxHP + passiveMaxHealthBonus + player.RuntimeMaxHealthModifier);
        int finalStarBonus = Mathf.Max(0, passiveStarBonus);
        int finalSpeed = Mathf.Max(0, baseData.speed + passiveSpeedBonus + equipmentSpeedBonus);
        int finalDefense = Mathf.Max(0, baseData.def + passiveDefenseBonus + equipmentDefenseBonus);

        int previousMaxHealth = player.MaxHealth;

        player.CurrentAttack = finalAttack;
        player.MaxHealth = finalMaxHealth;
        player.CurrentSpeed = finalSpeed;
        player.CurrentDefense = finalDefense;

        int hpDelta = player.MaxHealth - previousMaxHealth;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);

        player.PassiveStarGainBonus = finalStarBonus;

        player.NotifyStatsUpdated();
    }
}

public struct SkillPassiveTotals
{
    public int attackBonus;
    public int maxHpBonus;
    public int starBonus;
    public int speedBonus;
    public int defenseBonus;
}

public struct EquipmentStatTotals
{
    public int attackBonus;
    public int speedBonus;
    public int defenseBonus;
}