using UnityEngine;

/// <summary>
/// Effect พิเศษสำหรับช่องประเภท Event โดยเฉพาะ
/// หน้าที่ของมันคือการนำ eventName จากข้อมูลของช่อง
/// ไปสั่งให้ GameEventManager ทำงานต่อ
/// </summary>
/// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX สำคัญมาก
[CreateAssetMenu(fileName = "GameEventEffect", menuName = "Tile Effects/Game Event Effect")]
public class GameEventEffectSO : TileEffectSO
{
    // เมธอด Execute จะทำงานเมื่อผู้เล่นตกบนช่อง Event
    public override void Execute(NodeConnection nodeData, GameObject playerObject, PlayerData playerData)
    {
        // ตรวจสอบว่ามี eventName กำหนดไว้ในช่องนั้นหรือไม่
        if (string.IsNullOrEmpty(nodeData.eventName))
        {
            Debug.LogWarning($"Landed on an Event tile (ID: {nodeData.tileID}) but no eventName was specified.");
            return;
        }

        Debug.Log($"<color=cyan>[GameEventEffect]</color> Landing on an event tile. Triggering event: '{nodeData.eventName}'");

        // เรียกใช้ GameEventManager ให้ทำงานตาม eventName ที่ระบุไว้
        GameEventManager.TryTriggerEvent(nodeData.eventName, playerObject);
    }
}