using TMPro;
using UnityEngine;

public static class PlayerStatsPanelPresenter
{
    public static void Present(PlayerStatusPanelRefs panelRefs, PlayerState player)
    {
        if (panelRefs == null || player == null)
            return;

        SetText(panelRefs.statusMaxHpText, $"HP: {player.MaxHealth}");
        SetText(panelRefs.hudCurrentHpText, $"HP: {player.PlayerHealth}/{player.MaxHealth}");
        SetText(panelRefs.hudCreditText, $"Credit: {ResolvePersistentCredit(player)}");

        SetText(panelRefs.hudLevelText, $"Lv. {player.PlayerLevel}");
        SetText(panelRefs.statusAttackText, $"ATK: {player.CurrentAttack}");
        SetText(panelRefs.statusSpeedText, $"SPD: {player.CurrentSpeed}");
        SetText(panelRefs.statusDefenseText, $"DEF: {player.CurrentDefense}");
    }

    private static int ResolvePersistentCredit(PlayerState player)
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return Mathf.Max(0, GameData.Instance.selectedPlayer.Credit);

        if (player.selectedPlayerPreset != null)
            return Mathf.Max(0, player.selectedPlayerPreset.Credit);

        return Mathf.Max(0, player.PlayerCredit);
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }
}
