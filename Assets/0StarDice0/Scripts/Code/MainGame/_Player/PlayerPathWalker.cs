using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerPathWalker : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float delayAfterNodeArrival = 0.5f;

    [Header("Audio")]
    public AudioClip walkSound;
    public AudioClip landSound;

    private AudioSource audioSource;

    [Range(0f, 1f)] public float soundVolume = 0.8f;
    [SerializeField] private EventManager eventManager;

    [Header("State")]
    public int currentNodeID;

    private RouteManager routeManager;
    private ChoiceUIManager choiceUIManager;
    private PlayerState myState;

    private int stepsRemaining = 0;
    private bool isExecutingTurn = false;
    private bool isMoving = false;
    private Transform chosenNodeFromUI;
    private int previousNodeID;

    public bool IsExecutingTurn => isExecutingTurn;
    public bool IsMoving => isMoving;
    public Transform CurrentNodeTransform => routeManager?.GetNodeData(currentNodeID)?.node;
    public int PreviousNodeID => previousNodeID;

    private void Awake()
    {
        myState = GetComponent<PlayerState>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true; // ให้เสียงวนลูปถ้ายาวไม่พอ
            audioSource.playOnAwake = false; // อย่าเพิ่งเล่นตอนเริ่มเกม
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TestFight" || scene.name == "Shop" || scene.name.Contains("Minigame")) return;

        RouteManager.TryGet(out routeManager);
        choiceUIManager = FindObjectOfType<ChoiceUIManager>();
    }

    private void Start()
    {
        if (routeManager == null) RouteManager.TryGet(out routeManager);
        if (eventManager == null) eventManager = FindFirstObjectByType<EventManager>();
    }

    public void ExecuteMove(int steps)
    {
        // 🛡️ Force Reset ป้องกันสถานะค้างจากเทิร์นก่อน
        if (isExecutingTurn || isMoving)
        {
            StopAllCoroutines();
            isExecutingTurn = false;
            isMoving = false;
        }

        if (steps <= 0)
        {
            CheckFinalNodeEvent(); // ถ้าได้ 0 ก้าว ให้เช็ค Event ช่องที่ยืนอยู่ทันที
            return;
        }

        GiveTurnStartBonus();
        stepsRemaining = steps;
        previousNodeID = currentNodeID;
        StartCoroutine(MoveTurnCoroutine());
    }

    private IEnumerator MoveTurnCoroutine()
    {
        isExecutingTurn = true;
        
        if (choiceUIManager != null) choiceUIManager.HideChoices();

        while (stepsRemaining > 0)
        {
            if (routeManager == null) break;

            List<Transform> choices = routeManager.GetAllConnectedNodes(CurrentNodeTransform);
            Transform nextNode = null;

            if (choices.Count == 0) break;
            else if (choices.Count == 1) nextNode = choices[0];
            else
            {
                // === ระบบทางแยก ===
                if (myState != null && myState.isAI)
                {
                    nextNode = choices[Random.Range(0, choices.Count)];
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    if (choiceUIManager != null)
                    {
                        chosenNodeFromUI = null;
                        choiceUIManager.DisplayChoices(choices, OnPathChosen);
                        yield return new WaitUntil(() => chosenNodeFromUI != null);
                        nextNode = chosenNodeFromUI;
                    }
                    else nextNode = choices[0];
                }
            }

            int nextTileID = routeManager.ExtractNumberFromName(nextNode.name);

            // สั่งเดินไปยังโหนดถัดไป
            yield return StartCoroutine(MoveTowardsCoroutine(nextNode));

            currentNodeID = nextTileID;

            if (TryBreakRockAndBounceBack(nextTileID))
            {
                Debug.Log($"🪨 {name} ชนหินที่ช่อง {nextTileID} หินแตกและเด้งกลับไปช่อง {currentNodeID}");
            }

            previousNodeID = currentNodeID;
            stepsRemaining--;

            if (stepsRemaining > 0)
                yield return new WaitForSeconds(delayAfterNodeArrival);
        }

        isExecutingTurn = false;
        CheckFinalNodeEvent(); // ✅ เดินจบแล้ว ไปเช็ค Event ต่อ
    }

    private void CheckFinalNodeEvent()
    {
        NodeConnection finalNodeData = routeManager?.GetNodeData(currentNodeID);
        bool hasGameTurnManager = GameTurnManager.TryGet(out var gameTurnManager);

        if (finalNodeData != null)
        {
            Debug.Log($"[PathWalker] {name} landed on ID: {currentNodeID}. Triggering Event...");

            if (audioSource != null && landSound != null)
            {
                // ปรับ Pitch กลับเป็นปกติ (เผื่อตอนเดินเราไปสุ่ม Pitch ไว้)
                //audioSource.pitch = 1.0f;
                //audioSource.PlayOneShot(landSound, soundVolume);
            }

            // ✅ 1. บอก Manager ให้ล็อค State ไว้ที่ EventProcessing (กัน AI วิ่งแซง)
            if (hasGameTurnManager)
                gameTurnManager.SetState(GameState.EventProcessing);

            // ✅ 2. ส่งไม้ต่อให้ EventManager (ใช้สคริปต์ EventManager ตัวเดิมของคุณ)
            if (eventManager != null)
            {
                eventManager.RaisePlayerLandedOnNode(finalNodeData, this.gameObject);
            }
            else
            {
                // ถ้าไม่มี EventManager ให้จบเทิร์นเลย
                if (hasGameTurnManager) gameTurnManager.RequestEndTurn();
            }
        }
        else
        {
            if (hasGameTurnManager) gameTurnManager.RequestEndTurn();
        }
    }

    private IEnumerator MoveTowardsCoroutine(Transform targetNode)
    {
        isMoving = true;
        if (audioSource != null && walkSound != null)
        {
            audioSource.PlayOneShot(walkSound);
            //Debug.Log($"<color=yellow>⭐ PlayWalkingSound");
        }
        while (Vector3.Distance(transform.position, targetNode.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetNode.position;
        isMoving = false;
    }

    public bool TryBreakRockAndBounceBack(int rockTileID)
    {
        if (routeManager == null || rockTileID <= 0)
        {
            return false;
        }

        if (!routeManager.IsRockObstacleActive(rockTileID))
        {
            return false;
        }

        bool wasBroken = routeManager.TryBreakRockObstacle(rockTileID);
        if (!wasBroken)
        {
            return false;
        }

        NodeConnection previousNode = routeManager.GetNodeData(previousNodeID);
        if (previousNode != null && previousNode.node != null)
        {
            transform.position = previousNode.node.position;
            currentNodeID = previousNodeID;
        }

        return true;
    }

    private void OnPathChosen(Transform chosenNode)
    {
        chosenNodeFromUI = chosenNode;
        if (choiceUIManager != null) choiceUIManager.HideChoices();
    }

    public void TeleportToNode(Transform targetNode)
    {
        if (targetNode == null || routeManager == null) return;
        transform.position = targetNode.position;
        currentNodeID = routeManager.ExtractNumberFromName(targetNode.name);
    }

    private void GiveTurnStartBonus()
    {
        if (myState == null) return;

        int starGain = Random.Range(1, 4);
        int totalGain = myState.AddStars(starGain);

        Debug.Log($"[PathWalker] {name} gained +{totalGain} stars at move start (base: {starGain}, bonus: +{myState.GetPerGainStarBonus()}, total: {myState.PlayerStar}).");
    }

    public void SetChoiceUIManager(ChoiceUIManager ui) { this.choiceUIManager = ui; }

    public void ReconnectReferences(RouteManager newRouteManager)
    {
        this.routeManager = newRouteManager;
        this.choiceUIManager = FindObjectOfType<ChoiceUIManager>();
        StopAllCoroutines();
        isExecutingTurn = false;
        isMoving = false;
    }

    public void WarpByCard(Transform targetNode)
    {
        if (!RouteManager.TryGet(out var routeManagerRef)) return;

        Debug.Log($"[Card Effect] กำลังวาร์ปผู้เล่นไปยัง: {targetNode.name}");

        // 1. ย้ายตัวละครทางกายภาพ (Visual)
        transform.position = targetNode.position;

        // 2. อัปเดต Logic ว่าตอนนี้เรายืนอยู่ที่ Node ไหน (แก้ตรงนี้!)
        bool found = false;

        for (int i = 0; i < routeManagerRef.nodeConnections.Count; i++)
        {
            // เช็คว่า Node ในลิสต์ ตรงกับ Node ที่เราเลือกไหม
            if (routeManagerRef.nodeConnections[i].node == targetNode)
            {
                // ---------------------------------------------------------
                // 🔴 จุดที่แก้ไข: ใช้ currentNodeID และดึงค่า tileID มาใส่
                // ---------------------------------------------------------
                currentNodeID = routeManagerRef.nodeConnections[i].tileID;

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
