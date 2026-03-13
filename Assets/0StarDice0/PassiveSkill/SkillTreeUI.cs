﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Optional Auto-Bind Name Overrides")]
    [SerializeField] private string starButtonKeyword = "star";
    [SerializeField] private string attackButtonKeyword = "attack";

    [Header("RuntimeHub Services")]
    [SerializeField] private PassiveSkillManager passiveSkillManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private TextMeshProUGUI goldText; // legacy alias of Credit text

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
        AutoBindUiReferencesIfMissing();
        ResolvePassiveSkillManager();
        LogMissingReferences();
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
        if (manager == null)
        {
            Debug.LogWarning("[SkillTreeUI] PassiveSkillManager was not found. Ensure RuntimeHub is loaded.", this);
            return;
        }

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

    [ContextMenu("Validate SkillTreeUI Setup")]
    public void ValidateSetup()
    {
        AutoBindUiReferencesIfMissing();
        LogMissingReferences();
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

    private void AutoBindUiReferencesIfMissing()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        Button[] buttons = GetComponentsInChildren<Button>(true);

        if (creditText == null) creditText = FindText(texts, "credit");
        if (goldText == null) goldText = FindText(texts, "gold");
        if (goldText == null) goldText = creditText;

        if (starLevelText == null) starLevelText = FindText(texts, "star", "level");
        if (starCostText == null) starCostText = FindText(texts, "star", "cost");
        if (upgradeStarBtn == null) upgradeStarBtn = FindButton(buttons, starButtonKeyword, "upgrade");

        if (attackLevelText == null) attackLevelText = FindText(texts, "attack", "level");
        if (attackCostText == null) attackCostText = FindText(texts, "attack", "cost");
        if (upgradeAttackBtn == null) upgradeAttackBtn = FindButton(buttons, attackButtonKeyword, "upgrade");
    }

    private void LogMissingReferences()
    {
        if (creditText == null) Debug.LogWarning("[SkillTreeUI] Missing creditText reference.", this);
        if (starLevelText == null) Debug.LogWarning("[SkillTreeUI] Missing starLevelText reference.", this);
        if (starCostText == null) Debug.LogWarning("[SkillTreeUI] Missing starCostText reference.", this);
        if (upgradeStarBtn == null) Debug.LogWarning("[SkillTreeUI] Missing upgradeStarBtn reference.", this);
        if (attackLevelText == null) Debug.LogWarning("[SkillTreeUI] Missing attackLevelText reference.", this);
        if (attackCostText == null) Debug.LogWarning("[SkillTreeUI] Missing attackCostText reference.", this);
        if (upgradeAttackBtn == null) Debug.LogWarning("[SkillTreeUI] Missing upgradeAttackBtn reference.", this);
    }

    private static TextMeshProUGUI FindText(TextMeshProUGUI[] texts, params string[] keywords)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (ContainsAllKeywords(texts[i].name, keywords))
                return texts[i];
        }

        return null;
    }

    private static Button FindButton(Button[] buttons, params string[] keywords)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (ContainsAllKeywords(buttons[i].name, keywords))
                return buttons[i];
        }

        return null;
    }

    private static bool ContainsAllKeywords(string source, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;

        string lower = source.ToLowerInvariant();
        for (int i = 0; i < keywords.Length; i++)
        {
            if (!lower.Contains(keywords[i]))
                return false;
        }

        return true;
    }
}