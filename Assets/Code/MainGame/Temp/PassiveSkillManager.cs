using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    public static PassiveSkillManager Instance { get; private set; }

    [Header("Save Data")]
    public int starSkillLevel = 0;      // เลเวลสกิล: เก็บดาวเพิ่ม
    public int attackSkillLevel = 0;    // เลเวลสกิล: ตีแรงขึ้น

    [Header("Settings")]
    public int baseUpgradeCost = 100;   // ราคาเริ่มต้น
    public int attackCostStep = 60;     // เพิ่มราคาสายโจมตีต่อเลเวล
    public int starCostStep = 45;       // เพิ่มราคาสายดาวต่อเลเวล
    public int starBonusPerLevel = 1;
    public int attackBonusPerLevel = 5;

    private const string StarSkillLvKey = "StarSkillLv";
    private const string AtkSkillLvKey = "AtkSkillLv";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    public bool TryUpgradeStarSkill()
    {
        int cost = GetStarUpgradeCost();
        return TrySpendCurrentPlayerMoney(cost, () =>
        {
            starSkillLevel++;
            ApplyPassiveBonusToCurrentPlayer();
            SaveData();
        });
    }

    public bool TryUpgradeAttackSkill()
    {
        int cost = GetAttackUpgradeCost();
        return TrySpendCurrentPlayerMoney(cost, () =>
        {
            attackSkillLevel++;
            ApplyPassiveBonusToCurrentPlayer();
            SaveData();
        });
    }

    public int GetStarUpgradeCost()
    {
        return baseUpgradeCost + (starSkillLevel * starCostStep);
    }

    public int GetAttackUpgradeCost()
    {
        return baseUpgradeCost + 20 + (attackSkillLevel * attackCostStep);
    }

    public int GetStarBonusAmount()
    {
        return starSkillLevel * starBonusPerLevel;
    }

    public int GetAttackBonusAmount()
    {
        return attackSkillLevel * attackBonusPerLevel;
    }

    public void ApplyPassiveBonusToCurrentPlayer()
    {
        if (GameTurnManager.CurrentPlayer == null || GameData.Instance?.selectedPlayer == null)
        {
            return;
        }

        PlayerState player = GameTurnManager.CurrentPlayer;
        PlayerData data = GameData.Instance.selectedPlayer;

        int baseAttack = data.attackDamage;
        int baseMaxHp = data.maxHP;

        int bonusAttack = GetAttackBonusAmount();
        int bonusMaxHp = GetStarBonusAmount();

        int oldMaxHp = player.MaxHealth;

        player.CurrentAttack = baseAttack + bonusAttack;
        player.MaxHealth = baseMaxHp + bonusMaxHp;

        if (player.MaxHealth != oldMaxHp)
        {
            int hpDelta = player.MaxHealth - oldMaxHp;
            player.PlayerHealth = Mathf.Clamp(player.PlayerHealth + hpDelta, 0, player.MaxHealth);
        }
        else
        {
            player.PlayerHealth = Mathf.Clamp(player.PlayerHealth, 0, player.MaxHealth);
        }
    }

    private bool TrySpendCurrentPlayerMoney(int amount, System.Action onSuccess)
    {
        if (amount < 0)
        {
            return false;
        }

        if (GameTurnManager.CurrentPlayer == null || GameData.Instance?.selectedPlayer == null)
        {
            Debug.LogWarning("[PassiveSkillManager] Current player or GameData is missing.");
            return false;
        }

        PlayerState player = GameTurnManager.CurrentPlayer;
        if (player.PlayerMoney < amount)
        {
            return false;
        }

        player.PlayerMoney -= amount;
        GameData.Instance.selectedPlayer.SetMoney(player.PlayerMoney);
        onSuccess?.Invoke();
        return true;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(StarSkillLvKey, starSkillLevel);
        PlayerPrefs.SetInt(AtkSkillLvKey, attackSkillLevel);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        starSkillLevel = PlayerPrefs.GetInt(StarSkillLvKey, 0);
        attackSkillLevel = PlayerPrefs.GetInt(AtkSkillLvKey, 0);
    }

    [ContextMenu("Reset Save")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(StarSkillLvKey);
        PlayerPrefs.DeleteKey(AtkSkillLvKey);
        LoadData();
        ApplyPassiveBonusToCurrentPlayer();
    }
}
