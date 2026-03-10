using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private PassiveSkillManager passiveSkillManager;
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
        if (ResolvePassiveSkillManager() != null)
        {
            ResolvePassiveSkillManager().ApplyPassiveBonusToCurrentPlayer();
        }

        UpdateUI();

        upgradeStarBtn.onClick.AddListener(() => {
            if (ResolvePassiveSkillManager() != null && ResolvePassiveSkillManager().TryUpgradeStarSkill())
            {
                UpdateUI();
            }
        });

        upgradeAttackBtn.onClick.AddListener(() => {
            if (ResolvePassiveSkillManager() != null && ResolvePassiveSkillManager().TryUpgradeAttackSkill())
            {
                UpdateUI();
            }
        });
    }

    private PassiveSkillManager ResolvePassiveSkillManager()
    {
        if (passiveSkillManager == null)
            passiveSkillManager = FindFirstObjectByType<PassiveSkillManager>();

        return passiveSkillManager;
    }

    private void UpdateUI()
    {
        if (ResolvePassiveSkillManager() == null) return;

        int playerCredit = GameTurnManager.CurrentPlayer != null
            ? GameTurnManager.CurrentPlayer.PlayerCredit
            : (GameData.Instance?.selectedPlayer != null ? GameData.Instance.selectedPlayer.Credit : 0);

        if (creditText != null) creditText.text = $"Credit: {playerCredit}";
        if (goldText != null) goldText.text = $"Credit: {playerCredit}";

        int starLv = ResolvePassiveSkillManager().starSkillLevel;
        int starCost = ResolvePassiveSkillManager().GetStarUpgradeCost();
        starLevelText.text = $"Lv. {starLv} (+{ResolvePassiveSkillManager().GetStarBonusAmount()} MaxHP)";
        starCostText.text = $"Cost: {starCost}";
        upgradeStarBtn.interactable = playerCredit >= starCost;

        int atkLv = ResolvePassiveSkillManager().attackSkillLevel;
        int atkCost = ResolvePassiveSkillManager().GetAttackUpgradeCost();
        attackLevelText.text = $"Lv. {atkLv} (+{ResolvePassiveSkillManager().GetAttackBonusAmount()} Dmg)";
        attackCostText.text = $"Cost: {atkCost}";
        upgradeAttackBtn.interactable = playerCredit >= atkCost;
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
