using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// แยกระบบสุ่มหินรายเทิร์นออกจาก RouteManager (แนว KISS)
/// - Subscribe กับ GameTurnManager.OnTurnChanged
/// - ครบทุก N เทิร์นจะเรียก RouteManager.TrySpawnRandomRockObstacle()
/// </summary>
public class RockObstacleSpawnByTurn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RouteManager routeManager;

    [Header("Spawn Settings")]
    [SerializeField] private bool enableRandomRockSpawnByTurn = true;
    [SerializeField, Min(1)] private int randomRockSpawnIntervalTurns = 5;
    [SerializeField, Min(1)] private int randomRockSpawnCountPerInterval = 1;
    [SerializeField, Min(0)] private int maxActiveRandomRockObstacles = 6;
    [SerializeField] private bool spawnOnlyOnPlayerTurn = true;

    [Header("Scene Filter")]
    [SerializeField] private bool randomRockOnlyInMainEarth = true;
    [SerializeField] private string mainEarthSceneName = "MainEarth";

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private int turnStartCounter;
    private bool isSubscribedToTurnManager;

    private void Awake()
    {
        if (routeManager == null)
        {
            RouteManager.TryGet(out routeManager);
        }
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
            if (verboseLog)
            {
                Debug.Log("[RockObstacleSpawnByTurn] Subscribed OnTurnChanged");
            }
        }
        else if (verboseLog)
        {
            Debug.LogWarning("[RockObstacleSpawnByTurn] ไม่พบ GameTurnManager ขณะ Subscribe");
        }
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

    private void HandleTurnChanged(bool isAITurn)
    {
        if (!enableRandomRockSpawnByTurn)
        {
            return;
        }

        if (spawnOnlyOnPlayerTurn && isAITurn)
        {
            return;
        }

        if (randomRockSpawnIntervalTurns <= 0)
        {
            randomRockSpawnIntervalTurns = 1;
        }
        if (randomRockSpawnCountPerInterval <= 0)
        {
            randomRockSpawnCountPerInterval = 1;
        }

        if (randomRockOnlyInMainEarth)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!string.Equals(currentSceneName, mainEarthSceneName))
            {
                return;
            }
        }

        if (routeManager == null && !RouteManager.TryGet(out routeManager))
        {
            if (verboseLog)
            {
                Debug.LogWarning("[RockObstacleSpawnByTurn] ไม่พบ RouteManager");
            }

            return;
        }

        turnStartCounter++;
        if (turnStartCounter % randomRockSpawnIntervalTurns != 0)
        {
            return;
        }

        int spawnCount = randomRockSpawnCountPerInterval;
        if (maxActiveRandomRockObstacles > 0)
        {
            int activeRockCount = routeManager.CountActiveRockObstacles();
            int remainingCapacity = maxActiveRandomRockObstacles - activeRockCount;
            if (remainingCapacity <= 0)
            {
                if (verboseLog)
                {
                    Debug.Log($"[RockObstacleSpawnByTurn] ถึงเพดานหินแล้ว ({activeRockCount}/{maxActiveRandomRockObstacles})");
                }
                return;
            }

            spawnCount = Mathf.Min(spawnCount, remainingCapacity);
        }

        int spawnedCount = routeManager.TrySpawnRandomRockObstacles(spawnCount);
        if (spawnedCount <= 0 && verboseLog)
        {
            Debug.Log("[RockObstacleSpawnByTurn] ไม่มีช่องว่างสำหรับสุ่มวางหินเพิ่ม");
        }
        else if (verboseLog)
        {
            Debug.Log($"[RockObstacleSpawnByTurn] สุ่มวางหินสำเร็จ {spawnedCount}/{randomRockSpawnCountPerInterval} ก้อน");
        }
    }
}
