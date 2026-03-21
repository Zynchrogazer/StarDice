using UnityEngine;

/// <summary>
/// Scriptable Object สำหรับ "Effect กับดัก"
/// ทำหน้าที่เก็บข้อมูลเฉพาะทางของกับดัก (เช่น ความเสียหาย)
/// และมีตรรกะการทำงานเมื่อผู้เล่นตกบนช่องนี้
/// </summary>
[CreateAssetMenu(fileName = "NewTrapEffect", menuName = "Tile Effects/Trap Effect")]
public class TrapEffectSO : TileEffectSO
{
    [Header("Trap Settings")]
    [Tooltip("ความเสียหายเมื่อผู้เล่นตกบนช่องนี้")]
    public int damage = 10;

    [Tooltip("ผู้เล่นต้องหยุดเดินกี่ตา")]
    public int turnsToSkip = 1;

    /// <summary>
    /// เมธอดที่ถูกเรียกให้ทำงานโดย GameManager
    /// override มาจากคลาสแม่ TileEffectSO
    /// </summary>
    /// <param name="nodeData">ข้อมูลของโหนดที่ผู้เล่นอยู่</param>
    /// <param name="playerObject">GameObject ของผู้เล่น</param>
    public override void Execute(NodeConnection nodeData, GameObject playerObject, PlayerData playerData)
    {
        Debug.Log($"<color=red>กับดักทำงาน!</color> ที่ช่อง ID: {nodeData.tileID}");

        // ดึง Component PlayerData จาก GameObject ของผู้เล่นที่ถูกส่งเข้ามา
        //PlayerData playerData = playerObject.GetComponent<PlayerData>();

        // ตรวจสอบว่าหา PlayerData เจอก่อนที่จะเรียกใช้งาน
        if (playerData != null)
        {
            // เรียกใช้เมธอดบน PlayerData เพื่อสร้างความเสียหายและผลกระทบ
            
        }
        else
        {
            // แจ้งเตือนหากไม่พบ Component ที่จำเป็นบนตัวผู้เล่น
            Debug.LogError($"Could not find 'PlayerData' component on the player object: {playerObject.name}");
        }
    }
}