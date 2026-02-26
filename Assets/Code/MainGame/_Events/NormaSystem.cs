using UnityEngine;
using System.Collections;
using System;

public enum NormaType { Stars, Wins }

public class NormaSystem : MonoBehaviour
{
    public static NormaSystem Instance { get; private set; }

    [Header("Game Progression")]
    public int currentNormaRank = 1;
    public int maxNormaRank = 5;

    [Header("Current Goal")]
    public NormaType selectedNorma;
    public int targetAmount;

    public event Action<int, int, NormaType> OnNormaChanged;

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
        yield return new WaitUntil(() => GameTurnManager.Instance.currentState == GameState.WaitingForRoll);
        yield return new WaitForSeconds(0.5f);

        // ถ้าเริ่มเกมมา Rank 1 ให้เลือกเควส (แต่ถ้ากลับมาจากฉากอื่น Rank จะสูงกว่า 1 ก็จะไม่ทำซ้ำ)
        if (currentNormaRank == 1)
        {
            PromptNormaSelection(2);
        }
        
    }

    // ... (ฟังก์ชัน GetRequirement, CheckNormaCondition คงเดิม) ...
    public int GetRequirement(int rank, NormaType type)
    {
        switch (rank)
        {
            case 2: return (type == NormaType.Stars) ? 10 : 1;
            case 3: return (type == NormaType.Stars) ? 30 : 2;
            case 4: return (type == NormaType.Stars) ? 70 : 5;
            case 5: return (type == NormaType.Stars) ? 120 : 9;
            default: return 999;
        }
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

        if (NormaUIManager.Instance != null) NormaUIManager.Instance.UpdateInfoUI();

        // เช็คเงื่อนไข
        if (currentNormaRank < maxNormaRank)
        {
            // ยังไม่ตัน -> ให้เลือกเควสต่อไป
            PromptNormaSelection(currentNormaRank + 1);
        }
        else
        {
            // 👿 เลเวลตันแล้ว (Rank 5) -> เข้าสู่ FINAL PHASE!
            Debug.Log("⚠️ FINAL PHASE: Boss has appeared!");
            SpawnFinalBoss();
        }
    }

    private void PromptNormaSelection(int nextLevel)
    {
        if (GameTurnManager.CurrentPlayer == null || GameTurnManager.CurrentPlayer.isAI) return;
        if (NormaUIManager.Instance != null) NormaUIManager.Instance.ShowSelectionPanel(nextLevel);
    }

    public void SelectNorma(NormaType type)
    {
        int nextRank = currentNormaRank + 1;
        targetAmount = GetRequirement(nextRank, type);
        selectedNorma = type;

        OnNormaChanged?.Invoke(currentNormaRank, targetAmount, selectedNorma);

        // ของเดิม: จัดการตอนเริ่มเกม (Preparing)
        if (GameTurnManager.Instance != null && GameTurnManager.Instance.currentState == GameState.Preparing)
        {
            Debug.Log("[Norma] Selected! Moving to next state.");
        }
        // ✅ ของใหม่: ถ้าเลือกตอนเล่นอยู่ (EventProcessing) ให้จบเทิร์นด้วย
        else if (GameTurnManager.Instance != null && GameTurnManager.Instance.currentState == GameState.EventProcessing)
        {
            Debug.Log("[Norma] Selected! Ending turn.");
            GameTurnManager.Instance.RequestEndTurn();
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
        if (NormaUIManager.Instance != null)
        {
            NormaUIManager.Instance.UpdateInfoUI();
        }

        // ✅ ตรวจสอบสถานะว่าควรเปิดปุ่มทอยเต๋าหรือไม่
        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (GameTurnManager.Instance != null &&
            currentPlayer != null &&
            GameTurnManager.Instance.currentState == GameState.WaitingForRoll &&
            !currentPlayer.isAI)
        {
            DiceRollerFromPNG.Instance?.ForceEnableButton();
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