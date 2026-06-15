using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RouteGimmickController : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;

    [Header("Rock Obstacle Settings")]
    [Tooltip("Prefab ของหินที่ใช้วางเป็นสิ่งกีดขวาง (optional)")]
    public GameObject rockObstaclePrefab;
    [Tooltip("ระยะยกหินขึ้นจากตำแหน่ง node (หน่วยโลก)")]
    [Min(0f)] public float rockObstacleSpawnHeight = 0.02f;
    [Tooltip("ตำแหน่งช่องที่อยากให้มีหินตั้งแต่เริ่มเกม")]
    public List<int> initialRockTileIDs = new List<int>();
    [Tooltip("สุ่มวางหินเพิ่มตอนเริ่มฉาก (นอกเหนือจาก initialRockTileIDs)")]
    public bool randomSpawnRockOnStart = false;
    [Tooltip("จำนวนหินที่สุ่มเพิ่มตอนเริ่มฉาก")]
    [Min(0)] public int randomSpawnRockCountOnStart = 0;
    [Tooltip("สถานะหินที่กำลังใช้งานในเกม")]
    public List<RockObstacleState> activeRockObstacles = new List<RockObstacleState>();

    private readonly Dictionary<int, RockObstacleState> rockObstacleMap = new Dictionary<int, RockObstacleState>();
    private RouteManager routeManager;
    private bool isRockObstacleCacheInitialized;
    private bool isWarpModeActive;

    private void Awake()
    {
        routeManager = GetComponent<RouteManager>();
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        if (initialRockTileIDs != null)
        {
            foreach (int tileID in initialRockTileIDs)
            {
                ActivateRockObstacle(tileID);
            }
        }

        if (randomSpawnRockOnStart && randomSpawnRockCountOnStart > 0)
        {
            int spawnedCount = TrySpawnRandomRockObstacles(randomSpawnRockCountOnStart);
            if (spawnedCount > 0)
            {
                Debug.Log($"🪨 สุ่มหินตอนเริ่มฉาก {spawnedCount}/{randomSpawnRockCountOnStart} ก้อน");
            }
        }
    }

    public void StartWarpSelection()
    {
        if (isWarpModeActive) return;
        isWarpModeActive = true;
        Debug.Log(">>> เข้าสู่โหมดเลือกพื้นที่ Warp! (กรุณาคลิกที่ช่องบนฉาก)");
        SetAllTilesSelectable(true);
    }

    public void OnTileClicked(Transform selectedNode)
    {
        if (!isWarpModeActive) return;

        isWarpModeActive = false;
        SetAllTilesSelectable(false);
        WarpCurrentPlayerTo(selectedNode);
    }

    public bool TrySpawnRandomRockObstacle() => TrySpawnRandomRockObstacles(1) > 0;

    public int TrySpawnRandomRockObstacles(int amount)
    {
        if (amount <= 0 || routeManager.nodeConnections == null || routeManager.nodeConnections.Count == 0) return 0;

        EnsureRockObstacleCache();
        HashSet<int> occupiedPlayerTileIds = GetOccupiedPlayerTileIds();
        List<int> candidateTileIds = BuildRockCandidateTileIds(occupiedPlayerTileIds);
        int spawnTargetCount = Mathf.Min(amount, candidateTileIds.Count);
        int activatedCount = 0;

        for (int i = 0; i < spawnTargetCount; i++)
        {
            int randomIndex = Random.Range(0, candidateTileIds.Count);
            int targetTileId = candidateTileIds[randomIndex];
            candidateTileIds.RemoveAt(randomIndex);

            if (ActivateRockObstacle(targetTileId))
            {
                activatedCount++;
                Debug.Log($"🪨 สุ่มเสกหินที่ tile {targetTileId}");
            }
        }

        return activatedCount;
    }

    public bool IsRockObstacleActive(int tileID)
    {
        RockObstacleState state = GetOrCreateRockObstacleState(tileID, false);
        return state != null && state.isActive;
    }

    public int CountActiveRockObstacles()
    {
        EnsureRockObstacleCache();
        int activeCount = 0;
        foreach (RockObstacleState state in rockObstacleMap.Values)
        {
            if (state != null && state.isActive) activeCount++;
        }

        return activeCount;
    }

    public bool TryBreakRockObstacle(int tileID)
    {
        RockObstacleState state = GetOrCreateRockObstacleState(tileID, false);
        if (state == null || !state.isActive) return false;

        state.isActive = false;
        if (state.spawnedObject != null)
        {
            Destroy(state.spawnedObject);
            state.spawnedObject = null;
        }

        Debug.Log($"🪨 Rock obstacle on tile {tileID} was broken.");
        return true;
    }

    public bool ActivateRockObstacle(int tileID)
    {
        NodeConnection nodeData = routeManager.GetNodeData(tileID);
        if (nodeData == null || nodeData.node == null)
        {
            Debug.LogWarning($"[RouteGimmickController] ไม่พบ tileID {tileID} สำหรับวางหิน");
            return false;
        }

        RockObstacleState state = GetOrCreateRockObstacleState(tileID, true);
        state.isActive = true;
        SpawnRockVisualIfNeeded(nodeData, state);
        return true;
    }

    public void SpawnBossTile()
    {
        Debug.Log("⚡ RouteGimmickController: รับคำสั่งเตรียมเสกบอส...");
        List<NodeConnection> candidateNodes = BuildBossCandidateNodes();
        if (candidateNodes.Count == 0)
        {
            Debug.LogError("❌ ไม่เหลือช่องที่สามารถเสกบอสได้เลย! (มีแต่ Start, Shop, Teleport เต็มแมพ)");
            return;
        }

        NodeConnection targetNode = candidateNodes[Random.Range(0, candidateNodes.Count)];
        TileType oldType = targetNode.type;
        targetNode.type = TileType.Boss;
        targetNode.eventName = RouteTileMetadata.GetDefaultEventName(TileType.Boss);
        routeManager.ApplyTileVisual(targetNode);
        Debug.Log($"🔥 Boss Spawned at Tile ID: {targetNode.tileID} (Was: {oldType})");

        if (bossPrefab != null && targetNode.node != null)
        {
            Instantiate(bossPrefab, targetNode.node.position, Quaternion.identity);
            RequestEndTurnAfterGimmick();
        }
    }

    private void SetAllTilesSelectable(bool selectable)
    {
        foreach (NodeConnection nc in routeManager.nodeConnections)
        {
            TileClickable tileScript = nc?.node != null ? nc.node.GetComponent<TileClickable>() : null;
            if (tileScript != null) tileScript.SetSelectable(selectable);
        }
    }

    private void WarpCurrentPlayerTo(Transform selectedNode)
    {
        if (GameTurnManager.CurrentPlayer == null) return;

        PlayerPathWalker walker = GameTurnManager.CurrentPlayer.GetComponent<PlayerPathWalker>();
        if (walker == null) return;

        walker.TeleportToNode(selectedNode);
        Debug.Log($"🛸 Warped {GameTurnManager.CurrentPlayer.name} to {selectedNode.name}");
        RequestEndTurnAfterGimmick();
    }

    private void RequestEndTurnAfterGimmick()
    {
        GameEventManager eventManager = FindObjectOfType<GameEventManager>();
        if (eventManager != null)
        {
            eventManager.StartCoroutine(eventManager.WaitAndEndTurn());
            return;
        }

        Debug.LogError("หา GameEventManager ไม่เจอ! ลืมลากใส่ฉากหรือเปล่า?");
    }

    private List<int> BuildRockCandidateTileIds(HashSet<int> occupiedPlayerTileIds)
    {
        List<int> candidateTileIds = new List<int>();
        foreach (NodeConnection nodeData in routeManager.nodeConnections)
        {
            if (nodeData == null || nodeData.node == null || nodeData.tileID <= 0) continue;
            if (IsRockObstacleActive(nodeData.tileID) || occupiedPlayerTileIds.Contains(nodeData.tileID)) continue;
            if (nodeData.type == TileType.Start || nodeData.type == TileType.Shop || nodeData.type == TileType.Teleport) continue;
            candidateTileIds.Add(nodeData.tileID);
        }

        return candidateTileIds;
    }

    private List<NodeConnection> BuildBossCandidateNodes()
    {
        List<NodeConnection> candidateNodes = new List<NodeConnection>();
        foreach (NodeConnection nc in routeManager.nodeConnections)
        {
            if (nc != null && nc.type != TileType.Start && nc.type != TileType.Shop && nc.type != TileType.Teleport)
            {
                candidateNodes.Add(nc);
            }
        }

        return candidateNodes;
    }

    private static HashSet<int> GetOccupiedPlayerTileIds()
    {
        HashSet<int> occupiedTileIds = new HashSet<int>();
        PlayerPathWalker[] walkers = FindObjectsByType<PlayerPathWalker>(FindObjectsSortMode.None);
        foreach (PlayerPathWalker walker in walkers)
        {
            if (walker != null && walker.currentNodeID > 0) occupiedTileIds.Add(walker.currentNodeID);
        }

        return occupiedTileIds;
    }

    private void SpawnRockVisualIfNeeded(NodeConnection nodeData, RockObstacleState state)
    {
        if (rockObstaclePrefab == null || state.spawnedObject != null) return;

        state.spawnedObject = Instantiate(rockObstaclePrefab, nodeData.node.position, rockObstaclePrefab.transform.rotation);
        MoveRockObstacleToRouteScene(state.spawnedObject);
        state.spawnedObject.transform.position = GetRockObstacleSpawnPosition(nodeData.node, state.spawnedObject);
    }

    private void MoveRockObstacleToRouteScene(GameObject rockInstance)
    {
        if (rockInstance == null) return;

        Scene routeScene = gameObject.scene;
        if (routeScene.IsValid() && routeScene.isLoaded && rockInstance.scene.handle != routeScene.handle)
        {
            SceneManager.MoveGameObjectToScene(rockInstance, routeScene);
        }
    }

    private Vector3 GetRockObstacleSpawnPosition(Transform nodeTransform, GameObject rockInstance)
    {
        Vector3 spawnPosition = nodeTransform.position;
        float targetGroundY = nodeTransform.position.y;

        if (TryGetCombinedBounds(nodeTransform, out Bounds nodeBounds))
        {
            spawnPosition.x = nodeBounds.center.x;
            spawnPosition.z = nodeBounds.center.z;
            targetGroundY = nodeBounds.max.y;
        }

        if (rockInstance != null && TryGetCombinedBounds(rockInstance.transform, out Bounds rockBounds))
        {
            float rockBottomOffset = rockBounds.min.y - rockInstance.transform.position.y;
            spawnPosition.y = targetGroundY - rockBottomOffset + rockObstacleSpawnHeight;
            return spawnPosition;
        }

        spawnPosition.y = targetGroundY + rockObstacleSpawnHeight;
        return spawnPosition;
    }

    private static bool TryGetCombinedBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
            return true;
        }

        bounds = default;
        return false;
    }

    private RockObstacleState GetOrCreateRockObstacleState(int tileID, bool createIfMissing)
    {
        if (tileID <= 0) return null;
        EnsureRockObstacleCache();

        if (rockObstacleMap.TryGetValue(tileID, out RockObstacleState cached)) return cached;
        if (!createIfMissing) return null;

        RockObstacleState newState = new RockObstacleState { tileID = tileID, isActive = true };
        activeRockObstacles.Add(newState);
        rockObstacleMap[tileID] = newState;
        return newState;
    }

    private void EnsureRockObstacleCache()
    {
        if (isRockObstacleCacheInitialized) return;

        rockObstacleMap.Clear();
        if (activeRockObstacles != null)
        {
            foreach (RockObstacleState state in activeRockObstacles)
            {
                if (state != null && state.tileID > 0 && !rockObstacleMap.ContainsKey(state.tileID))
                {
                    rockObstacleMap[state.tileID] = state;
                }
            }
        }

        isRockObstacleCacheInitialized = true;
    }
}
