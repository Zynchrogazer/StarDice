﻿using UnityEngine;

public class PlayerStatAggregator : MonoBehaviour
{
    private const string UnlockedSkillsSaveKey = "PassiveUnlockedSkills_SHARED";

    public static event System.Action<PlayerStatAggregator> OnAggregatorAvailable;

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
        OnAggregatorAvailable?.Invoke(this);
    }


    private void OnEnable()
    {
        GameEventManager.OnBoardSceneReady += RefreshStatsAfterBoardReady;
    }

    private void OnDisable()
    {
        GameEventManager.OnBoardSceneReady -= RefreshStatsAfterBoardReady;
    }

    private void RefreshStatsAfterBoardReady()
    {
        // รองรับ flow RuntimeHub + additive scene: กลับเข้า board แล้วคำนวณจาก save ล่าสุดทันที
        RefreshCurrentPlayerStats();
    }

    private void ResolveManagers()
    {
        ResolveSkillManager();
        ResolvePlayerDataManager();
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
        PlayerState player = GameTurnManager.CurrentPlayer;
        if (player == null)
            return;

        RefreshPlayerStats(player, GameData.Instance != null ? GameData.Instance.selectedPlayer : null);
    }

    public void RefreshPlayerStats(PlayerState player, PlayerData explicitBaseData = null)
    {
        if (player == null)
            return;

        PlayerData baseData = explicitBaseData;
        if (baseData == null)
            baseData = player.selectedPlayerPreset;
        if (baseData == null && GameData.Instance != null)
            baseData = GameData.Instance.selectedPlayer;
        if (baseData == null)
            return;

        SkillPassiveTotals unlockedSkillTotals = ResolveUnlockedSkillTotals();
        EquipmentStatTotals equipmentTotals = GetEquippedStatTotals();

       // 🟢 เปลี่ยนสูตรคำนวณใหม่: เอาโบนัสจากเลเวล (player.GetLevelBonus...) มาบวกเข้าไปด้วย!
        int finalAttack = baseData.attackDamage
            + unlockedSkillTotals.attackBonus
            + equipmentTotals.attackBonus
            + player.RuntimeAttackModifier
            + player.GetLevelBonusAttack();
        
        int finalMaxHealth = Mathf.Max(1, baseData.maxHP
            + unlockedSkillTotals.maxHpBonus
            + player.RuntimeMaxHealthModifier
            + player.GetLevelBonusMaxHealth());
        
        int finalStarBonus = Mathf.Max(0, unlockedSkillTotals.starBonus);
        
        int finalSpeed = Mathf.Max(0, baseData.speed + unlockedSkillTotals.speedBonus + equipmentTotals.speedBonus + player.GetLevelBonusSpeed());
        
        int finalDefense = Mathf.Max(0, baseData.def + unlockedSkillTotals.defenseBonus + equipmentTotals.defenseBonus + player.GetLevelBonusDefense());


        int previousMaxHealth = player.MaxHealth;

        // อัปเดตค่าพลังทั้งหมดกลับไปที่ Player
        player.CurrentAttack = finalAttack;
        player.MaxHealth = finalMaxHealth;
        player.CurrentSpeed = finalSpeed;
        player.CurrentDefense = finalDefense;

        // คำนวณส่วนต่างของเลือด เพื่อไม่ให้เลือดเด้งเต็มหรือหดแปลกๆ เวลาเปลี่ยนของ
        int hpDelta = player.MaxHealth - previousMaxHealth;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);

        player.PassiveStarGainBonus = finalStarBonus;

        player.NotifyStatsUpdated();
    }

    private SkillPassiveTotals ResolveUnlockedSkillTotals()
    {
        // ถ้า SkillManager อยู่คนละ scene/ถูก unload ให้ fallback ไปอ่านจาก save เสมอ
        SkillManager resolvedSkillManager = ResolveSkillManager();
        if (resolvedSkillManager != null)
            return resolvedSkillManager.GetUnlockedPassiveTotals();

        return LoadUnlockedPassiveTotalsFromSave();
    }

    private static SkillPassiveTotals LoadUnlockedPassiveTotalsFromSave()
    {
        SkillPassiveTotals totals = new SkillPassiveTotals();
        string serializedSkills = PlayerPrefs.GetString(UnlockedSkillsSaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(serializedSkills))
            return totals;

        string[] unlockedSkillIds = serializedSkills.Split('|');
        if (unlockedSkillIds == null || unlockedSkillIds.Length == 0)
            return totals;

        System.Collections.Generic.HashSet<string> unlockedSet = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < unlockedSkillIds.Length; i++)
        {
            string skillId = unlockedSkillIds[i];
            if (!string.IsNullOrWhiteSpace(skillId))
                unlockedSet.Add(skillId);
        }

        if (unlockedSet.Count == 0)
            return totals;

        PassiveSkillData[] allSkills = PassiveSkillCatalog.GetAll();
        for (int i = 0; i < allSkills.Length; i++)
        {
            PassiveSkillData passive = allSkills[i];
            if (passive == null || !unlockedSet.Contains(passive.skillID))
                continue;

            totals.attackBonus += passive.bonusAttack;
            totals.maxHpBonus += passive.bonusMaxHP;
            totals.starBonus += passive.bonusStar;
            totals.speedBonus += passive.bonusSpeed;
            totals.defenseBonus += passive.bonusDefense;
        }

        return totals;
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
