using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI goldText; // backward compatibility

    [Header("Star Skill")]
    public TextMeshProUGUI starLevelText;
    public TextMeshProUGUI starCostText;
    public Button upgradeStarBtn;

    [Header("Attack Skill")]
    public TextMeshProUGUI attackLevelText;
    public TextMeshProUGUI attackCostText;
    public Button upgradeAttackBtn;

    private void Start()
    {
        if (PassiveSkillManager.Instance != null)
        {
            PassiveSkillManager.Instance.ApplyPassiveBonusToCurrentPlayer();
        }

        UpdateUI();

        upgradeStarBtn.onClick.AddListener(() => {
            if (PassiveSkillManager.Instance != null && PassiveSkillManager.Instance.TryUpgradeStarSkill())
            {
                UpdateUI();
            }
        });

        upgradeAttackBtn.onClick.AddListener(() => {
            if (PassiveSkillManager.Instance != null && PassiveSkillManager.Instance.TryUpgradeAttackSkill())
            {
                UpdateUI();
            }
        });
    }

    private void UpdateUI()
    {
        if (PassiveSkillManager.Instance == null) return;

        int playerMoney = GameTurnManager.CurrentPlayer != null
            ? GameTurnManager.CurrentPlayer.PlayerMoney
            : (GameData.Instance?.selectedPlayer != null ? GameData.Instance.selectedPlayer.Money : 0);

        if (moneyText != null) moneyText.text = $"Money: {playerMoney}";
        if (goldText != null) goldText.text = $"Money: {playerMoney}";

        int starLv = PassiveSkillManager.Instance.starSkillLevel;
        int starCost = PassiveSkillManager.Instance.GetStarUpgradeCost();
        starLevelText.text = $"Lv. {starLv} (+{PassiveSkillManager.Instance.GetStarBonusAmount()} MaxHP)";
        starCostText.text = $"Cost: {starCost}";
        upgradeStarBtn.interactable = playerMoney >= starCost;

        int atkLv = PassiveSkillManager.Instance.attackSkillLevel;
        int atkCost = PassiveSkillManager.Instance.GetAttackUpgradeCost();
        attackLevelText.text = $"Lv. {atkLv} (+{PassiveSkillManager.Instance.GetAttackBonusAmount()} Dmg)";
        attackCostText.text = $"Cost: {atkCost}";
        upgradeAttackBtn.interactable = playerMoney >= atkCost;
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
