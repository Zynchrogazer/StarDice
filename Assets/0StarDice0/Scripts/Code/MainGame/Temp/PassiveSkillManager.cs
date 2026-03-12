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
    public int starBonusPerLevel = 1;
    public int attackBonusPerLevel = 5;

    private const string StarSkillLvKeyPrefix = "StarSkillLv";
    private const string AtkSkillLvKeyPrefix = "AtkSkillLv";
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

    public int GetStarBonusAmount()
    {
        EnsureLoadedForCurrentPlayer();
        return starSkillLevel * starBonusPerLevel;
    }

    public int GetAttackBonusAmount()
    {
        EnsureLoadedForCurrentPlayer();
        return attackSkillLevel * attackBonusPerLevel;
    }

    public void ApplyPassiveBonusToCurrentPlayer()
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

        int oldMaxHp = player.MaxHealth;
        player.CurrentAttack = data.attackDamage + GetAttackBonusAmount();
        player.MaxHealth = data.maxHP + GetStarBonusAmount();

        int hpDelta = player.MaxHealth - oldMaxHp;
        player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);
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
            GameData.Instance.selectedPlayer.SetCredit(player.PlayerCredit);
            onSuccess?.Invoke();
            return true;
        }

        PlayerData selectedPlayer = GameData.Instance.selectedPlayer;
        if (selectedPlayer.Credit < amount)
        {
            return false;
        }

        selectedPlayer.SetCredit(selectedPlayer.Credit - amount);
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
        loadedSaveKeySuffix = GetPlayerScopeSuffix();
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
        string suffix = GetPlayerScopeSuffix();
        if (suffix == loadedSaveKeySuffix) return;
        LoadData();
    }

    private string GetStarSkillSaveKey()
    {
        return BuildPlayerScopedKey(StarSkillLvKeyPrefix);
    }

    private string GetAttackSkillSaveKey()
    {
        return BuildPlayerScopedKey(AtkSkillLvKeyPrefix);
    }

    private string BuildPlayerScopedKey(string prefix)
    {
        return $"{prefix}_{GetPlayerScopeSuffix()}";
    }

    private PlayerStatAggregator ResolvePlayerStatAggregator()
    {
        if (playerStatAggregator == null)
            playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();

        return playerStatAggregator;
    }

    private string GetPlayerScopeSuffix()
    {
        string fallback = "default";
        if (GameData.Instance?.selectedPlayer == null)
        {
            return fallback;
        }

        return !string.IsNullOrWhiteSpace(GameData.Instance.selectedPlayer.playerName)
            ? GameData.Instance.selectedPlayer.playerName
            : GameData.Instance.selectedPlayer.name;
    }
}
