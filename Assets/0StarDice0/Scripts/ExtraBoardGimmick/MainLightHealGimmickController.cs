using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// รับผิดชอบกิมมิคช่อง Heal ชั่วคราวของด่าน MainLight
/// แยกออกจาก RouteManager เพื่อให้แยกหน้าที่ชัดขึ้น (KISS + SRP)
/// </summary>
public class MainLightHealGimmickController : MonoBehaviour
{
    private const string MainLightSceneName = "MainLight";

    [System.Serializable]
    private class TemporaryTileChange
    {
        public int tileID;
        public TileType originalType;
        public string originalEventName;
    }

    [Header("References")]
    [SerializeField] private RouteManager routeManager;

    [Header("MainLight Heal Tile Gimmick")]
    [Tooltip("เปิดเพื่อใช้งานกิมมิคช่อง Heal ของ MainLight")]
    [SerializeField] private bool enableMainLightHealGimmick = true;
    [Tooltip("ถ้าเปิด จะให้กิมมิคทำงานเฉพาะฉาก MainLight")]
    [SerializeField] private bool mainLightHealOnlyInMainLight = true;
    [Min(1)]
    [Tooltip("ระยะเวลา (เทิร์น) ที่ช่อง Heal ชั่วคราวจะคงอยู่ก่อนคืนค่ากลับ")]
    [SerializeField] private int mainLightHealDurationTurns = 3;
    [Min(1)]
    [Tooltip("จำนวนช่อง Heal ต่ำสุดต่อการ Trigger")]
    [SerializeField] private int mainLightHealMinTiles = 5;
    [Min(1)]
    [Tooltip("จำนวนช่อง Heal สูงสุดต่อการ Trigger")]
    [SerializeField] private int mainLightHealMaxTiles = 10;
    [Tooltip("เปิดเพื่อให้ระบบสุ่ม Trigger เองทุก ๆ N เทิร์น")]
    [SerializeField] private bool enableAutoTriggerByTurn = true;
    [Min(1)]
    [Tooltip("จำนวนเทิร์นต่อการสุ่ม Trigger 1 ครั้ง (ปรับได้)")]
    [SerializeField] private int autoTriggerIntervalTurns = 4;
    [Tooltip("ถ้าเปิดจะนับเฉพาะตอนจบเทิร์นผู้เล่น (ไม่นับ AI)")]
    [SerializeField] private bool autoTriggerOnlyPlayerTurn = false;

    [Header("Heal Event Preview UI")]
    [SerializeField] private GameObject healPreviewRoot;
    [SerializeField] private Image healPreviewImage;
    [SerializeField] private Sprite healEventPreviewSprite;
    [SerializeField] private float healPreviewDurationSeconds = 2f;
    [SerializeField] private bool waitForTurnAnnouncementBeforePreview = true;
    [SerializeField] private bool waitForLevelRewardPanelsBeforePreview = true;

    [Header("Heal Event Preview Visual")]
    [SerializeField] private bool autoConfigurePreviewImage = true;
    [SerializeField] private Vector2 healPreviewSize = new Vector2(640f, 640f);
    [SerializeField] private Vector2 healPreviewAnchoredPosition = Vector2.zero;
    [Range(0f, 1f)] [SerializeField] private float healPreviewAlpha = 0.35f;
    [SerializeField] private bool healPreviewPreserveAspect = true;
    [SerializeField] private bool healPreviewRaycastTarget = false;
    [SerializeField] private bool healPreviewBringToFront = true;

    private int mainLightHealTurnsLeft;
    private int autoTriggerTurnsLeft;
    private Coroutine activePreviewRoutine;
    private bool isHumanHealPreviewBlockingRoll;
    private bool isWaitingForTurnAnnouncementToFinish;
    private readonly List<TemporaryTileChange> activeMainLightHealChanges = new List<TemporaryTileChange>();

    private void Awake()
    {
        if (routeManager == null)
        {
            RouteManager.TryGet(out routeManager);
        }

        ResetAutoTriggerCounter();
        HideHealPreview();
    }

    public void TickTurn(bool isAITurn)
    {
        bool restoredThisTurn = TickActiveGimmickDuration();
        if (restoredThisTurn)
        {
            // KISS: ถ้าเพิ่งคืนค่าช่องเดิมในเทิร์นนี้ ให้จบรอบก่อน
            // เพื่อไม่ให้ restore แล้ว trigger ซ้ำทันทีจนดูเหมือนไม่เคยคืนค่า
            ResetAutoTriggerCounter();
            return;
        }

        if (!ShouldTickAutoTrigger(isAITurn))
        {
            return;
        }

        EnsureAutoTriggerSettings();
        autoTriggerTurnsLeft--;
        if (autoTriggerTurnsLeft > 0)
        {
            return;
        }

        bool isTriggered = TriggerGimmick();
        ResetAutoTriggerCounter();
        if (!isTriggered)
        {
            Debug.LogWarning("💚 MainLight Heal Gimmick: ถึงรอบสุ่มอัตโนมัติแล้ว แต่ Trigger ไม่ผ่านเงื่อนไข");
        }
    }

    private bool TickActiveGimmickDuration()
    {
        if (mainLightHealTurnsLeft <= 0)
        {
            return false;
        }

        mainLightHealTurnsLeft--;
        if (mainLightHealTurnsLeft > 0)
        {
            return false;
        }

        RestoreMainLightHealTiles();
        Debug.Log("💚 MainLight Heal Gimmick หมดเวลาแล้ว คืนค่าช่องเดิมเรียบร้อย");
        return true;
    }

    private bool ShouldTickAutoTrigger(bool isAITurn)
    {
        if (!enableAutoTriggerByTurn)
        {
            return false;
        }

        if (autoTriggerOnlyPlayerTurn && isAITurn)
        {
            return false;
        }

        return true;
    }

    private void EnsureAutoTriggerSettings()
    {
        if (autoTriggerIntervalTurns <= 0)
        {
            autoTriggerIntervalTurns = 1;
        }

        if (autoTriggerTurnsLeft <= 0)
        {
            autoTriggerTurnsLeft = autoTriggerIntervalTurns;
        }
    }

    private void ResetAutoTriggerCounter()
    {
        autoTriggerTurnsLeft = autoTriggerIntervalTurns > 0 ? autoTriggerIntervalTurns : 1;
    }

    [ContextMenu("Trigger MainLight Heal Gimmick")]
    public bool TriggerGimmick()
    {
        if (!enableMainLightHealGimmick)
        {
            return false;
        }

        if (routeManager == null && !RouteManager.TryGet(out routeManager))
        {
            return false;
        }

        if (!CanTriggerInCurrentScene())
        {
            return false;
        }

        EnsureValidSettings();

        if (activeMainLightHealChanges.Count > 0)
        {
            RestoreMainLightHealTiles();
        }

        int randomTileCount = Random.Range(mainLightHealMinTiles, mainLightHealMaxTiles + 1);
        int changedCount = ApplyTemporaryHealTiles(randomTileCount);
        if (changedCount <= 0)
        {
            return false;
        }

        mainLightHealTurnsLeft = mainLightHealDurationTurns;
        QueueHumanHealPreview();
        Debug.Log($"💚 Trigger MainLight Heal Gimmick: เปลี่ยน {changedCount} ช่อง เป็นเวลา {mainLightHealDurationTurns} เทิร์น");
        return true;
    }

    public bool IsBlockingRollFor(PlayerState playerState)
    {
        return isHumanHealPreviewBlockingRoll && playerState != null && !playerState.isAI;
    }

    public static void ReleasePendingHumanPreviewAfterTurnAnnouncement(PlayerState playerState)
    {
        if (playerState == null || playerState.isAI)
        {
            return;
        }

        MainLightHealGimmickController[] controllers = FindObjectsByType<MainLightHealGimmickController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            MainLightHealGimmickController controller = controllers[i];
            if (controller != null && controller.IsBlockingRollFor(playerState))
            {
                controller.isWaitingForTurnAnnouncementToFinish = false;
            }
        }
    }

    public static IEnumerator WaitForPendingHumanPreview(PlayerState playerState)
    {
        if (playerState == null || playerState.isAI)
        {
            yield break;
        }

        MainLightHealGimmickController[] controllers = FindObjectsByType<MainLightHealGimmickController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            MainLightHealGimmickController controller = controllers[i];
            if (controller == null || !controller.IsBlockingRollFor(playerState))
            {
                continue;
            }

            while (controller != null && controller.IsBlockingRollFor(playerState))
            {
                yield return null;
            }
        }
    }

    private void QueueHumanHealPreview()
    {
        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer == null || currentPlayer.isAI)
        {
            return;
        }

        if (activePreviewRoutine != null)
        {
            StopCoroutine(activePreviewRoutine);
            activePreviewRoutine = null;
        }

        isHumanHealPreviewBlockingRoll = true;
        isWaitingForTurnAnnouncementToFinish = ShouldWaitForTurnAnnouncementToFinish();
        activePreviewRoutine = StartCoroutine(ShowHumanHealPreviewRoutine());
    }

    private IEnumerator ShowHumanHealPreviewRoutine()
    {
        while (isWaitingForTurnAnnouncementToFinish)
        {
            HideHealPreview();
            yield return null;
        }

        if (waitForLevelRewardPanelsBeforePreview)
        {
            while (LevelRewardUI.IsAnyRewardPanelVisible())
            {
                HideHealPreview();
                yield return null;
            }
        }

        ShowHealPreview();
        yield return new WaitForSeconds(Mathf.Max(0.1f, healPreviewDurationSeconds));
        HideHealPreview();

        isHumanHealPreviewBlockingRoll = false;
        activePreviewRoutine = null;
    }

    private bool ShouldWaitForTurnAnnouncementToFinish()
    {
        return waitForTurnAnnouncementBeforePreview &&
            GameTurnManager.TryGet(out var gameTurnManager) &&
            gameTurnManager.currentState == GameState.TurnAnnouncement;
    }

    private void ShowHealPreview()
    {
        ApplyHealPreviewVisualSettings();

        if (healPreviewImage != null && healEventPreviewSprite != null)
        {
            healPreviewImage.sprite = healEventPreviewSprite;
        }

        if (healPreviewRoot != null)
        {
            healPreviewRoot.SetActive(true);
        }
        else if (healPreviewImage != null)
        {
            healPreviewImage.gameObject.SetActive(true);
        }
    }

    private void HideHealPreview()
    {
        if (healPreviewRoot != null)
        {
            healPreviewRoot.SetActive(false);
        }
        else if (healPreviewImage != null)
        {
            healPreviewImage.gameObject.SetActive(false);
        }
    }

    private void ApplyHealPreviewVisualSettings()
    {
        if (healPreviewImage == null)
        {
            return;
        }

        healPreviewImage.color = new Color(
            healPreviewImage.color.r,
            healPreviewImage.color.g,
            healPreviewImage.color.b,
            Mathf.Clamp01(healPreviewAlpha));
        healPreviewImage.preserveAspect = healPreviewPreserveAspect;
        healPreviewImage.raycastTarget = healPreviewRaycastTarget;

        if (autoConfigurePreviewImage)
        {
            RectTransform imageRect = healPreviewImage.rectTransform;
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = healPreviewAnchoredPosition;
            imageRect.sizeDelta = healPreviewSize;
        }

        if (healPreviewBringToFront)
        {
            Transform frontTarget = healPreviewRoot != null ? healPreviewRoot.transform : healPreviewImage.transform;
            frontTarget.SetAsLastSibling();
        }
    }

    private void OnDisable()
    {
        if (activePreviewRoutine != null)
        {
            StopCoroutine(activePreviewRoutine);
            activePreviewRoutine = null;
        }

        isHumanHealPreviewBlockingRoll = false;
        isWaitingForTurnAnnouncementToFinish = false;
        HideHealPreview();
    }

    private bool CanTriggerInCurrentScene()
    {
        if (!mainLightHealOnlyInMainLight)
        {
            return true;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return string.Equals(currentSceneName, MainLightSceneName, System.StringComparison.Ordinal);
    }

    private void EnsureValidSettings()
    {
        if (mainLightHealDurationTurns <= 0)
        {
            mainLightHealDurationTurns = 1;
        }

        if (mainLightHealMinTiles <= 0)
        {
            mainLightHealMinTiles = 1;
        }

        if (mainLightHealMaxTiles < mainLightHealMinTiles)
        {
            mainLightHealMaxTiles = mainLightHealMinTiles;
        }
    }

    private int ApplyTemporaryHealTiles(int desiredCount)
    {
        List<NodeConnection> nodes = routeManager.nodeConnections;
        if (nodes == null || nodes.Count == 0)
        {
            return 0;
        }

        List<NodeConnection> candidates = new List<NodeConnection>();
        for (int i = 0; i < nodes.Count; i++)
        {
            NodeConnection nodeData = nodes[i];
            if (!IsValidCandidate(nodeData))
            {
                continue;
            }

            candidates.Add(nodeData);
        }

        if (candidates.Count == 0)
        {
            return 0;
        }

        activeMainLightHealChanges.Clear();
        int targetCount = Mathf.Min(desiredCount, candidates.Count);

        for (int i = 0; i < targetCount; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);
            NodeConnection temp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;

            NodeConnection chosenNode = candidates[i];
            activeMainLightHealChanges.Add(new TemporaryTileChange
            {
                tileID = chosenNode.tileID,
                originalType = chosenNode.type,
                originalEventName = chosenNode.eventName
            });

            chosenNode.type = TileType.Heal;
            chosenNode.eventName = routeManager.GetDefaultEventName(TileType.Heal);
            routeManager.ApplyTileVisual(chosenNode);
        }

        routeManager.RebuildNodeDataMap();
        return activeMainLightHealChanges.Count;
    }

    private static bool IsValidCandidate(NodeConnection nodeData)
    {
        if (nodeData == null || nodeData.node == null)
        {
            return false;
        }

        if (nodeData.lockRandomType)
        {
            return false;
        }

        return nodeData.type != TileType.Heal &&
               nodeData.type != TileType.Start &&
               nodeData.type != TileType.Shop &&
               nodeData.type != TileType.Teleport &&
               nodeData.type != TileType.Boss &&
               nodeData.type != TileType.SpecialBoss;
    }

    private void RestoreMainLightHealTiles()
    {
        if (activeMainLightHealChanges.Count == 0)
        {
            mainLightHealTurnsLeft = 0;
            return;
        }

        for (int i = 0; i < activeMainLightHealChanges.Count; i++)
        {
            TemporaryTileChange change = activeMainLightHealChanges[i];
            NodeConnection nodeData = routeManager.GetNodeData(change.tileID);
            if (nodeData == null)
            {
                continue;
            }

            nodeData.type = change.originalType;
            nodeData.eventName = change.originalEventName;
            routeManager.ApplyTileVisual(nodeData);
        }

        activeMainLightHealChanges.Clear();
        mainLightHealTurnsLeft = 0;
        routeManager.RebuildNodeDataMap();
    }
}
