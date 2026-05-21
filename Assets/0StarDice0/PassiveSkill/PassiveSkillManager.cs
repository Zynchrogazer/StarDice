using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    [SerializeField] private PlayerStatAggregator playerStatAggregator;

    [Header("Save Data")]
    public int starSkillLevel = 0;      // เลเวลสกิล: เก็บดาวเพิ่ม
    public int attackSkillLevel = 0;    // เลเวลสกิล: ตีแรงขึ้น

    [Header("Settings")]
    public int baseUpgradeCost = 100;   // ราคาเริ่มต้น
    public int attackCostStep = 60;     // เพิ่มราคาสายโจมตีต่อเลเวล
    public int starCostStep = 45;       // เพิ่มราคาสายดาวต่อเลเวล
    public int starGainBonusPerLevel = 1;
    public int attackBonusPerLevel = 5;

     private const string StarSkillLvKey = "StarSkillLv_SHARED";
    private const string AtkSkillLvKey = "AtkSkillLv_SHARED";
    private const string SpdSkillLvKey = "SpdSkillLv_SHARED";
    private const string DefSkillLvKey = "DefSkillLv_SHARED";
    private bool isDataLoaded;
    private string loadedSaveKeySuffix = string.Empty;

    private void Awake()
    {
        PassiveSkillManager[] managers = FindObjectsByType<PassiveSkillManager>(FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        ResolvePlayerStatAggregator();
        EnsureLoadedForCurrentPlayer();
    }

    public bool TryUpgradeStarSkill()
    {
        EnsureLoadedForCurrentPlayer();
        int cost = GetStarUpgradeCost();
        return TrySpendCurrentPlayerCredit(cost, () =>
        {
            starSkillLevel++;
            ApplyPassiveBonusToCurrentPlayer();
            SaveData();
        });
    }

    public bool TryUpgradeAttackSkill()
    {
        EnsureLoadedForCurrentPlayer();
        int cost = GetAttackUpgradeCost();
        return TrySpendCurrentPlayerCredit(cost, () =>
        {
            attackSkillLevel++;
            ApplyPassiveBonusToCurrentPlayer();
            SaveData();
        });
    }

    public int GetStarUpgradeCost()
    {
        EnsureLoadedForCurrentPlayer();
        return baseUpgradeCost + (starSkillLevel * starCostStep);
    }

    public int GetAttackUpgradeCost()
    {
        EnsureLoadedForCurrentPlayer();
        return baseUpgradeCost + 20 + (attackSkillLevel * attackCostStep);
    }

    public int GetStarGainBonusAmount()
    {
        EnsureLoadedForCurrentPlayer();
        return starSkillLevel * starGainBonusPerLevel;
    }

    // Backward-compatible alias (kept to avoid breaking existing references).
    public int GetStarBonusAmount()
    {
        return GetStarGainBonusAmount();
    }

    public int GetAttackBonusAmount()
    {
        EnsureLoadedForCurrentPlayer();
        return attackSkillLevel * attackBonusPerLevel;
    }

    public void ApplyPassiveBonusToCurrentPlayer()
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

        player.CurrentAttack = data.attackDamage + GetAttackBonusAmount();
        player.PassiveStarGainBonus = Mathf.Max(0, GetStarGainBonusAmount());
        player.NotifyStatsUpdated();
    }

    private bool TrySpendCurrentPlayerCredit(int amount, System.Action onSuccess)
    {
        if (amount < 0)
        {
            return false;
        }

        if (GameData.Instance?.selectedPlayer == null)
        {
            Debug.LogWarning("[PassiveSkillManager] Selected player data is missing.");
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
            onSuccess?.Invoke();
            return true;
        }

        if (GameData.Instance.GetSelectedPlayerCredit() < amount)
        {
            return false;
        }

        GameData.Instance.SetSelectedPlayerCredit(GameData.Instance.GetSelectedPlayerCredit() - amount);
        onSuccess?.Invoke();
        return true;
    }

    private void SaveData()
    {
        EnsureLoadedForCurrentPlayer();
        PlayerPrefs.SetInt(GetStarSkillSaveKey(), starSkillLevel);
        PlayerPrefs.SetInt(GetAttackSkillSaveKey(), attackSkillLevel);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        starSkillLevel = PlayerPrefs.GetInt(GetStarSkillSaveKey(), 0);
        attackSkillLevel = PlayerPrefs.GetInt(GetAttackSkillSaveKey(), 0);
        isDataLoaded = true;
    }

    [ContextMenu("Reset Save")]
    public void ResetSave()
    {
        EnsureLoadedForCurrentPlayer();
        PlayerPrefs.DeleteKey(GetStarSkillSaveKey());
        PlayerPrefs.DeleteKey(GetAttackSkillSaveKey());
        LoadData();
        ApplyPassiveBonusToCurrentPlayer();
    }

    private void EnsureLoadedForCurrentPlayer()
    {
        if (isDataLoaded) return;
        LoadData();
    }


    private string GetStarSkillSaveKey()
    {
        return StarSkillLvKey;
    }

    private string GetAttackSkillSaveKey()
    {
        return AtkSkillLvKey;
    }

    private string GetSpeedSkillSaveKey()
    {
        return SpdSkillLvKey;
    }

    private string GetDefenseSkillSaveKey()
    {
        return DefSkillLvKey;
    }

     public static void ClearSavedProgress()
    {
        PlayerPrefs.DeleteKey(StarSkillLvKey);
        PlayerPrefs.DeleteKey(AtkSkillLvKey);
        PlayerPrefs.DeleteKey(SpdSkillLvKey);
        PlayerPrefs.DeleteKey(DefSkillLvKey);

        PassiveSkillManager[] managers = FindObjectsByType<PassiveSkillManager>(FindObjectsSortMode.None);
        for (int i = 0; i < managers.Length; i++)
        {
            PassiveSkillManager manager = managers[i];
            if (manager == null)
            {
                continue;
            }

            manager.starSkillLevel = 0;
            manager.attackSkillLevel = 0;
            manager.isDataLoaded = false;
            manager.ApplyPassiveBonusToCurrentPlayer();
        }
    }

    private PlayerStatAggregator ResolvePlayerStatAggregator()
    {
        if (playerStatAggregator == null)
            playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();

        return playerStatAggregator;
    }

    
}
