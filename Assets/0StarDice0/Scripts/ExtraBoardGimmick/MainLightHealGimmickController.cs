using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private int mainLightHealTurnsLeft;
    private int autoTriggerTurnsLeft;
    private readonly List<TemporaryTileChange> activeMainLightHealChanges = new List<TemporaryTileChange>();

    private void Awake()
    {
        if (routeManager == null)
        {
            RouteManager.TryGet(out routeManager);
        }

        ResetAutoTriggerCounter();
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
        Debug.Log($"💚 Trigger MainLight Heal Gimmick: เปลี่ยน {changedCount} ช่อง เป็นเวลา {mainLightHealDurationTurns} เทิร์น");
        return true;
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
