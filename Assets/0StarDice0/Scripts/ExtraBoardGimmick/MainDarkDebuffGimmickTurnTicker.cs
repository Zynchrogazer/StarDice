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

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

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
            if (verboseLog)
            {
                Debug.Log("[MainDarkDebuffGimmickTurnTicker] Subscribed OnTurnChanged");
            }
        }
        else
        {
            Debug.LogWarning("[MainDarkDebuffGimmickTurnTicker] ไม่พบ GameTurnManager ขณะ OnEnable");
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
}
