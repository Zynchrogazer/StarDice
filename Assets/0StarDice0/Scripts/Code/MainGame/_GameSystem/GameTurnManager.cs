using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// ===== ENUM =====
public enum GameState
{
    Idle,
    Preparing,
    WaitingForRoll,
    Rolling,
    Moving,
    EventProcessing,
    Ending
}

public class GameTurnManager : MonoBehaviour
{
    private static GameTurnManager cachedManager;

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

    public const string PendingBattleReturnKey = "PendingBattleReturn";

    [Header("State Machine")]
    public GameState currentState = GameState.Idle;

    [Header("Players")]
    public List<PlayerState> allPlayers = new List<PlayerState>();
    public int currentPlayerIndex = 0;

    [Header("References (Refactor Prep)")]
    [SerializeField] private DiceRollerFromPNG diceRoller;
    [SerializeField] private GameEventManager gameEventManager;

    
    public event System.Action<bool> OnTurnChanged;
    // ===== Current Player =====
    public static PlayerState CurrentPlayer
    {
        get
        {
            if (!TryGet(out var manager) || manager.allPlayers.Count == 0)
                return null;

            if (manager.currentPlayerIndex < 0 || manager.currentPlayerIndex >= manager.allPlayers.Count)
                return null;

            return manager.allPlayers[manager.currentPlayerIndex];
        }
    }

    public static bool TryGetCurrentPlayer(out PlayerState player)
    {
        player = CurrentPlayer;
        return player != null;
    }

    // ===== UNITY =====
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

    private void OnDestroy()
    {
        if (cachedManager == this)
        {
            cachedManager = null;
        }
    }

    private void Start()
    {
        RefreshPlayers(); // ✅ จัดแถวทันทีที่เริ่ม
        currentPlayerIndex = 0; // ✅ มั่นใจว่าเริ่มที่คนแรก (Human)

        StartCoroutine(StartTurnRoutine());
    }

    private void OnEnable()
    {
        // ⭐ ฟังสัญญาณ "กลับจาก Battle"
        GameEventManager.OnBoardSceneReady += HandleReturnFromBattle;
    }

    private void OnDisable()
    {
        GameEventManager.OnBoardSceneReady -= HandleReturnFromBattle;
    }

    // ===== STATE =====
    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"<color=magenta>[State] → {newState}</color>");
    }

    // ===== TURN FLOW =====
    private IEnumerator StartTurnRoutine()
    {
        yield return null;

        SetState(GameState.Preparing);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer != null)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log($"<color=cyan>[Turn] รอ UI ประกาศเทิร์น...</color>");
            OnTurnChanged?.Invoke(currentPlayer.isAI);

            if (!currentPlayer.isAI && currentPlayer.TryConsumeBurnDebuff(10))
            {
                Debug.Log($"<color=orange>🔥 Burn ticks on {currentPlayer.name} (-10 HP)</color>");
                yield return new WaitForSeconds(0.5f);
            }

            if (currentPlayer.PlayerHealth <= 0)
            {
                yield break;
            }
        }
        yield return new WaitForSeconds(1.0f);

        SetState(GameState.WaitingForRoll);
        currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
            yield break;

        Debug.Log($"<color=yellow>⭐ Turn Start: {currentPlayer.name} (AI: {currentPlayer.isAI})</color>");

        if (currentPlayer.isAI)
        {
            yield return new WaitForSeconds(0.8f);
            SetState(GameState.Rolling);

            if (ResolveDiceRoller() != null)
                ResolveDiceRoller().RollDiceForAI();
        }
        else
        {
            if (ResolveDiceRoller() != null)
                ResolveDiceRoller().ForceEnableButton();
        }
    }

    // ===== DICE RESULT =====
    public void OnDiceRolled(int steps)
    {
        if (currentState != GameState.WaitingForRoll &&
            currentState != GameState.Rolling)
            return;

        SetState(GameState.Moving);

        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer == null)
            return;

        Debug.Log($"🎲 {currentPlayer.name} rolled {steps}");

        PlayerPathWalker walker = currentPlayer.GetComponent<PlayerPathWalker>();
        if (walker != null)
        {
            walker.ExecuteMove(steps);
        }
        else
        {
            RequestEndTurn();
        }
    }

    // ===== END TURN =====
    public void RequestEndTurn()
    {
        if (currentState == GameState.Ending)
            return;

        SetState(GameState.Ending);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer != null)
            Debug.Log($"❌ End Turn: {currentPlayer.name}");

        currentPlayerIndex++;
        if (currentPlayerIndex >= allPlayers.Count)
            currentPlayerIndex = 0;

        StartCoroutine(StartTurnRoutine());
    }



    private DiceRollerFromPNG ResolveDiceRoller()
    {
        if (diceRoller == null)
            diceRoller = FindFirstObjectByType<DiceRollerFromPNG>();

        return diceRoller;
    }

    private GameEventManager ResolveGameEventManager()
    {
        if (gameEventManager == null)
            gameEventManager = FindFirstObjectByType<GameEventManager>();

        return gameEventManager;
    }

    public void ResetForSceneExit()
    {
        StopAllCoroutines();
        RefreshPlayers();

        foreach (var player in allPlayers)
        {
            player?.ResetForNewBoardSession();
        }

        currentPlayerIndex = 0;
        SetState(GameState.Idle);

        PlayerStartSpawner.LastKnownPositions.Clear();

        if (ResolveGameEventManager() != null)
        {
            ResolveGameEventManager().ResetEventStatus();
        }
    }

    public void ResetForNewBoardSession()
    {
        StopAllCoroutines();
        RefreshPlayers();

        foreach (var player in allPlayers)
        {
            player?.ResetForNewBoardSession();
        }

        currentPlayerIndex = 0;
        SetState(GameState.Idle);

        PlayerStartSpawner.LastKnownPositions.Clear();
        PlayerPrefs.SetInt(PendingBattleReturnKey, 0);

        PlayerStartSpawner spawner = FindObjectOfType<PlayerStartSpawner>(true);
        bool canRespawnPlayers = spawner != null
                                 && spawner.routeManager != null
                                 && spawner.routeManager.nodeConnections != null
                                 && spawner.routeManager.nodeConnections.Count > 0;

        if (canRespawnPlayers)
        {
            spawner.SpawnAllPlayers();
        }
        else
        {
            Debug.Log("[Manager] Skip SpawnAllPlayers: board scene/spawner is not ready yet.");
        }

        if (ResolveGameEventManager() != null)
        {
            ResolveGameEventManager().ResetEventStatus();
        }

        if (canRespawnPlayers)
        {
            StartCoroutine(StartTurnRoutine());
        }
    }

    // ===== ⭐ 핵심: RETURN FROM BATTLE =====
    // เปลี่ยนจาก private void HandleReturnFromBattle() เป็น public
    public void HandleReturnFromBattle()
    {
        // ทำงานเฉพาะกรณีกลับจากฉาก Battle จริง ๆ เท่านั้น
        if (PlayerPrefs.GetInt(PendingBattleReturnKey, 0) != 1)
        {
            return;
        }

        PlayerPrefs.SetInt(PendingBattleReturnKey, 0);
        PlayerPrefs.Save();

        Debug.Log("<color=magenta>[Manager] 📻 โดนปลุกโดยตรง! กำลังกู้คืนระบบ...</color>");

        RefreshPlayers();

        // 🛡️ Safety Check: ถ้าหาคนไม่เจอ ห้ามรันต่อเดี๋ยวค้าง
        if (allPlayers.Count == 0)
        {
            Debug.LogError("❌ ไม่สามารถเริ่มเทิร์นได้ เพราะไม่มีผู้เล่นใน List");
            return;
        }

        currentPlayerIndex = 0;

        SetState(GameState.Idle);
        StopAllCoroutines();
        if (ResolveGameEventManager() != null) ResolveGameEventManager().ResetEventStatus();

        Debug.Log("[Manager] ✅ กลับจาก Battle แล้ว เริ่มเทิร์นที่ผู้เล่นคนแรก");
        StartCoroutine(StartTurnRoutine());
    }

    // (และอย่าลืมฟังก์ชันจัดแถวที่ผมให้ไปคราวก่อน ถ้ายังไม่มีให้เติมลงไปครับ)
    // ใน GameTurnManager.cs

    // แก้ไขใน GameTurnManager.cs

    // ในไฟล์ GameTurnManager.cs

    private void RefreshPlayers()
    {
        allPlayers.Clear();

        PlayerState[] discoveredPlayers = FindObjectsByType<PlayerState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<PlayerState> validPlayers = new List<PlayerState>();

        // KISS: ใช้ฉากของ RouteManager เป็น board scene หลัก แล้วดึงผู้เล่นเฉพาะฉากนี้
        RouteManager currentMap = FindObjectOfType<RouteManager>();
        Scene boardScene = currentMap != null ? currentMap.gameObject.scene : SceneManager.GetActiveScene();

        if (currentMap == null) Debug.LogError("😱 [Manager] ไม่เจอ RouteManager ในฉากนี้!");

        for (int i = 0; i < discoveredPlayers.Length; i++)
        {
            PlayerState p = discoveredPlayers[i];
            if (p == null)
            {
                continue;
            }

            GameObject obj = p.gameObject;
            if (obj == null)
            {
                continue;
            }

            if (obj.scene != boardScene)
            {
                continue;
            }

            validPlayers.Add(p);

            // ✅ หัวใจสำคัญ: ยัดแผนที่ใหม่ใส่มือเดี๋ยวนี้!
            PlayerPathWalker walker = p.GetComponent<PlayerPathWalker>();
            if (walker != null && currentMap != null)
            {
                walker.ReconnectReferences(currentMap); // สั่งเชื่อมต่อใหม่ทันที
            }
        }

        // 3. เรียงลำดับ (คนมาก่อน Bot)
        validPlayers.Sort((a, b) =>
        {
            int typeComparison = a.isAI.CompareTo(b.isAI);
            if (typeComparison != 0) return typeComparison;
            return string.Compare(a.name, b.name);
        });

        allPlayers.AddRange(validPlayers);
        Debug.Log($"<color=green>[Manager] ♻️ Refresh Players & Map: {allPlayers.Count} players from board scene '{boardScene.name}'</color>");
    }
}
