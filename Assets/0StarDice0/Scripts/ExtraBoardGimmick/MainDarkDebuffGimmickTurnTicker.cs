using UnityEngine;

/// <summary>
/// แยกการนับเทิร์นของ MainDark debuff gimmick ออกจาก GameEventManager
/// เพื่อให้โครงสร้างเรียบง่ายและแยกหน้าที่ชัดเจน (KISS + SRP)
/// </summary>
public class MainDarkDebuffGimmickTurnTicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainDarkDebuffGimmickController darkDebuffGimmickController;

    [Header("Settings")]
    [SerializeField] private bool enableTurnTick = true;
    [SerializeField] private bool triggerOnceIfNoTurnManager = false;
    [SerializeField] private bool simulateTurnsIfNoTurnManager = true;
    [Min(0.1f)] [SerializeField] private float simulatedTurnIntervalSeconds = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private Coroutine fallbackTickCoroutine;

    private void Awake()
    {
        if (darkDebuffGimmickController == null)
        {
            darkDebuffGimmickController = FindFirstObjectByType<MainDarkDebuffGimmickController>();
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
            StopFallbackTickLoop();
            if (verboseLog)
            {
                Debug.Log("[MainDarkDebuffGimmickTurnTicker] Subscribed OnTurnChanged");
            }
        }
        else
        {
            if (verboseLog)
            {
                Debug.LogWarning("[MainDarkDebuffGimmickTurnTicker] ไม่พบ GameTurnManager ขณะ OnEnable");
            }

            if (triggerOnceIfNoTurnManager)
            {
                if (darkDebuffGimmickController == null)
                {
                    darkDebuffGimmickController = FindFirstObjectByType<MainDarkDebuffGimmickController>();
                }

                if (darkDebuffGimmickController != null)
                {
                    bool triggered = darkDebuffGimmickController.TriggerGimmickOnCurrentPlayer();
                    if (verboseLog)
                    {
                        Debug.Log($"[MainDarkDebuffGimmickTurnTicker] Fallback trigger = {triggered}");
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

        if (darkDebuffGimmickController == null)
        {
            darkDebuffGimmickController = FindFirstObjectByType<MainDarkDebuffGimmickController>();
        }

        if (darkDebuffGimmickController == null)
        {
            if (verboseLog)
            {
                Debug.LogWarning("[MainDarkDebuffGimmickTurnTicker] ไม่พบ MainDarkDebuffGimmickController");
            }
            return;
        }

        darkDebuffGimmickController.TickTurn(isAITurn);
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
            Debug.Log("[MainDarkDebuffGimmickTurnTicker] Started fallback tick loop");
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
