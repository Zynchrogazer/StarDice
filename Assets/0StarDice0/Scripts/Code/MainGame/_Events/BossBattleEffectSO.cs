using UnityEngine;

/// <summary>
/// Effect ที่จะทำงานเมื่อผู้เล่นตกบนช่อง Boss
/// หน้าที่หลักคือการเรียก BossBattleManager ให้เริ่มการต่อสู้
/// </summary>
[CreateAssetMenu(fileName = "BossBattleEffect", menuName = "Tile Effects/Boss Battle Effect")]
public class BossBattleEffectSO : TileEffectSO
{
    // เมธอด Execute จะทำงานเมื่อผู้เล่นตกบนช่อง Boss
    public override void Execute(NodeConnection nodeData, GameObject playerObject, PlayerData playerData)
    {
        Debug.Log($"<color=magenta>[BossBattleEffect]</color> Player has landed on a Boss tile! Preparing for battle...");

        // เรียกใช้ BossBattleManager ให้เริ่มการต่อสู้
        // เราจะสร้าง BossBattleManager ในขั้นตอนถัดไป
        BossBattleManager.Instance.StartBossBattle(playerObject);
    }
}