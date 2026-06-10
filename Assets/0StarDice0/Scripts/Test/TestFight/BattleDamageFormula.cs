using UnityEngine;

public static class BattleDamageFormula
{
    public static int WithPlayerAttack(PlayerData player, int effectPower)
    {
        int baseAttack = player != null ? Mathf.Max(0, player.attackDamage) : 0;
        return Mathf.Max(0, effectPower + baseAttack);
    }
}
