using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("RuntimeHub Services")]
    [SerializeField] private PassiveSkillManager passiveSkillManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private TextMeshProUGUI goldText; // backward compatibility

    [Header("Star Skill")]
    [SerializeField] private TextMeshProUGUI starLevelText;
    [SerializeField] private TextMeshProUGUI starCostText;
    [SerializeField] private Button upgradeStarBtn;

    [Header("Attack Skill")]
    [SerializeField] private TextMeshProUGUI attackLevelText;
    [SerializeField] private TextMeshProUGUI attackCostText;
    [SerializeField] private Button upgradeAttackBtn;

    private void Awake()
    {
        ResolvePassiveSkillManager();
    }

    private void OnEnable()
    {
        if (upgradeStarBtn != null)
            upgradeStarBtn.onClick.AddListener(OnClickUpgradeStar);

        if (upgradeAttackBtn != null)
            upgradeAttackBtn.onClick.AddListener(OnClickUpgradeAttack);

        if (ResolvePassiveSkillManager() != null)
            ResolvePassiveSkillManager().ApplyPassiveBonusToCurrentPlayer();

        RefreshUI();
    }

    private void OnDisable()
    {
        if (upgradeStarBtn != null)
            upgradeStarBtn.onClick.RemoveListener(OnClickUpgradeStar);

        if (upgradeAttackBtn != null)
            upgradeAttackBtn.onClick.RemoveListener(OnClickUpgradeAttack);
    }

    private void OnClickUpgradeStar()
    {
        if (ResolvePassiveSkillManager() != null && ResolvePassiveSkillManager().TryUpgradeStarSkill())
            RefreshUI();
    }

    private void OnClickUpgradeAttack()
    {
        if (ResolvePassiveSkillManager() != null && ResolvePassiveSkillManager().TryUpgradeAttackSkill())
            RefreshUI();
    }

    private PassiveSkillManager ResolvePassiveSkillManager()
    {
        if (passiveSkillManager == null)
            passiveSkillManager = FindFirstObjectByType<PassiveSkillManager>();

        return passiveSkillManager;
    }

    public void RefreshUI()
    {
        PassiveSkillManager manager = ResolvePassiveSkillManager();
        if (manager == null) return;

        int playerCredit = GameTurnManager.CurrentPlayer != null
            ? GameTurnManager.CurrentPlayer.PlayerCredit
            : (GameData.Instance?.selectedPlayer != null ? GameData.Instance.selectedPlayer.Credit : 0);

        if (creditText != null) creditText.text = $"Credit: {playerCredit}";
        if (goldText != null) goldText.text = $"Credit: {playerCredit}";

        int starCost = manager.GetStarUpgradeCost();
        if (starLevelText != null) starLevelText.text = $"Lv. {manager.starSkillLevel} (+{manager.GetStarBonusAmount()} MaxHP)";
        if (starCostText != null) starCostText.text = $"Cost: {starCost}";
        if (upgradeStarBtn != null) upgradeStarBtn.interactable = playerCredit >= starCost;

        int attackCost = manager.GetAttackUpgradeCost();
        if (attackLevelText != null) attackLevelText.text = $"Lv. {manager.attackSkillLevel} (+{manager.GetAttackBonusAmount()} Dmg)";
        if (attackCostText != null) attackCostText.text = $"Cost: {attackCost}";
        if (upgradeAttackBtn != null) upgradeAttackBtn.interactable = playerCredit >= attackCost;
    }

    public void OnBackButtonClicked()
    {
        Scene activeScene = gameObject.scene;
        if (activeScene.IsValid() && SceneManager.sceneCount > 1)
        {
            SceneManager.UnloadSceneAsync(activeScene);
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }
}
