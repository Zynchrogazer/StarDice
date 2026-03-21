using UnityEngine;

/// <summary>
/// แยกการนับเทิร์นของ MainLight heal gimmick ออกจาก RouteManager (แนว KISS)
/// - Subscribe กับ GameTurnManager.OnTurnChanged
/// - ทุกครั้งที่เปลี่ยนเทิร์น จะเรียก RouteManager.TickMainLightHealGimmickTurn()
/// </summary>
public class MainLightHealGimmickTurnTicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RouteManager routeManager;

    [Header("Settings")]
    [SerializeField] private bool enableTurnTick = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

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
                Debug.Log("[MainLightHealGimmickTurnTicker] Subscribed OnTurnChanged");
            }
        }
        else if (verboseLog)
        {
            Debug.LogWarning("[MainLightHealGimmickTurnTicker] ไม่พบ GameTurnManager ขณะ OnEnable");
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
        if (!enableTurnTick)
        {
            return;
        }

        if (routeManager == null && !RouteManager.TryGet(out routeManager))
        {
            if (verboseLog)
            {
                Debug.LogWarning("[MainLightHealGimmickTurnTicker] ไม่พบ RouteManager");
            }

            return;
        }

        routeManager.TickMainLightHealGimmickTurn();
    }
}