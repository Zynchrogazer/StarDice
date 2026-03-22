using TMPro;

public static class PlayerGlobalHudPresenter
{
    public static void Present(PlayerGlobalHudRefs hudRefs, PlayerState player)
    {
        if (hudRefs == null || player == null)
            return;

        SetText(hudRefs.currentHpText, $"HP: {player.PlayerHealth}/{player.MaxHealth}");
        SetText(hudRefs.creditText, $"Credit: {ResolvePersistentCredit(player)}");
        SetText(hudRefs.levelText, $"Lv. {player.PlayerLevel}");
    }

    private static int ResolvePersistentCredit(PlayerState player)
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return UnityEngine.Mathf.Max(0, GameData.Instance.selectedPlayer.Credit);

        if (player.selectedPlayerPreset != null)
            return UnityEngine.Mathf.Max(0, player.selectedPlayerPreset.Credit);

        return UnityEngine.Mathf.Max(0, player.PlayerCredit);
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }
}
