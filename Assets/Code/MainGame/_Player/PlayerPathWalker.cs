using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // เพิ่มเข้ามาเพื่อจัดการ Event การโหลดซีน

/// <summary>
/// หัวใจหลักในการควบคุมการเดินของผู้เล่นบนบอร์ด
/// เป็น Singleton ที่ไม่ถูกทำลายเมื่อเปลี่ยนซีน และสามารถหา Dependencies เจอได้ด้วยตัวเอง
/// </summary>
[RequireComponent(typeof(PlayerMovement))] // แนะนำให้ใส่ไว้ เพื่อบังคับว่าต้องมี PlayerMovement เสมอ
public class PlayerPathWalker : MonoBehaviour
{
    public static PlayerPathWalker Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("ระยะเวลาที่จะหยุดนิ่งหลังเดินถึงแต่ละช่อง (วินาที)")]
    public float delayAfterNodeArrival = 0.5f;

    [Header("State")]
    [Tooltip("ID ของ Node ที่ผู้เล่นยืนอยู่ ณ ปัจจุบัน (จะถูกอัปเดตอัตโนมัติ)")]
    public int currentNodeID;

    // --- ตัวแปรภายใน ไม่ต้องตั้งค่าใน Inspector ---
    private RouteManager routeManager;
    private ChoiceUIManager choiceUIManager;
    private PlayerMovement playerMovement;
    private int stepsRemaining = 0;
    private bool isExecutingTurn = false;
    private Transform chosenNodeFromUI;

    // --- Properties สำหรับให้สคริปต์อื่นเรียกใช้ ---
    public bool IsExecutingTurn => isExecutingTurn;
    public Transform CurrentNodeTransform => routeManager?.GetNodeData(currentNodeID)?.node;

    #region Unity Lifecycle & Scene Management

    private void Awake()
    {
        // ตั้งค่า Singleton Pattern พร้อมทำให้เป็นอมตะ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ดึง Component ที่จำเป็นจาก GameObject เดียวกัน
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Start()
    {
        // ✅ เพิ่มส่วนนี้: ถ้าเริ่มเกมมาแล้วยังไม่มี UI ให้หาทันที
        if (choiceUIManager == null)
        {
            // ใส่ true เพื่อหาตัวที่ปิดอยู่ (Inactive) ด้วย
            choiceUIManager = FindObjectOfType<ChoiceUIManager>(true);

            if (choiceUIManager != null)
                Debug.Log($"[PlayerPathWalker] Auto-linked ChoiceUI in Start: {choiceUIManager.name}");
        }

        // กันเหนียว: หา RouteManager ด้วยถ้ายังไม่มี
        if (routeManager == null)
        {
            routeManager = RouteManager.Instance;
        }
    }
    private void OnEnable()
    {
        // "สมัคร" รอฟัง event เมื่อซีนถูกโหลดเสร็จ
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // "ยกเลิก" การรอฟัง event เพื่อป้องกัน memory leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// เมธอดนี้จะทำงาน "อัตโนมัติ" ทุกครั้งที่ซีนใหม่โหลดเสร็จ
    /// เพื่อค้นหา Dependencies ที่จำเป็นในซีนนั้นๆ
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // RouteManager เป็น Singleton อมตะเหมือนกัน จึงหาจาก Instance ได้เลย
        routeManager = RouteManager.Instance;

        // ChoiceUIManager เป็น Object ที่อยู่ในซีน จึงต้องใช้ FindObjectOfType เพื่อค้นหา
        choiceUIManager = FindObjectOfType<ChoiceUIManager>(true);
        //choiceUIManager = FindFirstObjectByType<ChoiceUIManager>();

        if (routeManager == null)   
            Debug.LogError("[PlayerPathWalker] Critical Error: Could not find RouteManager.Instance!");
        if (choiceUIManager == null)
            Debug.LogWarning("[PlayerPathWalker] Could not find ChoiceUIManager in the current scene. Path selection will not be available.");
    }

    #endregion

    #region Public API

    /// <summary>
    /// เมธอดหลักสำหรับสั่งให้ผู้เล่นเริ่มเดิน
    /// </summary>
    /// <param name="steps">จำนวนก้าวที่ได้จากการทอยเต๋า</param>
    public void ExecuteMove(int steps)
    {
        if (isExecutingTurn)
        {
            Debug.LogWarning("Player is already moving.");
            return;
        }
        if (steps <= 0)
        {
            Debug.Log("Move command ignored (0 steps).");
            return;
        }
        if (CurrentNodeTransform == null)
        {
            Debug.LogError("CurrentNodeTransform is NULL! Cannot start move. Make sure player is spawned correctly.");
            return;
        }
        GiveTurnStartBonus();
        stepsRemaining = steps;
        Debug.Log($"<color=cyan>--- Starting Turn: Move {steps} steps from {CurrentNodeTransform.name} ---</color>");
        StartCoroutine(MoveTurnCoroutine());
    }

    /// <summary>
    /// ย้ายผู้เล่นไปยังตำแหน่งของ Node ที่ระบุทันที (ใช้สำหรับตอนเริ่มเกมหรือ Event วาร์ป)
    /// </summary>
    public void TeleportToNode(Transform targetNode)
    {
        if (isExecutingTurn || targetNode == null || routeManager == null) return;

        transform.position = targetNode.position;
        // อัปเดต ID ของตำแหน่งปัจจุบันให้ถูกต้อง
        currentNodeID = routeManager.ExtractNumberFromName(targetNode.name);
    }

    #endregion

    #region Internal Logic

    /// <summary>
    /// Coroutine ที่จัดการ Logic การเดินในแต่ละช่อง การเลือกเส้นทาง และการส่ง Event
    /// </summary>
    private IEnumerator MoveTurnCoroutine()
    {
        isExecutingTurn = true;

        if (choiceUIManager != null) choiceUIManager.HideChoices();

        while (stepsRemaining > 0)
        {
            if (routeManager == null)
            {
                Debug.LogError("RouteManager is missing! Aborting move.");
                break;
            }

            List<Transform> choices = routeManager.GetAllConnectedNodes(CurrentNodeTransform);
            Transform nextNode = null;

            if (choices.Count == 0)
            {
                Debug.LogWarning($"Player is at a dead end: {CurrentNodeTransform.name}. Move aborted.");
                break; // ไม่มีทางไปต่อ
            }
            else if (choices.Count == 1)
            {
                nextNode = choices[0]; // ทางเดียว ไปได้เลย
            }
            else
            {
                // มีหลายทางเลือก (ทางแยก)
                if (choiceUIManager != null)
                {
                    Debug.Log($"Path selection needed at {CurrentNodeTransform.name}.");
                    chosenNodeFromUI = null;
                    choiceUIManager.DisplayChoices(choices, OnPathChosen);
                    // รอจนกว่าผู้เล่นจะกดเลือกทางจาก UI
                    yield return new WaitUntil(() => chosenNodeFromUI != null);
                    nextNode = chosenNodeFromUI;
                }
                else
                {
                    Debug.LogError("ChoiceUIManager is missing! Cannot proceed with multiple path choices. Aborting move.");
                    break;
                }
            }

            // สั่งให้ PlayerMovement เคลื่อนที่ไป
            playerMovement.MoveTo(nextNode);
            yield return new WaitUntil(() => !playerMovement.IsMoving); // รอจนกว่าจะเดินถึง

            // อัปเดตตำแหน่งปัจจุบัน (ID)
            currentNodeID = routeManager.ExtractNumberFromName(nextNode.name);
            stepsRemaining--;
            Debug.Log($"Arrived at {CurrentNodeTransform.name}. Steps remaining: {stepsRemaining}");

            if (stepsRemaining > 0)
                yield return new WaitForSeconds(delayAfterNodeArrival);
        }

        // --- จบเทิร์น ---
        Debug.Log($"<color=yellow>--- Turn Ended at {CurrentNodeTransform.name} ---</color>");

        // ส่ง Event บอกระบบอื่นว่าเดินถึงที่หมายสุดท้ายแล้ว
        NodeConnection finalNodeData = routeManager.GetNodeData(currentNodeID);
        if (finalNodeData != null && EventManager.Instance != null)
        {
            EventManager.Instance.RaisePlayerLandedOnNode(finalNodeData, this.gameObject);
        }

        isExecutingTurn = false;
    }

    /// <summary>
    /// Callback ที่ถูกเรียกโดย ChoiceUIManager เมื่อผู้เล่นกดเลือกเส้นทาง
    /// </summary>
    private void OnPathChosen(Transform chosenNode)
    {
        chosenNodeFromUI = chosenNode;
        if (choiceUIManager != null) choiceUIManager.HideChoices();
    }

    #endregion

    public void SetChoiceUIManager(ChoiceUIManager newUI)
    {
        this.choiceUIManager = newUI;
        Debug.Log("[PlayerPathWalker] ChoiceUIManager reference updated successfully.");
    }
    private void GiveTurnStartBonus()
    {
        if (PlayerState.Instance == null) return;

        int starBonus = Random.Range(1, 4); // สุ่ม 1, 2, หรือ 3
        PlayerState.Instance.PlayerStar += starBonus;

        Debug.Log($"🌟 Turn Start Bonus! ได้รับดาว {starBonus} ดวง (รวม: {PlayerState.Instance.PlayerStar})");

        // ถ้าคุณมี UI ที่ต้องอัปเดตดาว สามารถสั่งตรงนี้ได้เลย
        // เช่น UIManager.Instance.UpdateStarUI();
    }

    public void WarpByCard(Transform targetNode)
    {
        if (RouteManager.Instance == null) return;

        Debug.Log($"[Card Effect] กำลังวาร์ปผู้เล่นไปยัง: {targetNode.name}");

        // 1. ย้ายตัวละครทางกายภาพ (Visual)
        transform.position = targetNode.position;

        // 2. อัปเดต Logic ว่าตอนนี้เรายืนอยู่ที่ Node ไหน (แก้ตรงนี้!)
        bool found = false;
        
        for (int i = 0; i < RouteManager.Instance.nodeConnections.Count; i++)
        {
            // เช็คว่า Node ในลิสต์ ตรงกับ Node ที่เราเลือกไหม
            if (RouteManager.Instance.nodeConnections[i].node == targetNode)
            {
                // ---------------------------------------------------------
                // 🔴 จุดที่แก้ไข: ใช้ currentNodeID และดึงค่า tileID มาใส่
                // ---------------------------------------------------------
                currentNodeID = RouteManager.Instance.nodeConnections[i].tileID;
                
                Debug.Log($"[Card Effect] อัปเดตตำแหน่งเป็น Node ID: {currentNodeID}");
                found = true;
                break; // เจอแล้วหยุดหา
            }
        }

        if (!found)
        {
            Debug.LogError("[Card Effect] ไม่พบ Node ปลายทางใน RouteManager! ระบบเดินอาจผิดพลาด");
        }
        
        // 3. (Optional) Play Sound
        // AudioManager.Instance.PlaySfx("WarpSound");
    }
}