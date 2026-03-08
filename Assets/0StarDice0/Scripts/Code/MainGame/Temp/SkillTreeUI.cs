using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI creditText;
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

        int playerCredit = GameTurnManager.CurrentPlayer != null
            ? GameTurnManager.CurrentPlayer.PlayerCredit
            : (GameData.Instance?.selectedPlayer != null ? GameData.Instance.selectedPlayer.Credit : 0);

        if (creditText != null) creditText.text = $"Credit: {playerCredit}";
        if (goldText != null) goldText.text = $"Credit: {playerCredit}";

        int starLv = PassiveSkillManager.Instance.starSkillLevel;
        int starCost = PassiveSkillManager.Instance.GetStarUpgradeCost();
        starLevelText.text = $"Lv. {starLv} (+{PassiveSkillManager.Instance.GetStarBonusAmount()} MaxHP)";
        starCostText.text = $"Cost: {starCost}";
        upgradeStarBtn.interactable = playerCredit >= starCost;

        int atkLv = PassiveSkillManager.Instance.attackSkillLevel;
        int atkCost = PassiveSkillManager.Instance.GetAttackUpgradeCost();
        attackLevelText.text = $"Lv. {atkLv} (+{PassiveSkillManager.Instance.GetAttackBonusAmount()} Dmg)";
        attackCostText.text = $"Cost: {atkCost}";
        upgradeAttackBtn.interactable = playerCredit >= atkCost;
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
