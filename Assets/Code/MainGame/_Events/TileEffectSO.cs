using UnityEngine;

/// <summary>
/// คลาสแม่แบบ (Abstract) สำหรับ Effect ที่เป็น Scriptable Object ทั้งหมด
/// คลาสนี้ไม่สามารถนำไปสร้างเป็น Asset ได้โดยตรง แต่ต้องถูกสืบทอด (inherit) ก่อน
/// </summary>
public abstract class TileEffectSO : ScriptableObject, ITileEffect
{
    [Header("Effect Base Settings")]
    [Tooltip("ประเภทของ Tile ที่ Effect นี้จะทำงาน (สำคัญมากสำหรับ Registry)")]
    public TileType EffectType;

    [Tooltip("คำอธิบายสั้นๆ เกี่ยวกับ Effect นี้ สำหรับใช้ใน UI หรือเพื่ออ้างอิง")]
    [TextArea(2, 4)]
    public string description;

    /// <summary>
    /// เมธอด Execute ที่คลาสลูกต้อง override
    /// รับทั้ง playerObject (GameObject) และ playerData (ScriptableObject) เพื่อใช้ร่วมกัน
    /// </summary>
    /// <param name="nodeData">ข้อมูลโหนด</param>
    /// <param name="playerObject">GameObject ของผู้เล่น</param>
    /// <param name="playerData">ScriptableObject PlayerData</param>
    public abstract void Execute(NodeConnection nodeData, GameObject playerObject, PlayerData playerData);
    protected PlayerData GetPlayerDataFromState()
    {
        return PlayerState.Instance?.selectedPlayerPreset;
    }
}
