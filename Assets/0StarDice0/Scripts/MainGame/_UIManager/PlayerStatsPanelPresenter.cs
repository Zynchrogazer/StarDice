using TMPro;
using UnityEngine;

public static class PlayerStatsPanelPresenter
{
    public static void Present(PlayerStatusPanelRefs panelRefs, PlayerState player)
    {
        if (panelRefs == null || player == null)
            return;

        SetText(panelRefs.statusMaxHpText, $"HP: {player.MaxHealth}");
        SetText(panelRefs.statusAttackText, $"ATK: {player.CurrentAttack}");
        SetText(panelRefs.statusSpeedText, $"SPD: {player.CurrentSpeed}");
        SetText(panelRefs.statusDefenseText, $"DEF: {player.CurrentDefense}");
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }
}
