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

    [Header("Scene Filter")]
    [SerializeField] private bool randomRockOnlyInMainEarth = true;
    [SerializeField] private string mainEarthSceneName = "MainEarth";

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private int turnStartCounter;

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

        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged += HandleTurnChanged;
            if (verboseLog)
            {
                Debug.Log("[RockObstacleSpawnByTurn] Subscribed OnTurnChanged");
            }
        }
        else if (verboseLog)
        {
            Debug.LogWarning("[RockObstacleSpawnByTurn] ไม่พบ GameTurnManager ขณะ OnEnable");
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged -= HandleTurnChanged;
        }
    }

    private void HandleTurnChanged(bool isAITurn)
    {
        if (!enableRandomRockSpawnByTurn)
        {
            return;
        }

        if (randomRockSpawnIntervalTurns <= 0)
        {
            randomRockSpawnIntervalTurns = 1;
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

        bool didSpawn = routeManager.TrySpawnRandomRockObstacle();
        if (!didSpawn && verboseLog)
        {
            Debug.Log("[RockObstacleSpawnByTurn] ไม่มีช่องว่างสำหรับสุ่มวางหินเพิ่ม");
        }
    }
}