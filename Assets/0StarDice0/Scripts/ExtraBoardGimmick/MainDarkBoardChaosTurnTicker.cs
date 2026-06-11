using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// กิมมิค MainDark ฝั่งบอร์ด:
/// - นับเฉพาะเทิร์นผู้เล่นจริง (ไม่รวม AI)
/// - ทุก 10 เทิร์น สุ่มช่องใหม่ผ่าน RouteManager พร้อมใส่ช่อง Lava และ iceeffect ลงใน pool
/// - ทุก 15 เทิร์น Clone มอนสเตอร์บนบอร์ดเพิ่ม 1 ตัว
/// แยกออกจาก RouteManager เพื่อให้ RouteManager รับผิดชอบแค่ข้อมูล/การสุ่มช่อง (SRP + KISS)
/// </summary>
public class MainDarkBoardChaosTurnTicker : MonoBehaviour
{
    private static readonly TileType[] MainDarkInjectedTiles = { TileType.Lava, TileType.iceeffect };

    [Header("References")]
    [SerializeField] private RouteManager routeManager;

    [Header("MainDark Board Chaos")]
    [SerializeField] private bool enableBoardChaos = true;
    [SerializeField] private bool onlyInMainDark = true;
    [SerializeField] private string mainDarkSceneName = "MainDark";

    [Header("Tile Randomize")]
    [SerializeField, Min(1)] private int randomizeEveryPlayerTurns = 10;

    [Header("Monster Clone")]
    [SerializeField, Min(1)] private int cloneEveryPlayerTurns = 15;
    [SerializeField, Min(0)] private int maxMonstersOnBoard = 0;
    [SerializeField] private bool cloneOriginalMonsterFirst = true;
    [SerializeField] private Color cloneTint = new Color(0.65f, 0.35f, 1f, 1f);

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private int playerTurnCounter;
    private bool isSubscribedToTurnManager;

    private void Awake()
    {
        ResolveRouteManager();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TrySubscribeTurnManager();
    }

    private void Start()
    {
        if (!Application.isPlaying || isSubscribedToTurnManager)
        {
            return;
        }

        TrySubscribeTurnManager();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (isSubscribedToTurnManager && GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged -= HandleTurnChanged;
        }

        isSubscribedToTurnManager = false;
    }

    private void TrySubscribeTurnManager()
    {
        if (isSubscribedToTurnManager)
        {
            return;
        }

        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged += HandleTurnChanged;
            isSubscribedToTurnManager = true;
            Log("Subscribed OnTurnChanged");
        }
        else if (verboseLog)
        {
            Debug.LogWarning("[MainDarkBoardChaos] ไม่พบ GameTurnManager ขณะ Subscribe");
        }
    }

    private void HandleTurnChanged(bool isAITurn)
    {
        if (!enableBoardChaos || isAITurn || !IsAllowedScene())
        {
            return;
        }

        playerTurnCounter++;

        if (playerTurnCounter % randomizeEveryPlayerTurns == 0)
        {
            RandomizeMainDarkTiles();
        }

        if (playerTurnCounter % cloneEveryPlayerTurns == 0)
        {
            TryCloneMonsterOnBoard();
        }
    }

    private void RandomizeMainDarkTiles()
    {
        if (!ResolveRouteManager())
        {
            return;
        }

        bool success = routeManager.RandomizeTiles(MainDarkInjectedTiles, true);
        if (success)
        {
            Log($"สุ่มช่องใหม่พร้อม Lava/Ice สำเร็จในเทิร์นผู้เล่นที่ {playerTurnCounter}");
        }
    }

    private void TryCloneMonsterOnBoard()
    {
        if (!ResolveRouteManager())
        {
            return;
        }

        List<PlayerState> allMonsters = GetBoardMonsters(true);
        if (allMonsters.Count == 0)
        {
            Log("ไม่มีมอนสเตอร์บนบอร์ดให้ clone");
            return;
        }

        if (maxMonstersOnBoard > 0 && allMonsters.Count >= maxMonstersOnBoard)
        {
            Log($"ถึงเพดานมอนสเตอร์แล้ว ({allMonsters.Count}/{maxMonstersOnBoard})");
            return;
        }

        PlayerState sourceMonster = PickSourceMonster(GetBoardMonsters(false));
        NodeConnection spawnNode = PickFreeMonsterSpawnNode();
        if (sourceMonster == null)
        {
            Log("ไม่มี original monster ให้ clone (ไม่ clone จากร่าง clone เพื่อกันจำนวนทวีคูณ)");
            return;
        }

        if (spawnNode == null || spawnNode.node == null)
        {
            Log("ไม่มีช่องว่างสำหรับ clone มอนสเตอร์");
            return;
        }

        GameObject cloneObject = Instantiate(sourceMonster.gameObject, spawnNode.node.position, sourceMonster.transform.rotation);
        cloneObject.name = $"{sourceMonster.gameObject.name}_MainDarkClone_{playerTurnCounter}";
        cloneObject.SetActive(true);

        MainDarkMonsterCloneMarker marker = cloneObject.GetComponent<MainDarkMonsterCloneMarker>();
        if (marker == null)
        {
            marker = cloneObject.AddComponent<MainDarkMonsterCloneMarker>();
        }
        marker.Initialize(sourceMonster);
        ApplyCloneTint(cloneObject);

        PlayerPathWalker cloneWalker = cloneObject.GetComponent<PlayerPathWalker>();
        if (cloneWalker != null)
        {
            cloneWalker.ReconnectReferences(routeManager);
            cloneWalker.TeleportToNode(spawnNode.node);
            cloneWalker.currentNodeID = spawnNode.tileID;
        }

        PlayerState cloneState = cloneObject.GetComponent<PlayerState>();
        if (cloneState != null && GameTurnManager.TryGet(out var gameTurnManager) && !gameTurnManager.allPlayers.Contains(cloneState))
        {
            gameTurnManager.allPlayers.Add(cloneState);
        }

        Debug.Log($"<color=purple>[MainDarkBoardChaos] Clone มอนสเตอร์ {sourceMonster.name} เพิ่มที่ Tile {spawnNode.tileID}</color>");
    }

    private PlayerState PickSourceMonster(List<PlayerState> originalMonsters)
    {
        if (originalMonsters == null || originalMonsters.Count == 0)
        {
            return null;
        }

        if (cloneOriginalMonsterFirst)
        {
            return originalMonsters[0];
        }

        return originalMonsters[Random.Range(0, originalMonsters.Count)];
    }

    private void ApplyCloneTint(GameObject cloneObject)
    {
        if (cloneObject == null)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers = cloneObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = cloneTint;
            }
        }
    }

    private NodeConnection PickFreeMonsterSpawnNode()
    {
        HashSet<int> occupiedTileIds = GetOccupiedTileIds();
        List<NodeConnection> candidates = new List<NodeConnection>();

        foreach (NodeConnection nc in routeManager.nodeConnections)
        {
            if (nc == null || nc.node == null || occupiedTileIds.Contains(nc.tileID))
            {
                continue;
            }

            if (nc.type == TileType.Start || nc.type == TileType.Shop || nc.type == TileType.Boss || nc.type == TileType.SpecialBoss)
            {
                continue;
            }

            candidates.Add(nc);
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    private List<PlayerState> GetBoardMonsters(bool includeClones)
    {
        List<PlayerState> monsters = new List<PlayerState>();
        Scene boardScene = routeManager != null ? routeManager.gameObject.scene : SceneManager.GetActiveScene();
        PlayerState[] players = FindObjectsByType<PlayerState>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < players.Length; i++)
        {
            PlayerState player = players[i];
            if (player == null || !player.isAI || player.gameObject.scene != boardScene)
            {
                continue;
            }

            if (!includeClones && MainDarkMonsterCloneMarker.TryGet(player.gameObject, out _))
            {
                continue;
            }

            monsters.Add(player);
        }

        return monsters;
    }

    private HashSet<int> GetOccupiedTileIds()
    {
        HashSet<int> occupiedTileIds = new HashSet<int>();
        PlayerPathWalker[] walkers = FindObjectsByType<PlayerPathWalker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < walkers.Length; i++)
        {
            PlayerPathWalker walker = walkers[i];
            if (walker != null)
            {
                occupiedTileIds.Add(walker.currentNodeID);
            }
        }

        return occupiedTileIds;
    }

    private bool ResolveRouteManager()
    {
        if (routeManager != null)
        {
            return true;
        }

        if (RouteManager.TryGet(out routeManager))
        {
            return true;
        }

        if (verboseLog)
        {
            Debug.LogWarning("[MainDarkBoardChaos] ไม่พบ RouteManager");
        }

        return false;
    }

    private bool IsAllowedScene()
    {
        return !onlyInMainDark || string.Equals(SceneManager.GetActiveScene().name, mainDarkSceneName, System.StringComparison.Ordinal);
    }

    private void Log(string message)
    {
        if (verboseLog)
        {
            Debug.Log($"[MainDarkBoardChaos] {message}");
        }
    }
}
