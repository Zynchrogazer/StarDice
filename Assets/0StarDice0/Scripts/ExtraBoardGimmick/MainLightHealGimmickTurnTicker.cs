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

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

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
}
