﻿using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PassiveSkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PassiveSkillData passiveSkillData;
    public Image iconImage;
    public Image frameImage;

    [Header("Colors")]
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color unlockableColor = new Color(1f, 0.92f, 0.3f, 1f);
    public Color unlockedColor = Color.white;

    [Header("Frame Colors")]
    public Color lockedFrameColor = new Color(0.35f, 0.15f, 0.15f, 1f);
    public Color unlockableFrameColor = new Color(0.95f, 0.8f, 0.2f, 1f);
    public Color unlockedFrameColor = new Color(0.25f, 0.95f, 0.4f, 1f);

    [Header("Locked Overlay")]
    [Range(0f, 1f)] public float lockedOverlayAlpha = 0.5f;
    public GameObject lockedOverlayObject;

    private Image lockedOverlayImage;
    [SerializeField] private PassiveSkillTooltip tooltip;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private TextMeshProUGUI costText;

    private void Awake()
    {
        if (frameImage == null)
        {
            frameImage = GetComponent<Image>();
        }

        EnsureLockedOverlay();
        ResolveCostText();
    }

    private void Start()
    {
        if (tooltip == null)
            tooltip = FindFirstObjectByType<PassiveSkillTooltip>();
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();
        if (passiveSkillData != null && iconImage != null)
        {
            iconImage.sprite = passiveSkillData.icon;
            UpdateUI();
        }

        if (ResolveSkillManager() != null)
            ResolveSkillManager().OnSkillTreeUpdated += UpdateUI;
    }

    private void OnDestroy()
    {
        if (ResolveSkillManager() != null)
            ResolveSkillManager().OnSkillTreeUpdated -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (passiveSkillData == null || iconImage == null || ResolveSkillManager() == null) return;

        bool isUnlocked = ResolveSkillManager().IsUnlocked(passiveSkillData);
        bool canUnlock = ResolveSkillManager().CanUnlock(passiveSkillData);

        if (isUnlocked)
        {
            iconImage.color = unlockedColor;
            if (frameImage != null) frameImage.color = unlockedFrameColor;
            SetLockedOverlay(false);
        }
        else if (canUnlock)
        {
            iconImage.color = unlockableColor;
            if (frameImage != null) frameImage.color = unlockableFrameColor;
            SetLockedOverlay(true, lockedOverlayAlpha * 0.5f);
        }
        else
        {
            iconImage.color = lockedColor;
            if (frameImage != null) frameImage.color = lockedFrameColor;
            SetLockedOverlay(true, lockedOverlayAlpha);
        }

        if (ResolveCostText() != null && passiveSkillData != null)
        {
            ResolveCostText().text = $"Cost: {passiveSkillData.costPoint}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (passiveSkillData != null && ResolveSkillManager() != null)
        {
            if (ResolveSkillManager().TryUnlockSkill(passiveSkillData))
            {
                Debug.Log($"Upgrade {passiveSkillData.skillName} Success!");
            }
            else
            {
                Debug.Log("Cannot Unlock (Not enough points or requirements not met)");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null)
            tooltip = FindFirstObjectByType<PassiveSkillTooltip>();
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        if (passiveSkillData != null && tooltip != null)
            tooltip.ShowTooltip(passiveSkillData.skillName, BuildTooltipDescription());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip == null)
            tooltip = FindFirstObjectByType<PassiveSkillTooltip>();
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        if (tooltip != null)
            tooltip.HideTooltip();
    }

    private TextMeshProUGUI ResolveCostText()
    {
        if (costText != null)
            return costText;

        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name.ToLowerInvariant().Contains("cost"))
            {
                costText = texts[i];
                break;
            }
        }

        return costText;
    }

    private SkillManager ResolveSkillManager()
    {
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        return skillManager;
    }

    private void EnsureLockedOverlay()
    {
        if (lockedOverlayObject == null)
        {
            Transform existing = transform.Find("LockedOverlay");
            if (existing != null)
            {
                lockedOverlayObject = existing.gameObject;
            }
            else
            {
                GameObject overlay = new GameObject("LockedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                overlay.transform.SetParent(transform, false);

                RectTransform rect = overlay.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                lockedOverlayObject = overlay;
            }
        }

        if (lockedOverlayObject != null)
        {
            lockedOverlayImage = lockedOverlayObject.GetComponent<Image>();
            if (lockedOverlayImage != null)
            {
                lockedOverlayImage.raycastTarget = false;
                lockedOverlayImage.color = new Color(0f, 0f, 0f, lockedOverlayAlpha);
            }
        }
    }

    private void SetLockedOverlay(bool visible, float alphaOverride = -1f)
    {
        if (lockedOverlayObject == null)
        {
            EnsureLockedOverlay();
        ResolveCostText();
        }

        if (lockedOverlayObject == null) return;

        if (lockedOverlayImage != null)
        {
            float useAlpha = alphaOverride >= 0f ? alphaOverride : lockedOverlayAlpha;
            lockedOverlayImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(useAlpha));
        }

        lockedOverlayObject.SetActive(visible);
    }

    private string BuildTooltipDescription()
    {
        if (passiveSkillData == null)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(passiveSkillData.description))
        {
            builder.AppendLine(passiveSkillData.description.Trim());
            builder.AppendLine();
        }

        AppendBonusLine(builder, "ATK", passiveSkillData.bonusAttack);
        AppendBonusLine(builder, "HP", passiveSkillData.bonusMaxHP);
        AppendBonusLine(builder, "STAR", passiveSkillData.bonusStar);
        AppendBonusLine(builder, "SPD", passiveSkillData.bonusSpeed);
        AppendBonusLine(builder, "DEF", passiveSkillData.bonusDefense);

        if (passiveSkillData.costPoint > 0)
        {
            builder.AppendLine($"Cost: {passiveSkillData.costPoint}");
        }

        string result = builder.ToString().TrimEnd();
        return string.IsNullOrEmpty(result) ? "No bonus" : result;
    }

    private static void AppendBonusLine(StringBuilder builder, string label, int value)
    {
        if (value == 0) return;
        string sign = value > 0 ? "+" : string.Empty;
        builder.AppendLine($"{label}: {sign}{value}");
    }
}