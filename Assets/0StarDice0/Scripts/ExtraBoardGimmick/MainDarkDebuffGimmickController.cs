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
        [Min(1)] public int turns;
    }

    [Header("MainDark Debuff Gimmick")]
    [SerializeField] private bool enableMainDarkDebuffGimmick = true;
    [SerializeField] private bool mainDarkDebuffOnlyInMainDark = true;
    [SerializeField] private bool enableAutoTriggerByTurn = false;
    [Min(1)] [SerializeField] private int autoTriggerIntervalTurns = 4;
    [SerializeField] private bool autoTriggerOnlyPlayerTurn = true;
    [SerializeField] private bool preventDuplicateActiveDebuffs = true;

    [Header("Debuff Pool (Random 1 Option Per Trigger)")]
    [SerializeField] private List<DebuffOption> debuffPool = new List<DebuffOption>
    {
        new DebuffOption { type = DebuffType.Ice, turns = 1 },
        new DebuffOption { type = DebuffType.Burn, turns = 2 },
        new DebuffOption { type = DebuffType.Curse, turns = 2 },
        new DebuffOption { type = DebuffType.Poison, turns = 2 },
        new DebuffOption { type = DebuffType.Sleep, turns = 1 }
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

        GameObject target = ResolveHumanPlayerTarget();
        bool triggered = TriggerGimmick(target);
        ResetAutoTriggerCounter();
        if (!triggered)
            Debug.LogWarning("[MainDarkDebuffGimmick] Auto trigger ไม่สำเร็จ");
    }

    [ContextMenu("Trigger MainDark Debuff Gimmick (Current Player)")]
    public bool TriggerGimmickOnCurrentPlayer()
    {
        return TriggerGimmick(ResolveHumanPlayerTarget());
    }


    private static GameObject ResolveHumanPlayerTarget()
    {
        if (GameTurnManager.CurrentPlayer != null && !GameTurnManager.CurrentPlayer.isAI)
            return GameTurnManager.CurrentPlayer.gameObject;

        if (GameTurnManager.TryGet(out var gameTurnManager) && gameTurnManager.allPlayers != null)
        {
            foreach (PlayerState player in gameTurnManager.allPlayers)
            {
                if (player != null && !player.isAI)
                    return player.gameObject;
            }
        }

        return null;
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
        if (playerState == null || playerState.isAI)
        {
            return false;
        }

        if (debuffPool == null || debuffPool.Count == 0)
        {
            return false;
        }

        List<DebuffOption> eligibleDebuffs = BuildEligibleDebuffPool(playerState);
        if (eligibleDebuffs.Count == 0)
        {
            Debug.Log("[MainDarkDebuffGimmick] Skip trigger because every debuff in pool is already active.");
            return false;
        }

        int randomIndex = Random.Range(0, eligibleDebuffs.Count);
        DebuffOption selectedOption = eligibleDebuffs[randomIndex];
        if (selectedOption.turns <= 0)
        {
            selectedOption.turns = 1;
        }

        ApplyDebuff(playerState, selectedOption);
        Debug.Log($"[MainDarkDebuffGimmick] Applied {selectedOption.type} ({selectedOption.turns} turn(s)) to {target.name}");

        return true;
    }

    private List<DebuffOption> BuildEligibleDebuffPool(PlayerState playerState)
    {
        List<DebuffOption> eligibleDebuffs = new List<DebuffOption>();
        for (int i = 0; i < debuffPool.Count; i++)
        {
            DebuffOption option = debuffPool[i];
            if (!preventDuplicateActiveDebuffs || !IsDebuffActive(playerState, option.type))
            {
                eligibleDebuffs.Add(option);
            }
        }

        return eligibleDebuffs;
    }

    private static bool IsDebuffActive(PlayerState playerState, DebuffType type)
    {
        if (playerState == null)
        {
            return false;
        }

        switch (type)
        {
            case DebuffType.Ice:
                return playerState.hasIceEffect || playerState.StunTurnsRemaining > 0;
            case DebuffType.Burn:
                return playerState.DebuffBurn && playerState.DebuffBurnTurnsRemaining > 0;
            case DebuffType.Curse:
                return playerState.backwardCurseTurns > 0;
            case DebuffType.Poison:
                return playerState.poisonDebuffTurns > 0;
            case DebuffType.Sleep:
                return playerState.sleepDebuffTurns > 0;
            default:
                return false;
        }
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
