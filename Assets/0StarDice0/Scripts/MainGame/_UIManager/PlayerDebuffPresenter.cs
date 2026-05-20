using UnityEngine;

/// <summary>
/// Legacy presenter retained only for type compatibility.
/// Debuff HUD rendering is now handled exclusively by SimpleDebuffUI.
/// </summary>
public class PlayerDebuffPresenter
{
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
