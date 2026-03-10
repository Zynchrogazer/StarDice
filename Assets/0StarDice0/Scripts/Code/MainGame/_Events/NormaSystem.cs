using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public enum NormaType { Stars, Wins }

public class NormaSystem : MonoBehaviour
{
    public static bool TryGet(out NormaSystem manager)
    {
        manager = Instance;
        return manager != null;
    }

    public static NormaSystem Instance { get; private set; }

    [Header("Game Progression")]
    public int currentNormaRank = 1;
    public int maxNormaRank = 6;

    [Header("Current Goal")]
    public NormaType selectedNorma;
    public int targetAmount;

    public event Action<int, int, NormaType> OnNormaChanged;
    [SerializeField] private NormaUIManager normaUIManager;
    [SerializeField] private GameTurnManager gameTurnManager;
    [SerializeField] private DiceRollerFromPNG diceRollerFromPng;

    private void Awake()
    {
        // 🛡️ ระบบป้องกันตัวซ้ำ + ทำให้เป็นอมตะ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // อยู่ยงคงกระพัน
    }

    private IEnumerator Start()
    {
        // ทำงานแค่ครั้งแรกของเกม ครั้งเดียวเท่านั้น
        yield return new WaitUntil(() => GameTurnManager.CurrentPlayer != null);
        yield return new WaitUntil(() => ResolveGameTurnManager() != null && ResolveGameTurnManager().currentState == GameState.WaitingForRoll);
        yield return new WaitForSeconds(0.5f);

        // ถ้าเริ่มเกมมา Rank 1 ให้เลือกเควส (แต่ถ้ากลับมาจากฉากอื่น Rank จะสูงกว่า 1 ก็จะไม่ทำซ้ำ)
        if (currentNormaRank == 1)
        {
            PromptNormaSelection(2);
        }
        
    }

    private enum NormaStagePreset
    {
        Default,
        MainFire,
        MainLight,
        MainWater,
        MainEarth,
        MainWind,
        MainDark
    }

    private const int NormaQuestCountPerStage = 5;
    private int[] cachedMainFireStarTargets;
    private int[] cachedMainLightStarTargets;
    private int[] cachedMainWaterStarTargets;
    private int[] cachedMainEarthStarTargets;
    private int[] cachedMainWindStarTargets;
    private int[] cachedMainDarkStarTargets;

    private readonly int[] mainFireWinTargets = { 2, 4, 7, 9, 12 };
    private readonly int[] mainLightWinTargets = { 2, 4, 6, 8, 10 };
    private readonly int[] mainWaterWinTargets = { 2, 4, 7, 9, 12 };
    private readonly int[] mainEarthWinTargets = { 2, 5, 8, 11, 14 };
    private readonly int[] mainWindWinTargets = { 2, 5, 8, 11, 14 };
    private readonly int[] mainDarkWinTargets = { 2, 5, 8, 11, 14 };

    public string GetRequirementText(int rank, NormaType type)
    {
        return GetRequirement(rank, type).ToString();
    }

    public int GetRequirement(int rank, NormaType type)
    {
        int questIndex = rank - 2; // rank 2..6 = quest 1..5
        if (questIndex >= 0 && questIndex < NormaQuestCountPerStage)
        {
            NormaStagePreset stagePreset = ResolveStagePreset();

            if (stagePreset == NormaStagePreset.MainFire)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainFire);

                if (type == NormaType.Stars)
                    return cachedMainFireStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainFireWinTargets[questIndex];
            }

            if (stagePreset == NormaStagePreset.MainLight)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainLight);

                if (type == NormaType.Stars)
                    return cachedMainLightStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainLightWinTargets[questIndex];
            }

            if (stagePreset == NormaStagePreset.MainWater)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainWater);

                if (type == NormaType.Stars)
                    return cachedMainWaterStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainWaterWinTargets[questIndex];
            }

            if (stagePreset == NormaStagePreset.MainEarth)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainEarth);

                if (type == NormaType.Stars)
                    return cachedMainEarthStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainEarthWinTargets[questIndex];
            }

            if (stagePreset == NormaStagePreset.MainWind)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainWind);

                if (type == NormaType.Stars)
                    return cachedMainWindStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainWindWinTargets[questIndex];
            }

            if (stagePreset == NormaStagePreset.MainDark)
            {
                EnsureStageStarTargetsGenerated(NormaStagePreset.MainDark);

                if (type == NormaType.Stars)
                    return cachedMainDarkStarTargets[questIndex];

                if (type == NormaType.Wins)
                    return mainDarkWinTargets[questIndex];
            }
        }

        switch (rank)
        {
            case 2: return (type == NormaType.Stars) ? 10 : 1;
            case 3: return (type == NormaType.Stars) ? 30 : 2;
            case 4: return (type == NormaType.Stars) ? 70 : 5;
            case 5: return (type == NormaType.Stars) ? 120 : 9;
            case 6: return (type == NormaType.Stars) ? 180 : 12;
            default: return 999;
        }
    }

    private NormaStagePreset ResolveStagePreset()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.Equals(sceneName, "TestMain", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sceneName, "MainFire", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainFire;

        if (string.Equals(sceneName, "MainLight", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainLight;

        if (string.Equals(sceneName, "MainWater", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainWater;

        if (string.Equals(sceneName, "MainEarth", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainEarth;

        if (string.Equals(sceneName, "MainWind", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainWind;

        if (string.Equals(sceneName, "MainDark", StringComparison.OrdinalIgnoreCase))
            return NormaStagePreset.MainDark;

        return NormaStagePreset.Default;
    }

    private void EnsureStageStarTargetsGenerated(NormaStagePreset stagePreset)
    {
        if (stagePreset == NormaStagePreset.MainFire)
        {
            if (cachedMainFireStarTargets != null && cachedMainFireStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(55, 71);
            cachedMainFireStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }

        if (stagePreset == NormaStagePreset.MainLight)
        {
            if (cachedMainLightStarTargets != null && cachedMainLightStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(45, 61);
            cachedMainLightStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }

        if (stagePreset == NormaStagePreset.MainWater)
        {
            if (cachedMainWaterStarTargets != null && cachedMainWaterStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(65, 81);
            cachedMainWaterStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }

        if (stagePreset == NormaStagePreset.MainEarth)
        {
            if (cachedMainEarthStarTargets != null && cachedMainEarthStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(75, 91);
            cachedMainEarthStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }

        if (stagePreset == NormaStagePreset.MainWind)
        {
            if (cachedMainWindStarTargets != null && cachedMainWindStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(90, 101);
            cachedMainWindStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }

        if (stagePreset == NormaStagePreset.MainDark)
        {
            if (cachedMainDarkStarTargets != null && cachedMainDarkStarTargets.Length == NormaQuestCountPerStage)
                return;

            int totalStars = UnityEngine.Random.Range(100, 121);
            cachedMainDarkStarTargets = GenerateCumulativeTargets(totalStars, NormaQuestCountPerStage);
            return;
        }
    }

    private GameTurnManager ResolveGameTurnManager()
    {
        if (gameTurnManager == null)
            gameTurnManager = FindFirstObjectByType<GameTurnManager>();

        return gameTurnManager;
    }

    private DiceRollerFromPNG ResolveDiceRoller()
    {
        if (diceRollerFromPng == null)
            diceRollerFromPng = FindFirstObjectByType<DiceRollerFromPNG>();

        return diceRollerFromPng;
    }

    private NormaUIManager ResolveNormaUIManager()
    {
        if (normaUIManager == null)
            normaUIManager = FindFirstObjectByType<NormaUIManager>();

        return normaUIManager;
    }

    private int[] GenerateCumulativeTargets(int total, int segments)
    {
        int[] increments = new int[segments];
        for (int i = 0; i < segments; i++)
            increments[i] = 1;

        int remaining = Mathf.Max(0, total - segments);
        for (int i = 0; i < remaining; i++)
        {
            int slot = UnityEngine.Random.Range(0, segments);
            increments[slot]++;
        }

        int[] cumulative = new int[segments];
        int running = 0;
        for (int i = 0; i < segments; i++)
        {
            running += increments[i];
            cumulative[i] = running;
        }

        return cumulative;
    }

    public bool CheckNormaCondition()
    {
        if (GameTurnManager.CurrentPlayer == null || GameTurnManager.CurrentPlayer.isAI)
            return false; // ถ้าเป็น AI หรือหาคนไม่เจอ ให้ตอบว่า "ไม่ได้อัปเวล"

        bool passed = false;
        if (selectedNorma == NormaType.Stars)
            passed = GameTurnManager.CurrentPlayer.PlayerStar >= targetAmount;
        else if (selectedNorma == NormaType.Wins)
            passed = GameTurnManager.CurrentPlayer.WinCount >= targetAmount;

        if (passed)
        {
            NormaLevelUp();
            return true; // ✅ แจ้งกลับว่า "อัปเวลแล้วนะ! (เปิด UI แล้ว)"
        }

        return false; // ❌ ยังไม่ผ่านเงื่อนไข
    }

    public void NormaLevelUp()
    {
        currentNormaRank++;
        Debug.Log($"🎉 NORMA RANK UP! Now Rank {currentNormaRank}");

        if (ResolveNormaUIManager() != null) ResolveNormaUIManager().UpdateInfoUI();

        // เช็คเงื่อนไข
        if (currentNormaRank < maxNormaRank)
        {
            // ยังไม่ตัน -> ให้เลือกเควสต่อไป
            PromptNormaSelection(currentNormaRank + 1);
        }
        else
        {
            // 👿 เลเวลตันแล้ว -> เข้าสู่ FINAL PHASE!
            Debug.Log("⚠️ FINAL PHASE: Boss has appeared!");
            SpawnFinalBoss();
        }
    }

    private void PromptNormaSelection(int nextLevel)
    {
        if (GameTurnManager.CurrentPlayer == null || GameTurnManager.CurrentPlayer.isAI) return;
        if (ResolveNormaUIManager() != null) ResolveNormaUIManager().ShowSelectionPanel(nextLevel);
    }

    public void SelectNorma(NormaType type)
    {
        int nextRank = currentNormaRank + 1;
        targetAmount = GetRequirement(nextRank, type);
        selectedNorma = type;

        OnNormaChanged?.Invoke(currentNormaRank, targetAmount, selectedNorma);

        // ของเดิม: จัดการตอนเริ่มเกม (Preparing)
        if (ResolveGameTurnManager() != null && ResolveGameTurnManager().currentState == GameState.Preparing)
        {
            Debug.Log("[Norma] Selected! Moving to next state.");
        }
        // ✅ ของใหม่: ถ้าเลือกตอนเล่นอยู่ (EventProcessing) ให้จบเทิร์นด้วย
        else if (ResolveGameTurnManager() != null && ResolveGameTurnManager().currentState == GameState.EventProcessing)
        {
            Debug.Log("[Norma] Selected! Ending turn.");
            ResolveGameTurnManager().RequestEndTurn();
        }
    }


    public void ResetForNewBoardSession()
    {
        currentNormaRank = 1;
        selectedNorma = NormaType.Stars;
        targetAmount = 999;
        cachedMainFireStarTargets = null;
        cachedMainLightStarTargets = null;
        cachedMainWaterStarTargets = null;
        cachedMainEarthStarTargets = null;
        cachedMainWindStarTargets = null;
        cachedMainDarkStarTargets = null;

        if (ResolveNormaUIManager() != null)
        {
            ResolveNormaUIManager().UpdateInfoUI();
        }

        StopAllCoroutines();
        StartCoroutine(PromptInitialSelectionWhenReady());
    }

    private IEnumerator PromptInitialSelectionWhenReady()
    {
        yield return new WaitUntil(() => GameTurnManager.CurrentPlayer != null);
        yield return new WaitUntil(() => ResolveGameTurnManager() != null && ResolveGameTurnManager().currentState == GameState.WaitingForRoll);
        yield return new WaitForSeconds(0.2f);

        if (currentNormaRank == 1)
        {
            PromptNormaSelection(2);
        }
    }

    private void OnEnable()
    {
        // ดักฟังข่าวเมื่อซีนพร้อม เพื่ออัปเดต UI 
        GameEventManager.OnBoardSceneReady += OnReturnToBoard;
    }

    private void OnDisable()
    {
        GameEventManager.OnBoardSceneReady -= OnReturnToBoard;
    }

    private void OnReturnToBoard()
    {
        // เมื่อกลับมาที่ซีนหลัก ให้เช็คว่าต้องเลือกเควสไหม หรือแค่อัปเดต HUD
        if (ResolveNormaUIManager() != null)
        {
            ResolveNormaUIManager().UpdateInfoUI();
        }

        // ✅ ตรวจสอบสถานะว่าควรเปิดปุ่มทอยเต๋าหรือไม่
        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (ResolveGameTurnManager() != null &&
            currentPlayer != null &&
            ResolveGameTurnManager().currentState == GameState.WaitingForRoll &&
            !currentPlayer.isAI)
        {
            ResolveDiceRoller()?.ForceEnableButton();
        }
    }

    private void SpawnFinalBoss()
    {
        // ค้นหา RouteManager ในฉาก (ผมสมมติว่าคุณมีสคริปต์นี้นะครับ)
        RouteManager route = FindObjectOfType<RouteManager>();
        if (route != null)
        {
            // สั่งให้ RouteManager เปลี่ยนช่องเป็นบอส
            route.SpawnBossTile();
        }
        else
        {
            Debug.LogError("ไม่เจอ RouteManager! ไม่สามารถเสกบอสได้");
        }
    }
}
