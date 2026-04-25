using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// กิมมิคด่าน MainDark: รวมเดบัฟจากหลายด่านเป็น pool เดียว แล้วสุ่มใส่ผู้เล่น
/// แนวทาง KISS + SRP:
/// - Controller นี้รับผิดชอบเฉพาะการสุ่มและ apply debuff
/// - ไม่ปะปน logic route/event อื่น
/// </summary>
public class MainDarkDebuffGimmickController : MonoBehaviour
{
    private const string MainDarkSceneName = "MainDark";

    private enum DebuffType
    {
        Ice = 0,
        Burn = 1,
        Curse = 2,
        Poison = 3,
        Sleep = 4
    }

    [System.Serializable]
    private struct DebuffOption
    {
        public DebuffType type;
        [Min(0)] public int weight;
        [Min(1)] public int turns;
    }

    [Header("MainDark Debuff Gimmick")]
    [SerializeField] private bool enableMainDarkDebuffGimmick = true;
    [SerializeField] private bool mainDarkDebuffOnlyInMainDark = true;
    [SerializeField] private bool enableAutoTriggerByTurn = false;
    [Min(1)] [SerializeField] private int autoTriggerIntervalTurns = 4;
    [SerializeField] private bool autoTriggerOnlyPlayerTurn = false;
    [SerializeField] private bool verboseLog = false;

    [Header("Debuff Pool (Weighted Random)")]
    [SerializeField] private List<DebuffOption> debuffPool = new List<DebuffOption>
    {
        new DebuffOption { type = DebuffType.Ice, weight = 20, turns = 1 },
        new DebuffOption { type = DebuffType.Burn, weight = 20, turns = 3 },
        new DebuffOption { type = DebuffType.Curse, weight = 20, turns = 3 },
        new DebuffOption { type = DebuffType.Poison, weight = 20, turns = 3 },
        new DebuffOption { type = DebuffType.Sleep, weight = 20, turns = 3 }
    };

    private int autoTriggerTurnsLeft;

    private void Awake()
    {
        ResetAutoTriggerCounter();
    }

    public void TickTurn(bool isAITurn)
    {
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

        GameObject target = GameTurnManager.CurrentPlayer != null ? GameTurnManager.CurrentPlayer.gameObject : null;
        bool triggered = TriggerGimmick(target);
        ResetAutoTriggerCounter();

        if (verboseLog)
        {
            Debug.Log($"[MainDarkDebuffGimmick] Auto trigger result = {triggered}");
        }
    }

    [ContextMenu("Trigger MainDark Debuff Gimmick (Current Player)")]
    public bool TriggerGimmickOnCurrentPlayer()
    {
        GameObject target = GameTurnManager.CurrentPlayer != null ? GameTurnManager.CurrentPlayer.gameObject : null;
        return TriggerGimmick(target);
    }

    public bool TriggerGimmick(GameObject target)
    {
        if (!enableMainDarkDebuffGimmick)
        {
            return false;
        }

        if (target == null)
        {
            return false;
        }

        if (!CanTriggerInCurrentScene())
        {
            return false;
        }

        PlayerState playerState = target.GetComponent<PlayerState>();
        if (playerState == null)
        {
            return false;
        }

        DebuffOption selectedOption;
        if (!TryPickWeightedDebuff(out selectedOption))
        {
            return false;
        }

        ApplyDebuff(playerState, selectedOption);
        if (verboseLog)
        {
            Debug.Log($"[MainDarkDebuffGimmick] Applied {selectedOption.type} ({selectedOption.turns} turn(s)) to {target.name}");
        }

        return true;
    }

    private bool CanTriggerInCurrentScene()
    {
        if (!mainDarkDebuffOnlyInMainDark)
        {
            return true;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return string.Equals(currentSceneName, MainDarkSceneName, System.StringComparison.Ordinal);
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

    private bool TryPickWeightedDebuff(out DebuffOption selectedOption)
    {
        selectedOption = default;

        if (debuffPool == null || debuffPool.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;
        for (int i = 0; i < debuffPool.Count; i++)
        {
            DebuffOption option = debuffPool[i];
            if (option.weight > 0)
            {
                totalWeight += option.weight;
            }
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < debuffPool.Count; i++)
        {
            DebuffOption option = debuffPool[i];
            if (option.weight <= 0)
            {
                continue;
            }

            cumulative += option.weight;
            if (roll < cumulative)
            {
                selectedOption = option;
                if (selectedOption.turns <= 0)
                {
                    selectedOption.turns = 1;
                }
                return true;
            }
        }

        return false;
    }

    private static void ApplyDebuff(PlayerState playerState, DebuffOption option)
    {
        int turns = option.turns <= 0 ? 1 : option.turns;
        switch (option.type)
        {
            case DebuffType.Ice:
                playerState.ApplyIceDebuff();
                break;
            case DebuffType.Burn:
                playerState.ApplyBurnDebuff(turns);
                break;
            case DebuffType.Curse:
                playerState.ApplyBackwardCurse(turns);
                break;
            case DebuffType.Poison:
                playerState.ApplyPoisonDebuff(turns);
                break;
            case DebuffType.Sleep:
                playerState.ApplySleepDebuff(turns);
                break;
            default:
                playerState.ApplyPoisonDebuff(turns);
                break;
        }
    }
}
