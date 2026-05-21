﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SkillTreeUI : MonoBehaviour
{
    [Header("RuntimeHub Services")]
    [SerializeField] private SkillManager skillManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private TextMeshProUGUI goldText; // legacy alias of Credit text

    private PlayerData boundPlayerData;
    private SkillManager subscribedSkillManager;

    private void Awake()
    {
        AutoBindUiReferencesIfMissing();
        LogMissingReferences();
    }

    private void OnEnable()
    {
        subscribedSkillManager = ResolveSkillManager();
        if (subscribedSkillManager != null)
            subscribedSkillManager.OnSkillTreeUpdated += RefreshUI;

        BindCreditListener();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (subscribedSkillManager != null)
        {
            subscribedSkillManager.OnSkillTreeUpdated -= RefreshUI;
            subscribedSkillManager = null;
        }

        UnbindCreditListener();
    }

    private SkillManager ResolveSkillManager()
    {
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        return skillManager;
    }

    public void RefreshUI()
    {
        BindCreditListener();

        int playerCredit = GameTurnManager.CurrentPlayer != null
            ? GameTurnManager.CurrentPlayer.PlayerCredit
            : (GameData.Instance?.selectedPlayer != null ? GameData.Instance.GetSelectedPlayerCredit() : 0);

        if (creditText != null) creditText.text = $"Credit: {playerCredit}";
        if (goldText != null) goldText.text = $"Credit: {playerCredit}";
    }

    private void BindCreditListener()
    {
        PlayerData currentSelectedPlayer = GameData.Instance != null ? GameData.Instance.selectedPlayer : null;
        if (boundPlayerData == currentSelectedPlayer)
            return;

        UnbindCreditListener();
        boundPlayerData = currentSelectedPlayer;
        if (boundPlayerData != null)
            boundPlayerData.OnCreditChanged += HandleCreditChanged;
    }

    private void UnbindCreditListener()
    {
        if (boundPlayerData == null)
            return;

        boundPlayerData.OnCreditChanged -= HandleCreditChanged;
        boundPlayerData = null;
    }

    private void HandleCreditChanged(int _)
    {
        RefreshUI();
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

        if (creditText == null) creditText = FindText(texts, "credit");
        if (goldText == null) goldText = FindText(texts, "gold");
        if (goldText == null) goldText = creditText;
    }

    private void LogMissingReferences()
    {
        if (creditText == null) Debug.LogWarning("[SkillTreeUI] Missing creditText reference.", this);
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
