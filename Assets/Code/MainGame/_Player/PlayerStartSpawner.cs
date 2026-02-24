
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ใช้สำหรับสุ่มจุดเกิดของผู้เล่นจาก TileType.Start
/// จากนั้นส่งต่อไปยัง PlayerPathWalker เพื่อให้พร้อมเดิน
/// </summary>
public class PlayerStartSpawner : MonoBehaviour
{
    public RouteManager routeManager;
    public PlayerPathWalker playerPathWalker;

    void Start()
    {
        SpawnPlayerAtRandomStart();
    }

    public void SpawnPlayerAtRandomStart()
    {
        if (routeManager == null || playerPathWalker == null)
        {
            Debug.LogWarning("RouteManager หรือ PlayerPathWalker ยังไม่ได้เชื่อมต่อ");
            return;
        }

        // ค้นหา Node ที่มี TileType.Start
        List<NodeConnection> startNodes = routeManager.nodeConnections
            .Where(n => n.type == TileType.Start)
            .ToList();

        if (startNodes.Count == 0)
        {
            Debug.LogWarning("ไม่พบ Node ที่เป็น TileType.Start");
            return;
        }

        // สุ่มเลือกหนึ่ง Node
        NodeConnection randomStart = startNodes[Random.Range(0, startNodes.Count)];

        // ย้าย PlayerPathWalker ไปยังตำแหน่งนั้น
        playerPathWalker.TeleportToNode(randomStart.node); // TeleportToNode จะเซ็ตทั้ง position และ ID ให้เอง
        Debug.Log($"Spawned player at start node: {randomStart.node.name} (ID: {randomStart.tileID})");
    }
}
