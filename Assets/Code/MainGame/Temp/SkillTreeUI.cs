using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI goldText;

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
        UpdateUI();

        // ผูกปุ่มกด
        upgradeStarBtn.onClick.AddListener(() => {
            if (PassiveSkillManager.Instance.TryUpgradeStarSkill()) UpdateUI();
        });

        upgradeAttackBtn.onClick.AddListener(() => {
            if (PassiveSkillManager.Instance.TryUpgradeAttackSkill()) UpdateUI();
        });
    }

    private void UpdateUI()
    {
        if (PassiveSkillManager.Instance == null) return;

        // อัปเดตเงิน
        goldText.text = $"Gold: {PassiveSkillManager.Instance.globalGold}";

        // อัปเดตปุ่มดาว
        int starLv = PassiveSkillManager.Instance.starSkillLevel;
        int starCost = PassiveSkillManager.Instance.GetUpgradeCost(starLv);
        starLevelText.text = $"Lv. {starLv} (+{PassiveSkillManager.Instance.GetStarBonusAmount()} Stars)";
        starCostText.text = $"Cost: {starCost}";
        upgradeStarBtn.interactable = PassiveSkillManager.Instance.globalGold >= starCost;

        // อัปเดตปุ่มโจมตี
        int atkLv = PassiveSkillManager.Instance.attackSkillLevel;
        int atkCost = PassiveSkillManager.Instance.GetUpgradeCost(atkLv);
        attackLevelText.text = $"Lv. {atkLv} (+{PassiveSkillManager.Instance.GetAttackBonusAmount()} Dmg)";
        attackCostText.text = $"Cost: {atkCost}";
        upgradeAttackBtn.interactable = PassiveSkillManager.Instance.globalGold >= atkCost;
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu"); // หรือชื่อฉากเมนูของคุณ
    }
}