using UnityEngine;
using TMPro; // ใช้ TextMeshPro

public class PassiveSkillTooltip : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    [Header("Text Fit")]
    [SerializeField] private bool fitDescriptionInBox = true;
    [SerializeField] private float descriptionMinFontSize = 18f;
    [SerializeField] private float descriptionVerticalOffset = 12f;

    private RectTransform descriptionRect;
    private Vector2 originalDescriptionAnchoredPosition;
    private bool cachedDescriptionPosition;

    private void Awake()
    {
        CacheDescriptionRect();
        ApplyDescriptionTextFit();
        HideTooltip(); // ซ่อนตอนเริ่มเกม
    }

    public void ShowTooltip(string skillName, string skillDesc)
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(true);
        if (nameText != null) nameText.text = skillName;
        if (descriptionText != null)
        {
            descriptionText.text = skillDesc;
            ApplyDescriptionTextFit();
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void CacheDescriptionRect()
    {
        if (descriptionText == null || descriptionRect != null)
            return;

        descriptionRect = descriptionText.GetComponent<RectTransform>();
        if (descriptionRect == null)
            return;

        originalDescriptionAnchoredPosition = descriptionRect.anchoredPosition;
        cachedDescriptionPosition = true;
    }

    private void ApplyDescriptionTextFit()
    {
        if (descriptionText == null || !fitDescriptionInBox)
            return;

        CacheDescriptionRect();

        descriptionText.enableWordWrapping = true;
        descriptionText.enableAutoSizing = true;
        descriptionText.fontSizeMin = descriptionMinFontSize;
        descriptionText.fontSizeMax = Mathf.Max(descriptionText.fontSize, descriptionMinFontSize);
        descriptionText.overflowMode = TextOverflowModes.Truncate;
        descriptionText.verticalAlignment = VerticalAlignmentOptions.Top;

        if (descriptionRect != null && cachedDescriptionPosition)
        {
            descriptionRect.anchoredPosition = originalDescriptionAnchoredPosition + Vector2.up * descriptionVerticalOffset;
        }

        descriptionText.ForceMeshUpdate();
    }
}
