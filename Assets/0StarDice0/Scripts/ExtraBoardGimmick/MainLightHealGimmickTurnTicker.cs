using UnityEngine;

/// <summary>
/// แยกการนับเทิร์นของ MainLight heal gimmick ออกจาก RouteManager (แนว KISS)
/// - Subscribe กับ GameTurnManager.OnTurnChanged
/// - ทุกครั้งที่เปลี่ยนเทิร์น จะเรียก MainLightHealGimmickController.TickTurn(isAITurn)
/// </summary>
public class MainLightHealGimmickTurnTicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainLightHealGimmickController healGimmickController;

    [Header("Settings")]
    [SerializeField] private bool enableTurnTick = true;
    [SerializeField] private bool triggerOnceIfNoTurnManager = true;
    [SerializeField] private bool simulateTurnsIfNoTurnManager = true;
    [Min(0.1f)]
    [SerializeField] private float simulatedTurnIntervalSeconds = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private Coroutine fallbackTickCoroutine;

    private void Awake()
    {
        if (healGimmickController == null)
            healGimmickController = FindFirstObjectByType<MainLightHealGimmickController>();
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
            StopFallbackTickLoop();
            if (verboseLog)
            {
                Debug.Log("[MainLightHealGimmickTurnTicker] Subscribed OnTurnChanged");
            }
        }
        else
        {
            if (verboseLog)
            {
                Debug.LogWarning("[MainLightHealGimmickTurnTicker] ไม่พบ GameTurnManager ขณะ OnEnable");
            }

            if (triggerOnceIfNoTurnManager)
            {
                if (healGimmickController == null)
                    healGimmickController = FindFirstObjectByType<MainLightHealGimmickController>();

                if (healGimmickController != null)
                {
                    bool triggered = healGimmickController.TriggerGimmick();
                    if (verboseLog)
                    {
                        Debug.Log($"[MainLightHealGimmickTurnTicker] Fallback TriggerGimmick() = {triggered}");
                    }
                }
            }

            if (enableTurnTick && simulateTurnsIfNoTurnManager)
            {
                StartFallbackTickLoop();
            }
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

        StopFallbackTickLoop();
    }

    private void HandleTurnChanged(bool isAITurn)
    {
        if (!enableTurnTick)
        {
            return;
        }

        if (healGimmickController == null)
        {
            healGimmickController = FindFirstObjectByType<MainLightHealGimmickController>();
        }

        if (healGimmickController == null)
        {
            if (verboseLog)
            {
                Debug.LogWarning("[MainLightHealGimmickTurnTicker] ไม่พบ MainLightHealGimmickController");
            }

            return;
        }

        healGimmickController.TickTurn(isAITurn);
    }

    private void StartFallbackTickLoop()
    {
        if (fallbackTickCoroutine != null)
        {
            return;
        }

        fallbackTickCoroutine = StartCoroutine(FallbackTickLoop());
        if (verboseLog)
        {
            Debug.Log("[MainLightHealGimmickTurnTicker] Started fallback tick loop");
        }
    }

    private void StopFallbackTickLoop()
    {
        if (fallbackTickCoroutine == null)
        {
            return;
        }

        StopCoroutine(fallbackTickCoroutine);
        fallbackTickCoroutine = null;
    }

    private System.Collections.IEnumerator FallbackTickLoop()
    {
        while (enabled && gameObject.activeInHierarchy)
        {
            float waitSeconds = simulatedTurnIntervalSeconds > 0f ? simulatedTurnIntervalSeconds : 1f;
            yield return new WaitForSeconds(waitSeconds);
            HandleTurnChanged(false);
        }

        fallbackTickCoroutine = null;
    }
}
