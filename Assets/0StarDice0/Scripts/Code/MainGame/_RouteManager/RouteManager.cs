using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// enum ที่กำหนดประเภทของช่อง
public enum TileType { Normal , Event, Monster, Trap, Draw, Star, Teleport, Heal, Start, Boss, Minigame, Shop, Treasure, SpecialBoss, Lava}



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
public class RouteManager : MonoBehaviour
{
    private static RouteManager cachedManager;

    public static bool TryGet(out RouteManager manager)
    {
        if (cachedManager != null)
        {
            manager = cachedManager;
            return true;
        }

        manager = FindFirstObjectByType<RouteManager>();
        if (manager != null)
        {
            cachedManager = manager;
        }
        return manager != null;
    }

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

    // Dictionary สำหรับการค้นหาข้อมูลโหนดด้วยความเร็วสูงขณะเล่นเกม
    private Dictionary<int, NodeConnection> nodeDataMap;

    #region Unity Lifecycle & Editor
    private void Awake()
    {
        // ส่วนนี้สามารถทำงานได้ทั้งใน Editor และ Play Mode
        nodeDataMap = new Dictionary<int, NodeConnection>();
        foreach (var nc in nodeConnections)
        {
            if (nc != null && !nodeDataMap.ContainsKey(nc.tileID))
            {
                nodeDataMap.Add(nc.tileID, nc);
            }
        }

        // ▼▼▼ เพิ่มเงื่อนไขนี้เข้าไป ▼▼▼
        // ตรวจสอบว่าโค้ดกำลังรันใน Play Mode หรือไม่
        if (Application.isPlaying)
        {
            // โค้ดส่วนนี้จะทำงาน "เฉพาะตอนกด Play" เท่านั้น
            RouteManager[] managers = FindObjectsByType<RouteManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (managers.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            cachedManager = this;

            if (randomizeTilesOnGameStart)
            {
                RandomizeTilesAtGameStart();
            }
        }
    }

    private void OnDestroy()
    {
        if (cachedManager == this)
        {
            cachedManager = null;
        }
    }

    private void OnValidate()
    {
        // ทำงานใน Editor เท่านั้น เพื่อให้การปรับค่าใน Inspector เห็นผลทันที
        if (Application.isEditor)
        {
            SyncNodes();
            if (autoConnectSequential)
            {
                ConnectSequential();
            }

            if (autoApplyLockByTileIds)
            {
                ApplyLockFlagsFromTileIdList();
            }

            ApplyTileVisuals();
        }
    }

    private void SyncNodes()
    {
        // เก็บข้อมูลเก่าที่ตั้งค่าไว้ด้วยมือ (เช่น Type, EventName)
        var oldData = new Dictionary<Transform, NodeConnection>();
        foreach (var nc in nodeConnections)
        {
            if (nc != null && nc.node != null)
            {
                oldData[nc.node] = nc;
            }
        }

        nodeConnections.Clear();
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // เรียงลำดับ Node ตามตัวเลขในชื่อ
        children.Sort((a, b) => ExtractNumberFromName(a.name).CompareTo(ExtractNumberFromName(b.name)));

        foreach (Transform child in children)
        {
            NodeConnection nc = new NodeConnection { node = child };

            // กำหนด ID อัตโนมัติจากตัวเลขในชื่อ Node
            nc.tileID = ExtractNumberFromName(child.name);

            // นำข้อมูลเก่าที่เคยตั้งค่าไว้กลับมาใช้
            if (oldData.TryGetValue(child, out var saved))
            {
                nc.connectedNodes = saved.connectedNodes;
                nc.type = saved.type;
                nc.eventName = saved.eventName;
                nc.lockRandomType = saved.lockRandomType;
            }
            if (string.IsNullOrEmpty(nc.eventName))
            {
                nc.eventName = GetDefaultEventName(nc.type);
            }
            nodeConnections.Add(nc);
        }
    }


    [ContextMenu("Apply LockRandomType From lockTileIDs")]
    public void ApplyLockFlagsFromTileIdList()
    {
        HashSet<int> lockSet = new HashSet<int>(lockTileIDs);
        foreach (var nc in nodeConnections)
        {
            if (nc == null || nc.node == null)
            {
                continue;
            }

            if (clearOtherLocksWhenApplyingList)
            {
                nc.lockRandomType = false;
            }

            if (lockSet.Contains(nc.tileID))
            {
                nc.lockRandomType = true;
            }
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

        string message = lockedIds.Count > 0
            ? string.Join(", ", lockedIds)
            : "(none)";

        Debug.Log($"[RouteManager] Locked tile IDs: {message}");
    }



    public void RandomizeTilesAtGameStart()
    {
        var unlockedNodes = nodeConnections
            .Where(nc => nc != null && nc.node != null && !nc.lockRandomType)
            .ToList();

        if (unlockedNodes.Count == 0)
        {
            Debug.LogWarning("[RouteManager] ไม่พบช่องที่สุ่มได้ (อาจถูกล็อกทั้งหมด)");
            return;
        }

        var originalUnlockedTypes = unlockedNodes.Select(nc => nc.type).ToList();
        int seedToUse = useDeterministicSeed ? randomSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        System.Random rng = new System.Random(seedToUse);

        int attempts = Mathf.Max(1, maxRandomizeAttempts);
        bool success = false;

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            RestoreUnlockedTypes(unlockedNodes, originalUnlockedTypes);

            if (tileRandomMode == TileRandomMode.FullShuffle)
            {
                ApplyFullShuffle(unlockedNodes, rng);
            }
            else
            {
                ApplyLimitShuffle(unlockedNodes, rng);
            }

            if (!validateInvariantsAfterRandom || ValidateTileInvariants())
            {
                success = true;
                break;
            }
        }

        if (!success)
        {
            Debug.LogWarning("[RouteManager] สุ่มครบจำนวนครั้งแล้วแต่ invariant ไม่ผ่าน -> revert เป็นค่าก่อนสุ่ม");
            RestoreUnlockedTypes(unlockedNodes, originalUnlockedTypes);
        }

        ApplyTileVisuals();
        RebuildNodeDataMap();
    }

    void RestoreUnlockedTypes(List<NodeConnection> unlockedNodes, List<TileType> originalUnlockedTypes)
    {
        for (int i = 0; i < unlockedNodes.Count && i < originalUnlockedTypes.Count; i++)
        {
            unlockedNodes[i].type = originalUnlockedTypes[i];
            unlockedNodes[i].eventName = GetDefaultEventName(originalUnlockedTypes[i]);
        }
    }

    void ApplyFullShuffle(List<NodeConnection> unlockedNodes, System.Random rng)
    {
        List<TileType> types = unlockedNodes.Select(nc => nc.type).ToList();

        for (int i = types.Count - 1; i > 0; i--)
        {
            int randomIndex = rng.Next(0, i + 1);
            TileType temp = types[i];
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }

        for (int i = 0; i < unlockedNodes.Count; i++)
        {
            unlockedNodes[i].type = types[i];
            unlockedNodes[i].eventName = GetDefaultEventName(types[i]);
        }
    }

    void ApplyLimitShuffle(List<NodeConnection> unlockedNodes, System.Random rng)
    {
        Dictionary<TileType, int> maxByType = new Dictionary<TileType, int>();
        foreach (var limit in tileRandomLimits)
        {
            maxByType[limit.type] = limit.maxCount;
        }

        IEnumerable<NodeConnection> countedNodes = excludeLockedTilesFromLimitCounts
            ? unlockedNodes
            : nodeConnections.Where(nc => nc != null && nc.node != null);

        Dictionary<TileType, int> currentCounts = countedNodes
            .GroupBy(nc => nc.type)
            .ToDictionary(g => g.Key, g => g.Count());

        var randomOrderNodes = unlockedNodes.OrderBy(_ => rng.Next()).ToList();
        System.Array allTypes = System.Enum.GetValues(typeof(TileType));

        HashSet<TileType> allowSet = new HashSet<TileType>();
        if (useLimitAllowList)
        {
            foreach (var type in limitAllowedTypes)
            {
                allowSet.Add(type);
            }
        }

        foreach (var node in randomOrderNodes)
        {
            List<TileType> allowedTypes = new List<TileType>();
            foreach (TileType tileType in allTypes)
            {
                if (useLimitAllowList && !allowSet.Contains(tileType))
                {
                    continue;
                }

                int currentCount = currentCounts.ContainsKey(tileType) ? currentCounts[tileType] : 0;
                if (!maxByType.TryGetValue(tileType, out int maxCount) || currentCount < maxCount)
                {
                    allowedTypes.Add(tileType);
                }
            }

            TileType selectedType = allowedTypes.Count > 0
                ? allowedTypes[rng.Next(0, allowedTypes.Count)]
                : fallbackTileType;

            if (currentCounts.ContainsKey(node.type))
            {
                currentCounts[node.type] = Mathf.Max(0, currentCounts[node.type] - 1);
            }

            node.type = selectedType;
            node.eventName = GetDefaultEventName(selectedType);
            currentCounts[selectedType] = (currentCounts.ContainsKey(selectedType) ? currentCounts[selectedType] : 0) + 1;
        }
    }

    bool ValidateTileInvariants()
    {
        if (tileInvariantRules == null || tileInvariantRules.Count == 0)
        {
            return true;
        }

        Dictionary<TileType, int> counts = nodeConnections
            .Where(nc => nc != null && nc.node != null)
            .GroupBy(nc => nc.type)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var rule in tileInvariantRules)
        {
            int current = counts.ContainsKey(rule.type) ? counts[rule.type] : 0;
            int max = rule.maxCount < 0 ? int.MaxValue : rule.maxCount;
            if (current < rule.minCount || current > max)
            {
                return false;
            }
        }

        return true;
    }

    static readonly Dictionary<TileType, string> DefaultEventNames = new Dictionary<TileType, string>
    {
        { TileType.Star, "star" },
        { TileType.Monster, "battle" },
        { TileType.Event, "randomevent" },
        { TileType.Boss, "boss" },
        { TileType.Trap, "trap" },
        { TileType.Heal, "heal" },
        { TileType.Teleport, "warp" },
        { TileType.Minigame, "randomminigame" },
        { TileType.SpecialBoss, "specialboss" },
        { TileType.Draw, "draw" },
        { TileType.Shop, "shop" },
        { TileType.Start, "start" },
        { TileType.Treasure, "treasurebox" },
        { TileType.Lava, "lava" }
    };

    string GetDefaultEventName(TileType type)
    {
        if (DefaultEventNames.TryGetValue(type, out string eventName))
        {
            return eventName;
        }

        return string.Empty;
    }

    void RebuildNodeDataMap()
    {
        nodeDataMap = new Dictionary<int, NodeConnection>();
        foreach (var nc in nodeConnections)
        {
            if (nc != null && !nodeDataMap.ContainsKey(nc.tileID))
            {
                nodeDataMap.Add(nc.tileID, nc);
            }
        }
    }

    public void ConnectSequential()
    {
        if (clearPreviousConnectionsOnAutoConnect)
        {
            foreach (var nc in nodeConnections)
            {
                if (nc != null)
                {
                    nc.connectedNodes.Clear();
                }
            }
        }

        for (int i = 0; i < nodeConnections.Count - 1; i++)
        {
            NodeConnection currentNc = nodeConnections[i];
            NodeConnection nextNc = nodeConnections[i + 1];

            if (currentNc != null && currentNc.node != null &&
                nextNc != null && nextNc.node != null)
            {
                if (!currentNc.connectedNodes.Contains(nextNc.node))
                {
                    currentNc.connectedNodes.Add(nextNc.node);
                }
            }
        }
    }

    public void ApplyTileVisuals()
    {
        foreach (var nc in nodeConnections)
        {
            ApplyTileVisual(nc);
        }
    }

    void ApplyTileVisual(NodeConnection nc)
    {
        if (nc == null || nc.node == null) return;

        TileVisualSetting? setting = GetTileVisualSetting(nc.type);
        if (setting == null) return;

        TileVisualSetting visual = setting.Value;

        SpriteRenderer spriteRenderer = nc.node.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && visual.sprite != null)
        {
            spriteRenderer.sprite = visual.sprite;
        }

        Image uiImage = nc.node.GetComponent<Image>();
        if (uiImage != null && visual.sprite != null)
        {
            uiImage.sprite = visual.sprite;
        }

        Renderer meshRenderer = nc.node.GetComponent<Renderer>();
        if (meshRenderer == null)
        {
            meshRenderer = nc.node.GetComponentInChildren<Renderer>();
        }
        if (meshRenderer != null)
        {
            if (visual.material != null)
            {
                meshRenderer.sharedMaterial = visual.material;
            }

            if (visual.texture != null)
            {
                ApplyTextureToRenderer(meshRenderer, visual.texture);
            }
        }
    }

    TileVisualSetting? GetTileVisualSetting(TileType type)
    {
        foreach (var setting in tileVisualSettings)
        {
            if (setting.type == type)
            {
                return setting;
            }
        }

        return null;
    }

    void ApplyTextureToRenderer(Renderer renderer, Texture texture)
    {
        if (renderer == null || texture == null) return;

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);

        if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_BaseMap"))
        {
            propertyBlock.SetTexture("_BaseMap", texture);
        }

        if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_MainTex"))
        {
            propertyBlock.SetTexture("_MainTex", texture);
        }

        renderer.SetPropertyBlock(propertyBlock);
    }
    #endregion
    private bool isWarpModeActive = false;
    public void StartWarpSelection()
    {
        if (isWarpModeActive) return;
        isWarpModeActive = true;

        Debug.Log(">>> เข้าสู่โหมดเลือกพื้นที่ Warp! (กรุณาคลิกที่ช่องบนฉาก)");

        // วนลูปทุกโหนดในลิสต์ เพื่อเปิดใช้งาน TileClickable
        foreach (var nc in nodeConnections)
        {
            if (nc.node != null)
            {
                var tileScript = nc.node.GetComponent<TileClickable>();
                if (tileScript != null)
                {
                    // 🔴 แก้ชื่อฟังก์ชันตรงนี้ให้ตรงกับ TileClickable.cs
                    tileScript.SetSelectable(true);
                }
            }
        }
    }

    /// <summary>
    /// 2. เมื่อผู้เล่นกดเลือกช่องเสร็จแล้ว (ถูกเรียกจาก TileClickable)
    /// </summary>
    public void OnTileClicked(Transform selectedNode)
    {
        // ถ้าฟังก์ชันนี้ Error แปลว่าคุณยังไม่ได้ใส่ใน RouteManager

        if (!isWarpModeActive) return;

        // ปิดโหมดเลือกทันที
        isWarpModeActive = false;

        // วนลูปเพื่อคืนค่าสีเดิมให้ทุกช่อง
        foreach (var nc in nodeConnections)
        {
            if (nc.node != null)
            {
                var tileScript = nc.node.GetComponent<TileClickable>();
                if (tileScript != null)
                {
                    tileScript.SetSelectable(false);
                }
            }
        }

        // ✅ แก้ไข: สั่งให้ผู้เล่น "คนปัจจุบัน" วาร์ปไปที่นั่น
        if (GameTurnManager.CurrentPlayer != null)
        {
            // ดึงขา (Walker) ของคนปัจจุบันออกมา
            PlayerPathWalker walker = GameTurnManager.CurrentPlayer.GetComponent<PlayerPathWalker>();

            if (walker != null)
            {
                // ใช้คำสั่ง TeleportToNode ที่มีอยู่จริงในสคริปต์
                walker.TeleportToNode(selectedNode);
                Debug.Log($"🛸 Warped {GameTurnManager.CurrentPlayer.name} to {selectedNode.name}");
            }
        }
    }

    #region Public API
    /// <summary>
    /// ดึงข้อมูลทั้งหมดของโหนดจาก tileID (ทำงานเร็วมาก)
    /// </summary>
    public NodeConnection GetNodeData(int tileID)
    {
        if (nodeDataMap == null)
        {
            // กรณีที่ถูกเรียกใช้ใน Editor ก่อน Awake
            Awake();
        }
        nodeDataMap.TryGetValue(tileID, out NodeConnection data);
        return data;
    }

    /// <summary>
    /// ดึง List ของโหนดถัดไปที่เป็นไปได้จากโหนดปัจจุบัน
    /// </summary>
    public List<Transform> GetAllConnectedNodes(Transform currentNode)
    {
        NodeConnection nc = nodeConnections.Find(x => x.node == currentNode);
        if (nc != null)
        {
            return nc.connectedNodes;
        }
        return new List<Transform>(); // Return empty list if not found
    }

    /// <summary>
    /// เมธอดช่วยสำหรับดึงตัวเลขออกจากชื่อของ GameObject
    /// </summary>
    public int ExtractNumberFromName(string name)
    {
        Match match = Regex.Match(name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int result))
        {
            return result;
        }
        // คืนค่าที่สูงมากเพื่อให้โหนดที่ไม่มีตัวเลขไปอยู่ท้ายสุดตอนเรียงลำดับ
        return int.MaxValue;
    }
    #endregion


    #region Gizmos
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 10;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        foreach (var nc in nodeConnections)
        {
            if (nc == null || nc.node == null) continue;
            Vector3 from = nc.node.position;

            // วาด Sphere พร้อมกำหนดสีตามประเภทของช่อง
            //Gizmos.color = GetColorForTileType(nc.type);
            //Gizmos.DrawSphere(from, 0.1f);

            #if UNITY_EDITOR
            Handles.Label(from + Vector3.up * 0.1f, nc.node.name, labelStyle);
            #endif

            // วาดเส้นเชื่อม
            Gizmos.color = Color.green;
            foreach (var toNode in nc.connectedNodes)
            {
                if (toNode == null) continue;
                Gizmos.DrawLine(from, toNode.position);
            }
        }
    }




#endif
    #endregion
    [Header("Boss Settings")]
    public GameObject bossPrefab;

    public void SpawnBossTile()
    {
        Debug.Log("⚡ RouteManager: รับคำสั่งเตรียมเสกบอส...");

        // สร้าง List ไว้เก็บช่องที่มีสิทธิ์เป็นบอสได้
        List<NodeConnection> candidateNodes = new List<NodeConnection>();

        foreach (var nc in nodeConnections)
        {
            // ✅ เงื่อนไขใหม่: ช่องอะไรก็ได้ ที่ไม่ใช่ Start, Shop, และ Teleport
            if (nc.type != TileType.Start &&
                nc.type != TileType.Shop &&
                nc.type != TileType.Teleport)
            {
                candidateNodes.Add(nc);
            }
        }

        // เช็คว่ามีช่องเหลือให้ลงไหม
        if (candidateNodes.Count > 0)
        {
            // สุ่มเลือกมา 1 ช่อง
            int randomIndex = Random.Range(0, candidateNodes.Count);
            NodeConnection targetNode = candidateNodes[randomIndex];

            // เก็บประเภทเดิมไว้ดูเล่น (เผื่อ Debug)
            TileType oldType = targetNode.type;

            // 👑 เปลี่ยนร่างเป็น BOSS
            targetNode.type = TileType.Boss;
            targetNode.eventName = "boss"; // บังคับให้เป็น event ต่อสู้
            ApplyTileVisual(targetNode);

            Debug.Log($"🔥 Boss Spawned at Tile ID: {targetNode.tileID} (Was: {oldType})");

            // เสกโมเดลบอส
            if (bossPrefab != null && targetNode.node != null)
            {
                Instantiate(bossPrefab, targetNode.node.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError("❌ ไม่เหลือช่องที่สามารถเสกบอสได้เลย! (มีแต่ Start, Shop, Teleport เต็มแมพ)");
        }
    }
}
