using UnityEngine;

/// <summary>
/// Interface กลางสำหรับ Effect ทุกประเภทในเกม
/// ทำหน้าที่เป็น "สัญญา" ว่าคลาสใดๆ ที่ต้องการเป็น Effect จะต้องมีเมธอด Execute
/// </summary>
public interface ITileEffect
{
    /// <summary>
    /// เมธอดหลักที่จะถูกเรียกให้ทำงานเมื่อผู้เล่นตกบนช่อง
    /// </summary>
    /// <param name="nodeData">ข้อมูลทั้งหมดของโหนดที่ผู้เล่นอยู่ (จาก RouteManager)</param>
    /// <param name="playerObject">GameObject ของผู้เล่นที่ตกบนช่องนี้</param>
    void Execute(NodeConnection nodeData, GameObject playerObject, PlayerData playerData);
}