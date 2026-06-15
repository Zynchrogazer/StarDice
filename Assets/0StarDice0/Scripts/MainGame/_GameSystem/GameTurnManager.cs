using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// FSM หลักของ board turn flow: จำกัดเกมให้อยู่ในสถานะที่คาดเดาได้
public enum GameState
{
    Idle,
    Preparing,
    TurnAnnouncement,
    TurnGimmickProcessing,
    WaitingForRoll,
    Rolling,
    Moving,
    EventProcessing,
    Ending
}

public class GameTurnManager : MonoBehaviour
{
    private static GameTurnManager cachedManager;

    public const string PendingBattleReturnKey = "PendingBattleReturn";

    [Header("State Machine")]
    public GameState currentState = GameState.Idle;

    [Header("Players")]
    public List<PlayerState> allPlayers = new List<PlayerState>();
    public int currentPlayerIndex = 0;

    [Header("References")]
    [SerializeField] private DiceRollerFromPNG diceRoller;
    [SerializeField] private GameEventManager gameEventManager;

    [Header("Turn Flow Timing")]
    [Min(0f)] [SerializeField] private float turnAnnouncementWaitSeconds = 1.5f;

    public event System.Action<bool> OnTurnChanged;
    public event System.Action<GameState> OnStateChanged;

    #region Static Access

    public static PlayerState CurrentPlayer
    {
        get
        {
            if (!TryGet(out var manager) || manager.allPlayers.Count == 0)
            {
                return null;
            }

            return manager.IsValidCurrentPlayerIndex() ? manager.allPlayers[manager.currentPlayerIndex] : null;
        }
    }

    public static bool TryGet(out GameTurnManager manager)
    {
        if (cachedManager != null)
        {
            manager = cachedManager;
            return true;
        }

        manager = FindFirstObjectByType<GameTurnManager>();
        if (manager != null)
        {
            cachedManager = manager;
        }

        return manager != null;
    }

    public static bool TryGetCurrentPlayer(out PlayerState player)
    {
        player = CurrentPlayer;
        return player != null;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        GameTurnManager[] managers = FindObjectsByType<GameTurnManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        cachedManager = this;
    }

    private void OnEnable()
    {
        GameEventManager.OnBoardSceneReady += HandleReturnFromBattle;
    }

    private void Start()
    {
        RefreshPlayers();
        currentPlayerIndex = 0;
        StartCoroutine(StartTurnRoutine());
    }

    private void OnDisable()
    {
        GameEventManager.OnBoardSceneReady -= HandleReturnFromBattle;
    }

    private void OnDestroy()
    {
        if (cachedManager == this)
        {
            cachedManager = null;
        }
    }

    #endregion

    #region FSM Public API

    public void SetState(GameState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"<color=magenta>[State] → {newState}</color>");
    }

    public void OnDiceRolled(int steps)
    {
        if (currentState != GameState.WaitingForRoll && currentState != GameState.Rolling)
        {
            return;
        }

        SetState(GameState.Moving);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
        {
            return;
        }

        Debug.Log($"🎲 {currentPlayer.name} rolled {steps}");
        PlayerPathWalker walker = currentPlayer.GetComponent<PlayerPathWalker>();
        if (walker != null)
        {
            walker.ExecuteMove(steps);
            return;
        }

        RequestEndTurn();
    }

    public void RequestEndTurn()
    {
        if (currentState == GameState.Ending)
        {
            return;
        }

        if (allPlayers == null || allPlayers.Count == 0)
        {
            Debug.LogWarning("[GameTurnManager] RequestEndTurn ignored: no players in turn list.");
            SetState(GameState.Idle);
            return;
        }

        SetState(GameState.Ending);
        TickCurrentPlayerEndTurnDebuffs();
        AdvanceToNextPlayer();
        StartCoroutine(StartTurnRoutine());
    }

    public void HandleReturnFromBattle()
    {
        if (PlayerPrefs.GetInt(PendingBattleReturnKey, 0) != 1)
        {
            return;
        }

        PlayerPrefs.SetInt(PendingBattleReturnKey, 0);
        PlayerPrefs.Save();

        Debug.Log("<color=magenta>[Manager] 📻 โดนปลุกโดยตรง! กำลังกู้คืนระบบ...</color>");
        RefreshPlayers();
        if (allPlayers.Count == 0)
        {
            return;
        }

        PlayerState endedPlayer = CurrentPlayer;
        endedPlayer?.TickEndTurnDebuffs();
        GameTurnBattleReturnService.RelocateBattleMonster(endedPlayer, allPlayers);

        AdvanceToNextPlayer();
        SetState(GameState.Idle);
        StopAllCoroutines();
        ResolveGameEventManager()?.ResetEventStatus();

        Debug.Log($"[Manager] ✅ กลับจาก Battle แล้ว ส่งต่อเทิร์นให้: {CurrentPlayer?.name}");
        StartCoroutine(StartTurnRoutine());
        StartCoroutine(RecoverRollButtonAfterBoardReturn());
    }

    #endregion

    #region Turn FSM Flow

    private IEnumerator StartTurnRoutine()
    {
        yield return null;

        if (!EnsurePlayersReady())
        {
            SetState(GameState.Idle);
            yield break;
        }

        SetState(GameState.Preparing);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
        {
            yield break;
        }

        yield return AnnounceTurn(currentPlayer);
        yield return ProcessStartTurnGimmicks(currentPlayer);

        if (TrySkipTurnByDebuff(currentPlayer, out float skipDelay))
        {
            yield return new WaitForSeconds(skipDelay);
            RequestEndTurn();
            yield break;
        }

        yield return ProcessBurnAndHealthGate(currentPlayer);
        if (!currentPlayer.isAI && currentPlayer.PlayerHealth <= 0)
        {
            yield break;
        }

        yield return WaitForRollInput(currentPlayer);
    }

    private IEnumerator AnnounceTurn(PlayerState currentPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        SetState(GameState.TurnAnnouncement);
        Debug.Log("<color=cyan>[Turn] รอ UI ประกาศเทิร์น...</color>");
        OnTurnChanged?.Invoke(currentPlayer.isAI);
        yield return new WaitForSeconds(ResolveTurnAnnouncementWaitSeconds());
    }

    private IEnumerator ProcessStartTurnGimmicks(PlayerState currentPlayer)
    {
        SetState(GameState.TurnGimmickProcessing);
        MainDarkDebuffGimmickController.ReleasePendingHumanPreviewAfterTurnAnnouncement(currentPlayer);
        MainLightHealGimmickController.ReleasePendingHumanPreviewAfterTurnAnnouncement(currentPlayer);
        yield return MainDarkDebuffGimmickController.WaitForPendingHumanPreview(currentPlayer);
        yield return MainLightHealGimmickController.WaitForPendingHumanPreview(currentPlayer);
    }

    private IEnumerator ProcessBurnAndHealthGate(PlayerState currentPlayer)
    {
        if (!currentPlayer.isAI && currentPlayer.TryConsumeBurnDebuff(10))
        {
            Debug.Log($"<color=orange>🔥 Burn ticks on {currentPlayer.name} (-10 HP)</color>");
            yield return new WaitForSeconds(0.5f);
        }

        if (currentPlayer.isAI)
        {
            currentPlayer.EnsureBoardAIAlive();
        }
    }

    private IEnumerator WaitForRollInput(PlayerState currentPlayer)
    {
        yield return new WaitForSeconds(1.0f);
        SetState(GameState.WaitingForRoll);
        currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
        {
            yield break;
        }

        Debug.Log($"<color=yellow>⭐ Turn Start: {currentPlayer.name} (AI: {currentPlayer.isAI})</color>");
        if (currentPlayer.isAI)
        {
            yield return RunAITurnRoll();
            yield break;
        }

        EnableHumanRollButton();
    }

    private IEnumerator RunAITurnRoll()
    {
        yield return new WaitForSeconds(0.8f);
        SetState(GameState.Rolling);
        DiceRollerFromPNG roller = ResolveDiceRoller();
        if (roller != null)
        {
            roller.RollDiceForAI();
        }
        else
        {
            Debug.LogError("[GameTurnManager] DiceRollerFromPNG not found for AI turn.");
        }
    }

    private void EnableHumanRollButton()
    {
        DiceRollerFromPNG roller = ResolveDiceRoller();
        if (roller != null)
        {
            roller.ForceEnableButton();
        }
        else
        {
            Debug.LogError("[GameTurnManager] DiceRollerFromPNG not found for player turn.");
        }
    }

    #endregion

    #region Turn Rules

    private bool TrySkipTurnByDebuff(PlayerState currentPlayer, out float skipDelay)
    {
        skipDelay = 1.5f;
        if (currentPlayer.sleepDebuffTurns > 0)
        {
            Debug.Log($"<color=blue>💤 ข้ามเทิร์น! {currentPlayer.name} กำลังหลับอยู่ (เหลืออีก {currentPlayer.sleepDebuffTurns - 1} เทิร์น)</color>");
            currentPlayer.sleepDebuffTurns--;
            currentPlayer.NotifyStatsUpdated();
            return true;
        }

        if (currentPlayer.StunTurnsRemaining <= 0)
        {
            return false;
        }

        Debug.Log($"<color=blue>❄️ {currentPlayer.name} ถูกแช่แข็ง! ข้ามเทิร์นนี้ (เหลืออีก {currentPlayer.StunTurnsRemaining - 1} เทิร์น)</color>");
        currentPlayer.StunTurnsRemaining--;
        if (currentPlayer.StunTurnsRemaining <= 0)
        {
            currentPlayer.hasIceEffect = false;
        }

        currentPlayer.NotifyStatsUpdated();
        return true;
    }

    private void TickCurrentPlayerEndTurnDebuffs()
    {
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
        {
            return;
        }

        currentPlayer.TickEndTurnDebuffs();
        Debug.Log($"❌ End Turn: {currentPlayer.name}");
    }

    private void AdvanceToNextPlayer()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= allPlayers.Count)
        {
            currentPlayerIndex = 0;
        }
    }

    #endregion

    #region Reset / Scene Flow

    public void ResetForSceneExit()
    {
        StopAllCoroutines();
        ResetPlayersForBoardSession();
        SetState(GameState.Idle);
        ResolveGameEventManager()?.ResetEventStatus();
    }

    public void ResetForNewBoardSession()
    {
        StopAllCoroutines();
        ResetPlayersForBoardSession();
        PlayerPrefs.SetInt(PendingBattleReturnKey, 0);

        PlayerStartSpawner spawner = FindFirstObjectByType<PlayerStartSpawner>(FindObjectsInactive.Include);
        bool canRespawnPlayers = CanRespawnPlayers(spawner);
        if (canRespawnPlayers)
        {
            spawner.SpawnAllPlayers();
        }
        else
        {
            Debug.Log("[Manager] Skip SpawnAllPlayers: board scene/spawner is not ready yet.");
        }

        ResolveGameEventManager()?.ResetEventStatus();
        if (canRespawnPlayers)
        {
            StartCoroutine(StartTurnRoutine());
        }
    }

    private void ResetPlayersForBoardSession()
    {
        RefreshPlayers();
        foreach (var player in allPlayers)
        {
            player?.ResetForNewBoardSession();
        }

        currentPlayerIndex = 0;
        PlayerStartSpawner.LastKnownPositions.Clear();
    }

    private bool CanRespawnPlayers(PlayerStartSpawner spawner)
    {
        return spawner != null
               && spawner.routeManager != null
               && spawner.routeManager.nodeConnections != null
               && spawner.routeManager.nodeConnections.Count > 0;
    }

    private IEnumerator RecoverRollButtonAfterBoardReturn()
    {
        yield return new WaitForSeconds(1.75f);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentState == GameState.WaitingForRoll && currentPlayer != null && !currentPlayer.isAI)
        {
            ResolveDiceRoller()?.ForceEnableButton();
        }
    }

    #endregion

    #region Helpers / Resolvers

    private bool EnsurePlayersReady()
    {
        if (allPlayers == null || allPlayers.Count == 0)
        {
            RefreshPlayers();
        }

        if (allPlayers == null || allPlayers.Count == 0)
        {
            Debug.LogError("[GameTurnManager] Cannot start turn: no players found in board scene.");
            return false;
        }

        if (!IsValidCurrentPlayerIndex())
        {
            Debug.LogWarning($"[GameTurnManager] currentPlayerIndex out of range ({currentPlayerIndex}). Reset to 0.");
            currentPlayerIndex = 0;
        }

        return true;
    }

    private bool IsValidCurrentPlayerIndex()
    {
        return currentPlayerIndex >= 0 && currentPlayerIndex < allPlayers.Count;
    }

    private float ResolveTurnAnnouncementWaitSeconds()
    {
        float waitSeconds = Mathf.Max(0f, turnAnnouncementWaitSeconds);
        TurnAnnouncementUI[] announcementUIs = FindObjectsByType<TurnAnnouncementUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < announcementUIs.Length; i++)
        {
            TurnAnnouncementUI announcementUI = announcementUIs[i];
            if (announcementUI != null)
            {
                waitSeconds = Mathf.Max(waitSeconds, announcementUI.showDuration);
            }
        }

        return waitSeconds;
    }

    private DiceRollerFromPNG ResolveDiceRoller()
    {
        if (diceRoller == null)
        {
            diceRoller = FindFirstObjectByType<DiceRollerFromPNG>();
        }

        return diceRoller;
    }

    private GameEventManager ResolveGameEventManager()
    {
        if (gameEventManager == null)
        {
            gameEventManager = FindFirstObjectByType<GameEventManager>();
        }

        return gameEventManager;
    }

    private void RefreshPlayers()
    {
        GameTurnPlayerRegistry.RefreshBoardPlayers(allPlayers);
    }

    #endregion
}
