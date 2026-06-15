using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

// enum ที่กำหนดประเภทของช่อง
public enum TileType { Normal , Event, Monster, Trap, Draw, Star, Teleport, Heal, Start, Boss, Minigame, Shop, Treasure, SpecialBoss, Lava,iceeffect}



/// <summary>
/// คลาสที่เก็บข้อมูลทั้งหมดของโหนดแต่ละอัน ทั้งเส้นทางและคุณสมบัติของช่อง
/// </summary>
[System.Serializable]
public class NodeConnection 
{
    // ส่วนของเส้นทาง
    public Transform node;
    public List<Transform> connectedNodes = new List<Transform>();

    // ส่วนข้อมูลของช่อง
    [Header("Tile Data")]
    [Tooltip("ID ของช่องนี้ (จะถูกกำหนดอัตโนมัติจากชื่อของ Node)")]
    public int tileID;
    [Tooltip("ประเภทของช่องนี้")]
    public TileType type = TileType.Normal;
    [Tooltip("ข้อมูลเพิ่มเติมสำหรับบางประเภท เช่น ชื่อ Event")]
    public string eventName;
    [Tooltip("เปิดเพื่อล็อกช่องนี้ ไม่ให้ระบบสุ่มเปลี่ยนประเภท")]
    public bool lockRandomType;
}

public enum TileRandomMode { FullShuffle, LimitByType }

[System.Serializable]
public struct TileRandomLimit
{
    public TileType type;
    [Min(0)]
    public int maxCount;
}

[System.Serializable]
public struct TileInvariantRule
{
    public TileType type;
    [Min(0)]
    public int minCount;
    public int maxCount; // -1 = ไม่จำกัด
}

[System.Serializable]
public class RockObstacleState
{
    [Tooltip("tileID ของช่องที่มีหิน")]
    public int tileID;
    [Tooltip("ถ้าเปิด = หินยังขวางทางอยู่")]
    public bool isActive = true;
    [Tooltip("อ็อบเจ็กต์หินที่ถูก Spawn ไว้บนช่อง (ถ้ามี)")]
    public GameObject spawnedObject;
}

[System.Serializable]
public struct TileGenSettings
{
    public string name;
    public TileType type;
    public Material visualMaterial; // 🎨 ใส่ Material (สี/ลาย) ที่จะเปลี่ยนตรงนี้
    public int minCount;
}

[System.Serializable]
public struct TileVisualSetting
{
    public TileType type;
    [Tooltip("ใช้กับ SpriteRenderer/UI Image")]
    public Sprite sprite;
    [Tooltip("ใช้กับช่องแบบ 3D (Cube) ที่ต้องการเปลี่ยน Material ทั้งก้อน")]
    public Material material;
    [Tooltip("ใช้กับช่องแบบ 3D (Cube) ที่ต้องการเปลี่ยนเฉพาะ Texture")]
    public Texture texture;
}

[ExecuteAlways]
[RequireComponent(typeof(RouteGimmickController))]
public class RouteManager : MonoBehaviour
{
    private static RouteManager cachedManager;

    private readonly TileVisualCache tileVisualCache = new TileVisualCache();
    private readonly TileRandomizer tileRandomizer = new TileRandomizer();
    private Dictionary<int, NodeConnection> nodeDataMap;
    private RouteGimmickController gimmickController;

    [Header("Route Data")]
    [Tooltip("List ของ NodeConnection ทั้งหมดในบอร์ด")]
    public List<NodeConnection> nodeConnections = new List<NodeConnection>();

    [Header("Editor Tools")]
    [Tooltip("เปิด/ปิดการแสดงผล Gizmos ในหน้าต่าง Scene")]
    public bool showGizmos = true;
    [Tooltip("เปิด/ปิดการเชื่อมต่อโหนดตามลำดับโดยอัตโนมัติ")]
    public bool autoConnectSequential = false;
    [Tooltip("หากเปิดใช้งาน จะลบการเชื่อมต่อเก่าก่อนที่จะเชื่อมต่อใหม่อัตโนมัติ")]
    public bool clearPreviousConnectionsOnAutoConnect = true;

    [Header("Tile Visual Settings")]
    [Tooltip("กำหนดภาพของแต่ละชนิดช่อง (รองรับ Sprite, Material และ Texture)")]
    public List<TileVisualSetting> tileVisualSettings = new List<TileVisualSetting>();

    [Header("Auto Fill Controls")]
    [Tooltip("เปิด/ปิดการ auto fill eventName จาก TileType ตอน SyncNodes")]
    public bool autoFillEventNameOnSync = true;
    [Tooltip("เปิด/ปิดการ auto apply visual/texture จาก TileType ตอน SyncNodes")]
    public bool autoFillVisualOnSync = true;

    [Header("Tile Randomizer")]
    [Tooltip("สุ่มประเภทช่องทุกครั้งเมื่อเริ่มเกม")]
    public bool randomizeTilesOnGameStart = false;
    [Tooltip("รูปแบบการสุ่มช่อง")]
    public TileRandomMode tileRandomMode = TileRandomMode.FullShuffle;
    [Tooltip("ใช้กับโหมด LimitByType: กำหนดจำนวนสูงสุดของแต่ละประเภท")]
    public List<TileRandomLimit> tileRandomLimits = new List<TileRandomLimit>();
    [Tooltip("ประเภทช่องที่จะใช้เติมเมื่อประเภทที่ถูกจำกัดเต็มทั้งหมดแล้ว")]
    public TileType fallbackTileType = TileType.Normal;
    [Tooltip("ใช้ seed คงที่เพื่อให้สุ่มได้ผลลัพธ์เดิมทุกครั้ง")]
    public bool useDeterministicSeed = true;
    [Tooltip("seed สำหรับสุ่มช่อง (ค่าเดียวกัน = ผลลัพธ์เดิม)")]
    public int randomSeed = 12345;

    [Header("Limit Mode Controls")]
    [Tooltip("ถ้าเปิด จะสุ่มเฉพาะประเภทที่อยู่ใน allow list เท่านั้น")]
    public bool useLimitAllowList = false;
    [Tooltip("รายการประเภทที่อนุญาตให้สุ่มในโหมด LimitByType")]
    public List<TileType> limitAllowedTypes = new List<TileType>();
    [Tooltip("ถ้าเปิด ช่องที่ lockRandomType จะไม่ถูกนับรวมกับโควตา tileRandomLimits")]
    public bool excludeLockedTilesFromLimitCounts = true;

    [Header("Lock Tools")]
    [Tooltip("ถ้าเปิด จะ apply lockRandomType ตาม lockTileIDs อัตโนมัติใน Editor")]
    public bool autoApplyLockByTileIds = false;
    [Tooltip("รายการ tileID ที่ต้องการล็อกเป็น lockRandomType")]
    public List<int> lockTileIDs = new List<int>();
    [Tooltip("ถ้าเปิด ตอน apply lock list จะล้าง lockRandomType ของช่องอื่นก่อน")]
    public bool clearOtherLocksWhenApplyingList = false;

    [Header("Tile Invariant Validation")]
    [Tooltip("ตรวจ invariant หลังสุ่ม ถ้าไม่ผ่านจะสุ่มใหม่ตามจำนวนครั้งที่กำหนด")]
    public bool validateInvariantsAfterRandom = true;
    [Min(1)]
    [Tooltip("จำนวนครั้งสูงสุดที่พยายามสุ่มใหม่เมื่อ invariant ไม่ผ่าน")]
    public int maxRandomizeAttempts = 10;
    [Tooltip("กฎ min/max ของประเภทช่องสำคัญ (max = -1 คือไม่จำกัด)")]
    public List<TileInvariantRule> tileInvariantRules = new List<TileInvariantRule>();

    public static bool TryGet(out RouteManager manager)
    {
        if (cachedManager != null)
        {
            manager = cachedManager;
            return true;
        }

        manager = FindFirstObjectByType<RouteManager>();
        if (manager != null) cachedManager = manager;
        return manager != null;
    }

    private void Awake()
    {
        gimmickController = GetOrCreateGimmickController();
        RebuildNodeDataMap();

        if (!Application.isPlaying) return;

        RouteManager[] managers = FindObjectsByType<RouteManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        cachedManager = this;
        if (randomizeTilesOnGameStart) RandomizeTilesAtGameStart();
    }

    private void OnDestroy()
    {
        if (cachedManager == this) cachedManager = null;
    }

    private void OnValidate()
    {
        if (!Application.isEditor) return;

        tileVisualCache.MarkDirty();
        SyncNodesIfNeeded();
        AutoFillEventNamesForCurrentNodes();
        RunEditorAutomation();
        ApplyTileVisuals();
    }

    #region Node Sync & Route Data
    private void SyncNodesIfNeeded()
    {
        if (RouteNodeSynchronizer.ShouldSync(transform, nodeConnections)) SyncNodes();
    }

    private void SyncNodes()
    {
        RouteNodeSynchronizer.Sync(transform, nodeConnections, autoFillEventNameOnSync, autoFillVisualOnSync, ApplyTileVisual);
    }

    private void AutoFillEventNamesForCurrentNodes()
    {
        if (autoFillEventNameOnSync) RouteNodeSynchronizer.AutoFillEventNames(nodeConnections);
    }

    public void RebuildNodeDataMap()
    {
        nodeDataMap = new Dictionary<int, NodeConnection>();
        foreach (NodeConnection nc in nodeConnections)
        {
            if (nc != null && !nodeDataMap.ContainsKey(nc.tileID)) nodeDataMap.Add(nc.tileID, nc);
        }
    }

    public NodeConnection GetNodeData(int tileID)
    {
        if (nodeDataMap == null) RebuildNodeDataMap();
        nodeDataMap.TryGetValue(tileID, out NodeConnection data);
        return data;
    }

    public List<Transform> GetAllConnectedNodes(Transform currentNode)
    {
        NodeConnection nc = nodeConnections.Find(x => x.node == currentNode);
        return nc != null ? nc.connectedNodes : new List<Transform>();
    }

    public int ExtractNumberFromName(string name) => RouteNodeSynchronizer.ExtractNumberFromName(name);
    public string GetDefaultEventName(TileType type) => RouteTileMetadata.GetDefaultEventName(type);
    #endregion

    #region Editor Automation & Lock Tools
    private void RunEditorAutomation()
    {
        if (autoConnectSequential) ConnectSequential();
        if (autoApplyLockByTileIds) ApplyLockFlagsFromTileIdList();
    }

    [ContextMenu("Apply LockRandomType From lockTileIDs")]
    public void ApplyLockFlagsFromTileIdList()
    {
        HashSet<int> lockSet = new HashSet<int>(lockTileIDs);
        foreach (NodeConnection nc in nodeConnections)
        {
            if (nc == null || nc.node == null) continue;
            if (clearOtherLocksWhenApplyingList) nc.lockRandomType = false;
            if (lockSet.Contains(nc.tileID)) nc.lockRandomType = true;
        }
    }

    [ContextMenu("Log Locked Tile IDs")]
    public void LogLockedTileIds()
    {
        List<int> lockedIds = nodeConnections
            .Where(nc => nc != null && nc.node != null && nc.lockRandomType)
            .Select(nc => nc.tileID)
            .OrderBy(id => id)
            .ToList();

        Debug.Log($"[RouteManager] Locked tile IDs: {(lockedIds.Count > 0 ? string.Join(", ", lockedIds) : "(none)")}");
    }

    public void ConnectSequential()
    {
        if (clearPreviousConnectionsOnAutoConnect)
        {
            foreach (NodeConnection nc in nodeConnections) nc?.connectedNodes.Clear();
        }

        for (int i = 0; i < nodeConnections.Count - 1; i++)
        {
            NodeConnection currentNc = nodeConnections[i];
            NodeConnection nextNc = nodeConnections[i + 1];
            if (currentNc?.node != null && nextNc?.node != null && !currentNc.connectedNodes.Contains(nextNc.node))
            {
                currentNc.connectedNodes.Add(nextNc.node);
            }
        }
    }
    #endregion

    #region Tile Randomization
    public void RandomizeTilesAtGameStart() => RandomizeTiles();

    public bool RandomizeTiles(IEnumerable<TileType> injectedTileTypes = null, bool forceRandomSeed = false)
    {
        List<NodeConnection> unlockedNodes = nodeConnections
            .Where(nc => nc != null && nc.node != null && !nc.lockRandomType)
            .ToList();

        if (unlockedNodes.Count == 0)
        {
            Debug.LogWarning("[RouteManager] ไม่พบช่องที่สุ่มได้ (อาจถูกล็อกทั้งหมด)");
            return false;
        }

        List<TileType> originalUnlockedTypes = unlockedNodes.Select(nc => nc.type).ToList();
        int seedToUse = useDeterministicSeed && !forceRandomSeed ? randomSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        System.Random rng = new System.Random(seedToUse);

        bool success = tileRandomizer.Randomize(nodeConnections, unlockedNodes, originalUnlockedTypes, BuildRandomizerSettings(), rng, GetDefaultEventName);
        if (success) ApplyInjectedTileTypes(unlockedNodes, injectedTileTypes, rng);
        else Debug.LogWarning("[RouteManager] สุ่มครบจำนวนครั้งแล้วแต่ invariant ไม่ผ่าน -> revert เป็นค่าก่อนสุ่ม");

        ApplyTileVisuals();
        RebuildNodeDataMap();
        return success;
    }

    private void ApplyInjectedTileTypes(List<NodeConnection> unlockedNodes, IEnumerable<TileType> injectedTileTypes, System.Random rng)
    {
        if (injectedTileTypes == null || unlockedNodes == null || unlockedNodes.Count == 0) return;

        List<NodeConnection> candidates = new List<NodeConnection>(unlockedNodes);
        foreach (TileType injectedType in injectedTileTypes)
        {
            if (candidates.Count == 0) candidates.AddRange(unlockedNodes);
            int randomIndex = rng.Next(0, candidates.Count);
            NodeConnection selectedNode = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);
            selectedNode.type = injectedType;
            selectedNode.eventName = GetDefaultEventName(injectedType);
        }
    }

    private TileRandomizerSettings BuildRandomizerSettings()
    {
        return new TileRandomizerSettings
        {
            mode = tileRandomMode,
            fallbackTileType = fallbackTileType,
            tileRandomLimits = tileRandomLimits,
            useLimitAllowList = useLimitAllowList,
            limitAllowedTypes = limitAllowedTypes,
            excludeLockedTilesFromLimitCounts = excludeLockedTilesFromLimitCounts,
            validateInvariantsAfterRandom = validateInvariantsAfterRandom,
            maxRandomizeAttempts = maxRandomizeAttempts,
            tileInvariantRules = tileInvariantRules
        };
    }
    #endregion

    #region Tile Visuals
    public void ApplyTileVisuals()
    {
        foreach (NodeConnection nc in nodeConnections) ApplyTileVisual(nc);
    }

    public void ApplyTileVisual(NodeConnection nc)
    {
        if (nc == null || nc.node == null) return;
        TileVisualSetting? setting = tileVisualCache.Get(nc.type, tileVisualSettings);
        if (setting != null) TileVisualApplier.Apply(nc.node, setting.Value);
    }
    #endregion

    #region Gimmick Facade
    public void StartWarpSelection() => Gimmicks.StartWarpSelection();
    public void OnTileClicked(Transform selectedNode) => Gimmicks.OnTileClicked(selectedNode);
    public bool TrySpawnRandomRockObstacle() => Gimmicks.TrySpawnRandomRockObstacle();
    public int TrySpawnRandomRockObstacles(int amount) => Gimmicks.TrySpawnRandomRockObstacles(amount);
    public bool IsRockObstacleActive(int tileID) => Gimmicks.IsRockObstacleActive(tileID);
    public int CountActiveRockObstacles() => Gimmicks.CountActiveRockObstacles();
    public bool TryBreakRockObstacle(int tileID) => Gimmicks.TryBreakRockObstacle(tileID);
    public bool ActivateRockObstacle(int tileID) => Gimmicks.ActivateRockObstacle(tileID);
    public void SpawnBossTile() => Gimmicks.SpawnBossTile();

    private RouteGimmickController Gimmicks
    {
        get
        {
            if (gimmickController == null) gimmickController = GetOrCreateGimmickController();
            return gimmickController;
        }
    }

    private RouteGimmickController GetOrCreateGimmickController()
    {
        RouteGimmickController controller = GetComponent<RouteGimmickController>();
        return controller != null ? controller : gameObject.AddComponent<RouteGimmickController>();
    }
    #endregion

    #region Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        GUIStyle labelStyle = new GUIStyle
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        labelStyle.normal.textColor = Color.white;

        foreach (NodeConnection nc in nodeConnections)
        {
            if (nc == null || nc.node == null) continue;

            Vector3 from = nc.node.position;
            Handles.Label(from + Vector3.up * 0.1f, nc.node.name, labelStyle);
            Gizmos.color = Color.green;
            foreach (Transform toNode in nc.connectedNodes)
            {
                if (toNode != null) Gizmos.DrawLine(from, toNode.position);
            }
        }
    }
#endif
    #endregion
}
