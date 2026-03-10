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
    public static GameTurnManager Instance { get; private set; }
    public const string PendingBattleReturnKey = "PendingBattleReturn";

    [Header("State Machine")]
    public GameState currentState = GameState.Idle;

    [Header("Players")]
    public List<PlayerState> allPlayers = new List<PlayerState>();
    public int currentPlayerIndex = 0;

    
    public event System.Action<bool> OnTurnChanged;
    // ===== Current Player =====
    public static PlayerState CurrentPlayer =>
        (Instance != null && Instance.allPlayers.Count > 0)
            ? Instance.allPlayers[Instance.currentPlayerIndex]
            : null;

    // ===== UNITY =====
    private void Awake()
    {
        // เช็ค Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ✅ บรรทัดนี้สำคัญที่สุด! ทำให้ Manager ไม่ตายเมื่อเปลี่ยนฉาก
        DontDestroyOnLoad(gameObject);
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
        if (CurrentPlayer != null)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log($"<color=cyan>[Turn] รอ UI ประกาศเทิร์น...</color>");
            OnTurnChanged?.Invoke(CurrentPlayer.isAI);

            if (!CurrentPlayer.isAI && CurrentPlayer.TryConsumeBurnDebuff(10))
            {
                Debug.Log($"<color=orange>🔥 Burn ticks on {CurrentPlayer.name} (-10 HP)</color>");
                yield return new WaitForSeconds(0.5f);
            }

            if (CurrentPlayer.PlayerHealth <= 0)
            {
                yield break;
            }
        }
        yield return new WaitForSeconds(1.0f);

        SetState(GameState.WaitingForRoll);
        Debug.Log($"<color=yellow>⭐ Turn Start: {CurrentPlayer.name} (AI: {CurrentPlayer.isAI})</color>");

        if (CurrentPlayer.isAI)
        {
            yield return new WaitForSeconds(0.8f);
            SetState(GameState.Rolling);

            if (DiceRollerFromPNG.Instance != null)
                DiceRollerFromPNG.Instance.RollDiceForAI();
        }
        else
        {
            if (DiceRollerFromPNG.Instance != null)
                DiceRollerFromPNG.Instance.ForceEnableButton();
        }
    }

    // ===== DICE RESULT =====
    public void OnDiceRolled(int steps)
    {
        if (currentState != GameState.WaitingForRoll &&
            currentState != GameState.Rolling)
            return;

        SetState(GameState.Moving);

        Debug.Log($"🎲 {CurrentPlayer.name} rolled {steps}");

        PlayerPathWalker walker = CurrentPlayer.GetComponent<PlayerPathWalker>();
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
        Debug.Log($"❌ End Turn: {CurrentPlayer.name}");

        currentPlayerIndex++;
        if (currentPlayerIndex >= allPlayers.Count)
            currentPlayerIndex = 0;

        StartCoroutine(StartTurnRoutine());
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

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.ResetEventStatus();
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

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.ResetEventStatus();
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
        if (GameEventManager.Instance != null) GameEventManager.Instance.ResetEventStatus();

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

        // 2. 🗺️ หาแผนที่ของฉากปัจจุบันเตรียมไว้
        RouteManager currentMap = FindObjectOfType<RouteManager>();
        Scene activeScene = SceneManager.GetActiveScene();

        if (currentMap == null) Debug.LogError("😱 [Manager] ไม่เจอ RouteManager ในฉากนี้!");

        bool hasPersistentPlayers = false;
        for (int i = 0; i < discoveredPlayers.Length; i++)
        {
            PlayerState candidate = discoveredPlayers[i];
            if (candidate != null && candidate.gameObject.scene.buildIndex == -1)
            {
                hasPersistentPlayers = true;
                break;
            }
        }

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

            bool isPersistentPlayer = obj.scene.buildIndex == -1;
            bool isInActiveScene = obj.scene == activeScene;

            // โหมดปกติ: ถ้ามีผู้เล่นข้ามฉาก ให้เลือกเฉพาะชุดนั้นก่อน
            // โหมด fallback: ถ้าไม่พบผู้เล่นข้ามฉากเลย ให้ใช้ผู้เล่นจาก active scene เท่านั้น
            if (hasPersistentPlayers)
            {
                if (!isPersistentPlayer)
                {
                    continue;
                }
            }
            else if (!isInActiveScene)
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

        string sourceMode = hasPersistentPlayers ? "persistent players" : "active-scene players (fallback)";
        Debug.Log($"<color=green>[Manager] ♻️ Refresh Players & Map: {allPlayers.Count} players from {sourceMode}</color>");
    }
}
