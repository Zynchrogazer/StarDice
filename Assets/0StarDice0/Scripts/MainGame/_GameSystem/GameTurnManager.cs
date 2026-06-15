using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// ===== ENUM =====
// FSM หลักของ board turn flow: จำกัดเกมให้อยู่ในสถานะที่คาดเดาได้
// เช่น รอทอยได้เฉพาะ WaitingForRoll/Rolling และเริ่มเดินเมื่อเปลี่ยนเป็น Moving
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

    [Header("Turn Flow Timing")]
    [Min(0f)] [SerializeField] private float turnAnnouncementWaitSeconds = 1.5f;

    
    public event System.Action<bool> OnTurnChanged;
    public event System.Action<GameState> OnStateChanged;
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
    // จุดเดียวสำหรับเปลี่ยน state เพื่อให้ debug/subscribe UI ได้ง่าย และลดการกระโดดสถานะผิดลำดับ
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

    // ===== TURN FLOW =====
    private IEnumerator StartTurnRoutine()
    {
        yield return null;

        if (allPlayers == null || allPlayers.Count == 0)
        {
            RefreshPlayers();
            if (allPlayers == null || allPlayers.Count == 0)
            {
                Debug.LogError("[GameTurnManager] Cannot start turn: no players found in board scene.");
                SetState(GameState.Idle);
                yield break;
            }
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= allPlayers.Count)
        {
            Debug.LogWarning($"[GameTurnManager] currentPlayerIndex out of range ({currentPlayerIndex}). Reset to 0.");
            currentPlayerIndex = 0;
        }

        SetState(GameState.Preparing);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer != null)
        {
            yield return new WaitForSeconds(0.5f);
            SetState(GameState.TurnAnnouncement);
            Debug.Log($"<color=cyan>[Turn] รอ UI ประกาศเทิร์น...</color>");
            OnTurnChanged?.Invoke(currentPlayer.isAI);
            yield return new WaitForSeconds(ResolveTurnAnnouncementWaitSeconds());

            SetState(GameState.TurnGimmickProcessing);
            MainDarkDebuffGimmickController.ReleasePendingHumanPreviewAfterTurnAnnouncement(currentPlayer);
            MainLightHealGimmickController.ReleasePendingHumanPreviewAfterTurnAnnouncement(currentPlayer);
            yield return MainDarkDebuffGimmickController.WaitForPendingHumanPreview(currentPlayer);
            yield return MainLightHealGimmickController.WaitForPendingHumanPreview(currentPlayer);

            if (currentPlayer.sleepDebuffTurns > 0)
            {
                Debug.Log($"<color=blue>💤 ข้ามเทิร์น! {currentPlayer.name} กำลังหลับอยู่ (เหลืออีก {currentPlayer.sleepDebuffTurns - 1} เทิร์น)</color>");
                
                // หักลบจำนวนเทิร์น และสั่งอัปเดต UI
                currentPlayer.sleepDebuffTurns--;
                currentPlayer.NotifyStatsUpdated(); 

                // รอ 1.5 วินาทีให้ผู้เล่นเห็นว่าข้ามเทิร์นเพราะหลับ แล้วส่งไม้ต่อเลย
                yield return new WaitForSeconds(1.5f); 
                RequestEndTurn(); 
                yield break; // หยุดการทำงานของคนนี้ทันที (ไม่ให้ไปทอยเต๋าต่อ)
            }

            if (currentPlayer.StunTurnsRemaining > 0)
            {
                Debug.Log($"<color=blue>❄️ {currentPlayer.name} ถูกแช่แข็ง! ข้ามเทิร์นนี้ (เหลืออีก {currentPlayer.StunTurnsRemaining - 1} เทิร์น)</color>");
                
                // ลดจำนวนเทิร์นลง 1
                currentPlayer.StunTurnsRemaining--;
                
                // ถ้าคุณมีไอคอนน้ำแข็ง ก็ให้ปลดออกตอนมันนับถึง 0
                if (currentPlayer.StunTurnsRemaining <= 0)
                {
                    currentPlayer.hasIceEffect = false;
                    currentPlayer.NotifyStatsUpdated(); // อัปเดต UI
                }

                // สั่งข้ามเทิร์นทันที!
                yield return new WaitForSeconds(1.5f); // รอให้ผู้เล่นเห็นสักแป๊บ
                RequestEndTurn(); // เตะส่งไปคิวถัดไปเลย
                yield break; // จบการทำงานฟังก์ชันนี้
            }

            if (!currentPlayer.isAI && currentPlayer.TryConsumeBurnDebuff(10))
            {
                Debug.Log($"<color=orange>🔥 Burn ticks on {currentPlayer.name} (-10 HP)</color>");
                yield return new WaitForSeconds(0.5f);
            }

            if (currentPlayer.isAI)
            {
                currentPlayer.EnsureBoardAIAlive();
            }
            else if (currentPlayer.PlayerHealth <= 0)
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
            else
                Debug.LogError("[GameTurnManager] DiceRollerFromPNG not found for AI turn.");
        }
        else
        {
            if (ResolveDiceRoller() != null)
                ResolveDiceRoller().ForceEnableButton();
            else
                Debug.LogError("[GameTurnManager] DiceRollerFromPNG not found for player turn.");
        }
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

        if (allPlayers == null || allPlayers.Count == 0)
        {
            Debug.LogWarning("[GameTurnManager] RequestEndTurn ignored: no players in turn list.");
            SetState(GameState.Idle);
            return;
        }

        SetState(GameState.Ending);
        PlayerState currentPlayer = CurrentPlayer;
        if (currentPlayer != null)
        {
            currentPlayer.TickEndTurnDebuffs();
            Debug.Log($"❌ End Turn: {currentPlayer.name}");
        }

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
        if (PlayerPrefs.GetInt(PendingBattleReturnKey, 0) != 1) return;
        PlayerPrefs.SetInt(PendingBattleReturnKey, 0);
        PlayerPrefs.Save();

        Debug.Log("<color=magenta>[Manager] 📻 โดนปลุกโดยตรง! กำลังกู้คืนระบบ...</color>");

        RefreshPlayers();

        if (allPlayers.Count == 0) return;

        PlayerState endedPlayer = CurrentPlayer;
        endedPlayer?.TickEndTurnDebuffs();

        // 🟢🟢 [เพิ่มโค้ดส่วนนี้] ระบบจับมอนสเตอร์โยนไปช่องอื่นหลังสู้เสร็จ 🟢🟢
        if (endedPlayer != null && RouteManager.TryGet(out var routeManager))
        {
            PlayerPathWalker currentWalker = endedPlayer.GetComponent<PlayerPathWalker>();
            if (currentWalker != null)
            {
                int currentTile = currentWalker.currentNodeID;
                PlayerState monsterToRelocate = null;

                if (endedPlayer.isAI) 
                {
                    // กรณีที่ 1: มอนสเตอร์เป็นคนเดินมาชนเรา (Attacker) ให้เด้งมอนสเตอร์(ตัวมันเอง)ออกไป
                    monsterToRelocate = endedPlayer;
                }
                else
                {
                    // กรณีที่ 2: เราเดินไปชนมอนสเตอร์ (Defender) ให้ค้นหาว่า AI ตัวไหนที่ยืนทับช่องเราอยู่
                    foreach (var p in allPlayers)
                    {
                        if (p != endedPlayer && p.isAI)
                        {
                            var w = p.GetComponent<PlayerPathWalker>();
                            if (w != null && w.currentNodeID == currentTile)
                            {
                                monsterToRelocate = p;
                                break;
                            }
                        }
                    }
                }

                // ถ้าเจอมอนสเตอร์ที่เพิ่งสู้กัน ให้สั่งย้ายมันไปช่องอื่น
                if (monsterToRelocate != null)
                {
                    RelocateMovingMonster(monsterToRelocate, routeManager);
                }
            }
        }
        // 🟢🟢 [จบส่วนที่เพิ่ม] 🟢🟢

        // สลับเทิร์น
        currentPlayerIndex++;
        if (currentPlayerIndex >= allPlayers.Count) currentPlayerIndex = 0;

        SetState(GameState.Idle);
        StopAllCoroutines();
        if (ResolveGameEventManager() != null) ResolveGameEventManager().ResetEventStatus();

        Debug.Log($"[Manager] ✅ กลับจาก Battle แล้ว ส่งต่อเทิร์นให้: {CurrentPlayer?.name}");
        StartCoroutine(StartTurnRoutine());
        StartCoroutine(RecoverRollButtonAfterBoardReturn());
    }

    // ==========================================
    // 👾 ฟังก์ชันสุ่มที่อยู่ใหม่ให้ AI มอนสเตอร์
    // ==========================================
    private void RelocateMovingMonster(PlayerState monsterAI, RouteManager routeManager)
    {
        // 1. เก็บ ID ของช่องที่ "มีคนยืนอยู่แล้ว" เพื่อป้องกันไม่ให้มอนสเตอร์วาร์ปไปตกทับคนอื่นอีกรอบ
        HashSet<int> occupiedIDs = new HashSet<int>();
        foreach (var p in allPlayers)
        {
            var w = p.GetComponent<PlayerPathWalker>();
            if (w != null) occupiedIDs.Add(w.currentNodeID);
        }

        // 2. รวบรวมช่องว่างทั้งหมดในกระดาน
        List<Transform> candidateNodes = new List<Transform>();
        foreach (var nc in routeManager.nodeConnections)
        {
            // กรองเงื่อนไข: ต้องไม่ใช่ช่อง Start/Shop และ ต้องไม่มีใครยืนอยู่
            if (nc != null && nc.node != null && 
                nc.type != TileType.Start && 
                nc.type != TileType.Shop && 
                !occupiedIDs.Contains(nc.tileID))
            {
                candidateNodes.Add(nc.node);
            }
        }

        // 3. สุ่มจับมอนสเตอร์โยนไปที่ช่องใหม่
        if (candidateNodes.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidateNodes.Count);
            Transform randomNode = candidateNodes[randomIndex];

            PlayerPathWalker aiWalker = monsterAI.GetComponent<PlayerPathWalker>();
            if (aiWalker != null)
            {
                // ใช้คำสั่ง TeleportToNode ที่คุณเขียนเตรียมไว้ใน PlayerPathWalker แล้วได้เลย!
                aiWalker.TeleportToNode(randomNode);
                Debug.Log($"<color=orange>💨 [Manager] จับมอนสเตอร์ AI ({monsterAI.name}) วาร์ปหนีไปซ่อนที่ {randomNode.name} แล้ว!</color>");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่มีช่องว่างเหลือให้มอนสเตอร์หนีเลย!");
        }
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
