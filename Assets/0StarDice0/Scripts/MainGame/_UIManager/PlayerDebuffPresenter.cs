using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebuffPresenter
{
    private readonly GameObject debuffIconPrefab;
    private readonly Vector2 debuffIconSize;
    private readonly Sprite burnDebuffSprite;
    private readonly Sprite iceDebuffSprite;
    private readonly List<DebuffSpriteIconHoverHandler> debuffIconHandlers = new List<DebuffSpriteIconHoverHandler>();
    private DebuffTooltipHoverHandler debuffTooltipHoverHandler;

    public PlayerDebuffPresenter(GameObject debuffIconPrefab, Vector2 debuffIconSize, Sprite burnDebuffSprite, Sprite iceDebuffSprite)
    {
        this.debuffIconPrefab = debuffIconPrefab;
        this.debuffIconSize = debuffIconSize;
        this.burnDebuffSprite = burnDebuffSprite;
        this.iceDebuffSprite = iceDebuffSprite;
    }

    public void ResetBindings()
    {
        debuffTooltipHoverHandler = null;
        debuffIconHandlers.Clear();
    }

    public void Present(PlayerStatusPanelRefs panelRefs, PlayerState player)
    {
        if (panelRefs == null || player == null)
            return;

        List<DebuffUIEntry> entries = BuildEntries(player);
        RefreshSpriteIcons(panelRefs, entries);
        RefreshTextFallback(panelRefs, entries);
    }

    private List<DebuffUIEntry> BuildEntries(PlayerState player)
    {
        List<DebuffUIEntry> entries = new List<DebuffUIEntry>(2);

        if (player.DebuffBurn && player.DebuffBurnTurnsRemaining > 0)
        {
            entries.Add(new DebuffUIEntry(
                "burn",
                "🔥",
                burnDebuffSprite,
                player.BurnDebuffAppliedOrder,
                $"Burn: รับความเสียหายตอนเริ่มเทิร์น\nคงเหลือ: {player.DebuffBurnTurnsRemaining} เทิร์น"));
        }

        if (player.hasIceEffect)
        {
            entries.Add(new DebuffUIEntry(
                "ice",
                "❄️",
                iceDebuffSprite,
                player.IceDebuffAppliedOrder,
                "Ice: ทอยเต๋าครั้งถัดไปจะเหลือครึ่งหนึ่ง\nคงเหลือ: 1 ครั้ง"));
        }

        entries.Sort((left, right) =>
        {
            int leftOrder = left.Order <= 0 ? int.MaxValue : left.Order;
            int rightOrder = right.Order <= 0 ? int.MaxValue : right.Order;
            return leftOrder.CompareTo(rightOrder);
        });

        return entries;
    }

    private void RefreshSpriteIcons(PlayerStatusPanelRefs panelRefs, List<DebuffUIEntry> entries)
    {
        if (panelRefs.debuffIconContainer == null)
            return;

        EnsureSpriteIconPool(panelRefs.debuffIconContainer, entries.Count);

        for (int i = 0; i < debuffIconHandlers.Count; i++)
        {
            bool shouldShow = i < entries.Count;
            DebuffSpriteIconHoverHandler handler = debuffIconHandlers[i];
            GameObject iconObject = handler != null ? handler.gameObject : null;
            if (iconObject == null)
                continue;

            if (!shouldShow)
            {
                iconObject.SetActive(false);
                continue;
            }

            DebuffUIEntry entry = entries[i];
            handler.Bind(panelRefs.debuffTooltipRoot, panelRefs.debuffTooltipText, entry.Tooltip);
            handler.SetSprite(entry.IconSprite);
            iconObject.name = $"DebuffIcon_{entry.Key}";
            iconObject.SetActive(entry.IconSprite != null);
        }

        if (entries.Count == 0 && panelRefs.debuffTooltipRoot != null)
            panelRefs.debuffTooltipRoot.SetActive(false);
    }

    private void RefreshTextFallback(PlayerStatusPanelRefs panelRefs, List<DebuffUIEntry> entries)
    {
        if (panelRefs.debuffLegacyText == null)
            return;

        bool isUsingSpriteIcons = panelRefs.debuffIconContainer != null;
        panelRefs.debuffLegacyText.gameObject.SetActive(!isUsingSpriteIcons);
        if (isUsingSpriteIcons)
            return;

        EnsureDebuffTooltipHandler(panelRefs);
        panelRefs.debuffLegacyText.text = BuildDebuffIconRichText(entries);
        if (debuffTooltipHoverHandler != null)
            debuffTooltipHoverHandler.SetEntries(entries);
    }

    private void EnsureSpriteIconPool(Transform debuffIconContainer, int requiredCount)
    {
        while (debuffIconHandlers.Count < requiredCount)
        {
            DebuffSpriteIconHoverHandler handler = CreateDebuffSpriteIcon(debuffIconContainer);
            if (handler == null)
                break;

            debuffIconHandlers.Add(handler);
        }
    }

    private DebuffSpriteIconHoverHandler CreateDebuffSpriteIcon(Transform debuffIconContainer)
    {
        GameObject iconObject;
        if (debuffIconPrefab != null)
        {
            iconObject = Object.Instantiate(debuffIconPrefab, debuffIconContainer);
        }
        else
        {
            iconObject = new GameObject("DebuffIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(debuffIconContainer, false);

            RectTransform rectTransform = iconObject.transform as RectTransform;
            if (rectTransform != null)
                rectTransform.sizeDelta = debuffIconSize;

            LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = debuffIconSize.x;
                layoutElement.preferredHeight = debuffIconSize.y;
                layoutElement.minWidth = debuffIconSize.x;
                layoutElement.minHeight = debuffIconSize.y;
            }

            Image image = iconObject.GetComponent<Image>();
            if (image != null)
                image.preserveAspect = true;
        }

        DebuffSpriteIconHoverHandler handler = iconObject.GetComponent<DebuffSpriteIconHoverHandler>();
        if (handler == null)
            handler = iconObject.AddComponent<DebuffSpriteIconHoverHandler>();

        if (handler.TargetImage == null)
            handler.CacheImageReference();

        iconObject.SetActive(false);
        return handler;
    }

    private void EnsureDebuffTooltipHandler(PlayerStatusPanelRefs panelRefs)
    {
        if (panelRefs.debuffLegacyText == null)
            return;

        if (debuffTooltipHoverHandler == null)
            debuffTooltipHoverHandler = panelRefs.debuffLegacyText.GetComponent<DebuffTooltipHoverHandler>();

        if (debuffTooltipHoverHandler == null)
            debuffTooltipHoverHandler = panelRefs.debuffLegacyText.gameObject.AddComponent<DebuffTooltipHoverHandler>();

        debuffTooltipHoverHandler.Bind(panelRefs.debuffLegacyText, panelRefs.debuffTooltipRoot, panelRefs.debuffTooltipText);
    }

    private static string BuildDebuffIconRichText(List<DebuffUIEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return "-";

        StringBuilder sb = new StringBuilder(32);
        for (int i = 0; i < entries.Count; i++)
        {
            DebuffUIEntry entry = entries[i];
            sb.Append("<link=");
            sb.Append(entry.Key);
            sb.Append('>');
            sb.Append(entry.LegacyIconText);
            sb.Append("</link>");

            if (i < entries.Count - 1)
                sb.Append("  ");
        }

        return sb.ToString();
    }

    public readonly struct DebuffUIEntry
    {
        public DebuffUIEntry(string key, string legacyIconText, Sprite iconSprite, int order, string tooltip)
        {
            Key = key;
            LegacyIconText = legacyIconText;
            IconSprite = iconSprite;
            Order = order;
            Tooltip = tooltip;
        }

        public string Key { get; }
        public string LegacyIconText { get; }
        public Sprite IconSprite { get; }
        public int Order { get; }
        public string Tooltip { get; }
    }
}
