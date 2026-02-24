using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// จัดการตำแหน่งเริ่มต้นของผู้เล่นให้ตรงกับช่อง Start ของบอร์ด
/// และช่วยกู้สถานะเมื่อกลับจากฉากอื่นแล้วตำแหน่ง/Route อ้างอิงไม่ถูกต้อง
/// </summary>
public class PlayerStartSpawner : MonoBehaviour
{
    public RouteManager routeManager;
    public PlayerPathWalker playerPathWalker;

    [Header("Recovery Settings")]
    [Tooltip("ถ้าเข้า scene บอร์ดแล้ว currentNode ไม่อยู่ใน map นี้ ให้ย้ายกลับ Start อัตโนมัติ")]
    public bool respawnWhenCurrentNodeInvalid = true;

    private void Start()
    {
        ResolveReferences();
        SpawnPlayerAtRandomStart();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveReferences();

        // ไม่เจอ RouteManager = ไม่ใช่ฉากบอร์ด/ยังไม่พร้อม
        if (routeManager == null || playerPathWalker == null)
            return;

        if (!respawnWhenCurrentNodeInvalid)
            return;

        NodeConnection currentNodeData = routeManager.GetNodeData(playerPathWalker.currentNodeID);
        if (currentNodeData == null || currentNodeData.node == null)
        {
            Debug.LogWarning($"[PlayerStartSpawner] Current node ID {playerPathWalker.currentNodeID} is invalid in scene '{scene.name}'. Respawning to Start tile.");
            SpawnPlayerAtRandomStart();
        }
    }

    private void ResolveReferences()
    {
        if (playerPathWalker == null)
            playerPathWalker = GetComponent<PlayerPathWalker>();

        if (routeManager == null)
            routeManager = FindObjectOfType<RouteManager>(true);

        if (routeManager == null)
            routeManager = RouteManager.Instance;
    }

    public void SpawnPlayerAtRandomStart()
    {
        if (!ResolveAndValidate())
            return;

        List<NodeConnection> startNodes = routeManager.nodeConnections
            .Where(n => n != null && n.node != null && n.type == TileType.Start)
            .ToList();

        if (startNodes.Count == 0)
        {
            Debug.LogWarning("[PlayerStartSpawner] ไม่พบ Node ที่เป็น TileType.Start, fallback ไปช่องแรกที่ใช้งานได้");

            NodeConnection fallback = routeManager.nodeConnections
                .FirstOrDefault(n => n != null && n.node != null);

            if (fallback == null)
            {
                Debug.LogError("[PlayerStartSpawner] ไม่มี Node ที่ใช้งานได้ใน RouteManager");
                return;
            }

            playerPathWalker.TeleportToNode(fallback.node);
            Debug.Log($"[PlayerStartSpawner] Spawned with fallback node: {fallback.node.name} (ID: {fallback.tileID})");
            return;
        }

        NodeConnection randomStart = startNodes[Random.Range(0, startNodes.Count)];
        playerPathWalker.TeleportToNode(randomStart.node);
        Debug.Log($"[PlayerStartSpawner] Spawned player at start node: {randomStart.node.name} (ID: {randomStart.tileID})");
    }

    private bool ResolveAndValidate()
    {
        ResolveReferences();

        if (routeManager == null)
        {
            Debug.LogWarning("[PlayerStartSpawner] RouteManager ยังไม่ได้เชื่อมต่อ");
            return false;
        }

        if (playerPathWalker == null)
        {
            Debug.LogWarning("[PlayerStartSpawner] PlayerPathWalker ยังไม่ได้เชื่อมต่อ");
            return false;
        }

        if (routeManager.nodeConnections == null || routeManager.nodeConnections.Count == 0)
        {
            Debug.LogWarning("[PlayerStartSpawner] RouteManager.nodeConnections ว่าง");
            return false;
        }

        return true;
    }
}
