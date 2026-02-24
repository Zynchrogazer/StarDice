using System;
using UnityEngine;

/// <summary>
/// ทำหน้าที่เป็นศูนย์กลางกระจายข่าวสาร (Event Hub) ของเกม
/// ใช้ Singleton Pattern เพื่อให้เข้าถึงได้ง่ายจากทุกที่
/// </summary>
public class EventManager : MonoBehaviour
{
    // Singleton Instance สำหรับให้คลาสอื่นเรียกใช้
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        // ตรวจสอบและกำหนดค่า Singleton Instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Event ที่จะถูกส่ง "เมื่อผู้เล่นเดินจบเทิร์นและหยุดที่ช่องสุดท้าย"
    /// โดยจะส่งข้อมูล NodeConnection ของช่องนั้น และ GameObject ของผู้เล่นไปด้วย
    /// </summary>
    public event Action<NodeConnection, GameObject> OnPlayerLandedOnNode;


    /// <summary>
    /// เมธอดสำหรับให้ระบบอื่น (เช่น PlayerPathWalker) เรียกใช้เพื่อส่ง Event
    /// </summary>
    /// <param name="nodeData">ข้อมูลของโหนดที่ผู้เล่นหยุด (จาก RouteManager)</param>
    /// <param name="playerObject">GameObject ของผู้เล่นที่ตกบนช่องนี้</param>
    public void RaisePlayerLandedOnNode(NodeConnection nodeData, GameObject playerObject)
    {
        // ตรวจสอบว่ามีใครรอฟัง Event นี้อยู่หรือไม่ ก่อนที่จะส่งออกไป
        OnPlayerLandedOnNode?.Invoke(nodeData, playerObject);
    }
}