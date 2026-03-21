using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebuffTooltipHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private TMP_Text targetText;
    private GameObject tooltipRoot;
    private TMP_Text tooltipLabel;
    private readonly Dictionary<string, string> tooltipByKey = new Dictionary<string, string>();
    private bool isPointerInside;

    public void Bind(TMP_Text text, GameObject root, TMP_Text label)
    {
        targetText = text;
        tooltipRoot = root;
        tooltipLabel = label;

        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    public void SetEntries(List<PlayerUIController.DebuffUIEntry> entries)
    {
        tooltipByKey.Clear();
        if (entries == null)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            PlayerUIController.DebuffUIEntry entry = entries[i];
            if (!string.IsNullOrEmpty(entry.Key) && !tooltipByKey.ContainsKey(entry.Key))
                tooltipByKey.Add(entry.Key, entry.Tooltip ?? string.Empty);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        UpdateTooltipAtPointer(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isPointerInside)
            return;

        UpdateTooltipAtPointer(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        HideTooltip();
    }

    private void UpdateTooltipAtPointer(PointerEventData eventData)
    {
        if (targetText == null || tooltipRoot == null || tooltipLabel == null)
            return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(targetText, eventData.position, eventData.pressEventCamera);
        if (linkIndex < 0)
        {
            HideTooltip();
            return;
        }

        TMP_LinkInfo linkInfo = targetText.textInfo.linkInfo[linkIndex];
        string key = linkInfo.GetLinkID();

        if (!tooltipByKey.TryGetValue(key, out string tooltipMessage) || string.IsNullOrEmpty(tooltipMessage))
        {
            HideTooltip();
            return;
        }

        tooltipLabel.text = tooltipMessage;
        tooltipRoot.SetActive(true);

        RectTransform tooltipRect = tooltipRoot.transform as RectTransform;
        RectTransform canvasRect = tooltipRoot.GetComponentInParent<Canvas>()?.transform as RectTransform;
        if (tooltipRect != null && canvasRect != null)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                tooltipRect.anchoredPosition = localPoint + new Vector2(20f, -20f);
            }
        }
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }
}
