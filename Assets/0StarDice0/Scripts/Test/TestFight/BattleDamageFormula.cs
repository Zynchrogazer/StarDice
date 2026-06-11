using UnityEngine;

public static class BattleDamageFormula
{
    private const float EffectAttackContribution = 0.35f;

    public static int WithPlayerAttack(PlayerData player, int effectPower)
    {
        int safeEffectPower = Mathf.Max(0, effectPower);
        int baseAttack = player != null ? player.GetBaseAttack() : 0;
        int attackContribution = Mathf.RoundToInt(baseAttack * EffectAttackContribution);
        return Mathf.Max(1, safeEffectPower + attackContribution);
    }
}
