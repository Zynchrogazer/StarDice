using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    // 🆔 บัตรประชาชน: สุ่มเลขประจำตัวให้ BoardManager ตัวนี้
    private int myID;

    private void Awake()
    {
        myID = Random.Range(1000, 9999); // สุ่มเลข 4 หลัก
    }
    // ✅ 1. ใช้ Start เพื่อเชื่อมต่อ "ตอนเริ่มเกมครั้งแรก" (แก้ปัญหา Event ไม่ติดตอนเริ่ม)
    private void Start()
    {
        Debug.Log($"🔎 [BoardManager #{myID}] 🟢 เริ่มทำงานและกำลังตามหา GameTurnManager...");
        ConnectToEvent();

        // --- 🕵️‍♂️ โซนนักสืบ: ตรวจสอบสถานะ Manager ---

        // 1. เช็คว่า GameTurnManager มีชีวิตอยู่ไหม?
        if (GameTurnManager.Instance == null)
        {
            Debug.LogError("😱 [CRITICAL] GameTurnManager หายสาบสูญ! (Instance is NULL)");
            Debug.LogError("👉 สาเหตุที่เป็นไปได้: ลืมใส่ DontDestroyOnLoad หรือถูกทำลายซ้ำซ้อน");
            return;
        }
        else
        {
            Debug.Log($"✅ พบ GameTurnManager (State: {GameTurnManager.Instance.currentState})");
        }

        // 2. เช็คว่า GameEventManager มีชีวิตอยู่ไหม?
        if (GameEventManager.Instance == null)
        {
            Debug.LogError("😱 [CRITICAL] GameEventManager หายสาบสูญ! (Instance is NULL)");
        }
        else
        {
            // สั่งล้างค่าสุ่มค้างไว้ก่อนเลย
            GameEventManager.Instance.isRandomSpinning = false;
        }

        // --- ⚡ โซนเครื่องปั๊มหัวใจ: ปลุกเดี๋ยวนี้! ---

        // เรียกใช้ฟังก์ชันที่เราเพิ่งแก้เป็น public เมื่อกี้
        Debug.Log("⚡ [BoardManager] กำลังสั่ง GameTurnManager.HandleReturnFromBattle() แบบ Direct Call");
        GameTurnManager.Instance.HandleReturnFromBattle();
    }

    // ✅ 2. ใช้ OnEnable เพื่อเชื่อมต่อ "ตอนกลับมาจาก Battle"
    private void OnEnable() 
    {
        Debug.Log($"🟢 [BoardManager #{myID}] ถูกปลุก (OnEnable)");
        ConnectToEvent();
    }

    // ✅ 3. ใช้ OnDisable เพื่อถอดสายตอนไปฉากอื่น
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerLandedOnNode -= HandleTileEffect;
        }
    }

    // ฟังก์ชันสำหรับเชื่อมต่อ (เขียนแยกออกมาจะได้เรียกใช้ซ้ำได้)
    private void ConnectToEvent()
    {
        if (EventManager.Instance != null)
        {
            // 🛡️ เทคนิคสำคัญ: สั่ง "ถอดสายเก่าออกก่อน" เสมอ (ถึงไม่มีก็ไม่ error)
            // เพื่อป้องกันการเชื่อมซ้ำ 2 รอบ ซึ่งจะทำให้ Event เบิ้ล
            EventManager.Instance.OnPlayerLandedOnNode -= HandleTileEffect;

            // แล้วค่อยเสียบสายใหม่
            EventManager.Instance.OnPlayerLandedOnNode += HandleTileEffect;

            // Debug.Log("[BoardManager] 🟢 Connected to EventManager");
        }
    }

    // 🟢 รับ Event ตกช่อง -> ตัดสินใจว่าจะทำอะไรต่อ
    private void HandleTileEffect(NodeConnection nodeData, GameObject playerObject)
    {
        // 🔥 ให้มันแสดงเลข ID ตอนทำงานด้วย
        Debug.Log($"[BoardManager #{myID}] 🏁 {playerObject.name} landed on Tile ID: {nodeData.tileID}");
        

        // -----------------------------------------------------------------------
        // ⚔️ 1. เงื่อนไขที่ 1: ตกใส่ผู้เล่น (PvP)
        // -----------------------------------------------------------------------
        // อันนี้ AI ต้องทำเสมอ (ตามโจทย์) ถ้าเจอคนยืนอยู่ สู้ทันที!
        if (CheckForBattle(playerObject, nodeData.tileID))
        {
            return; // ตัดเข้าฉากสู้ -> จบฟังก์ชัน
        }

        // เช็คว่าเป็น AI หรือเปล่า?
        PlayerState pState = playerObject.GetComponent<PlayerState>();
        bool isAI = (pState != null && pState.isAI);

        // -----------------------------------------------------------------------
        // 🔮 2. ตัดสินใจตามประเภทช่อง
        // -----------------------------------------------------------------------
        switch (nodeData.type)
        {
            // กลุ่มช่องพื้นฐาน (จบเทิร์นเลย)
            case TileType.Normal:
                StartCoroutine(FinishTurnRoutine());
                break;
            case TileType.Start:
                // ถ้าเป็นคน (ไม่ใช่ AI) ให้เช็ค Norma ก่อน
                if (!isAI && NormaSystem.Instance != null)
                {
                    // เรียกฟังก์ชันที่เราเพิ่งแก้เป็น bool
                    bool leveledUp = NormaSystem.Instance.CheckNormaCondition();

                    if (leveledUp)
                    {
                        Debug.Log("[BoardManager] 🎉 Norma Level Up! รอผู้เล่นเลือกเป้าหมายใหม่...");
                        return; // 🛑 หยุด! อย่าเพิ่งจบเทิร์น (รอผู้เล่นกด UI เลือกเสร็จก่อน)
                    }
                }
                StartCoroutine(FinishTurnRoutine());
                break;

            // 🌀 เงื่อนไขที่ 2: ช่อง Teleport (AI ต้องวาร์ปได้)
            case TileType.Teleport:
                Debug.Log($"[BoardManager] 🌀 Teleport Tile! Triggering Warp Event.");
                if (GameEventManager.Instance != null)
                {
                    // สั่งวาร์ปทั้งคนทั้ง AI
                    GameEventManager.Instance.TriggerEvent("warp", playerObject);
                }
                else
                {
                    StartCoroutine(FinishTurnRoutine());
                }
                break;

            // 🛑 กลุ่มช่องอื่นๆ ทั้งหมด (Trap, Event, Monster, Treasure, Minigame, etc.)
            default:
                if (isAI)
                {
                    // 🤖 ถ้าเป็น AI -> "เมินหมด!"
                    Debug.Log($"[BoardManager] 🤖 AI {playerObject.name} เมินช่อง {nodeData.type} -> จบเทิร์นทันที");

                    // ข้าม Event Manager ไปเลย แล้วจบเทิร์น
                    StartCoroutine(FinishTurnRoutine());
                }
                else
                {
                    // 👤 ถ้าเป็นคน -> "เล่น Event ตามปกติ"
                    TriggerEventForHuman(nodeData, playerObject);
                }
                break;
        }
    }

    // ฟังก์ชันช่วย Trigger Event สำหรับคนเล่น (แยกออกมาให้อ่านง่าย)
    private void TriggerEventForHuman(NodeConnection nodeData, GameObject playerObject)
    {
        if (GameEventManager.Instance == null)
        {
            StartCoroutine(FinishTurnRoutine());
            return;
        }

        switch (nodeData.type)
        {
            // 1. มอนสเตอร์ทั่วไป -> ไป TestFight
            case TileType.Monster:
                Debug.Log($"[BoardManager] ⚔️ Monster Encounter!");
                GameEventManager.Instance.TriggerEvent("battle", playerObject);
                break;

            // 2. บอส -> ไป bossfire (ต้องแยกออกมา!)
            case TileType.Boss:
                Debug.Log($"[BoardManager] 👿 BOSS FIGHT! Triggering Boss Event.");
                // ✅ ส่ง Event ชื่อ "boss" เพื่อให้ GameEventManager โหลดฉาก bossfire
                GameEventManager.Instance.TriggerEvent("boss", playerObject);
                break;
            

            // 3. กรณีอื่นๆ -> ใช้ชื่อ Event ตามที่ตั้งไว้ใน RouteManager
            default:
                Debug.Log($"[BoardManager] ✨ Triggering Event: {nodeData.eventName}");
                GameEventManager.Instance.TriggerEvent(nodeData.eventName, playerObject);
                break;
        }
    }

    private IEnumerator FinishTurnRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.RequestEndTurn();
        }
    }

    private bool CheckForBattle(GameObject currentPlayer, int currentTileID)
    {
        PlayerPathWalker[] allPlayers = FindObjectsOfType<PlayerPathWalker>();
        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer.gameObject == currentPlayer) continue;
            if (otherPlayer.currentNodeID == currentTileID)
            {
                // เจอคนอื่นยืนช่องเดียวกัน -> สู้!
                StartBattle(currentPlayer, otherPlayer.gameObject);
                return true;
            }
        }
        return false;
    }
 
    private void StartBattle(GameObject attacker, GameObject defender)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Loading Battle Scene (PvP): {attacker.name} vs {defender.name}");
         
         if (currentSceneName == "MainLight") 
    {
        string[] Scenes = { "Light buff", "Light damage", "Light heal", "Lightone" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }

     if (currentSceneName == "TestMain") 
    {
        string[] Scenes = { "enemyfire buff", "enemyfire damage", "enemyfire heal", "enemyfire1" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }
     if (currentSceneName == "MainWater") 
    {
        string[] Scenes = { "enemy water buff", "enemy water damage", "enemy water heal", "enemy water" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }
    if (currentSceneName == "MainWind") 
    {
        string[] Scenes = { "enemy wind buff", "enemy wind damage", "enemy wind heal", "enemy wind" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }
     if (currentSceneName == "MainEarth") 
    {
        string[] Scenes = { "enemy earth buff", "enemy earth damage", "enemy earth heal", "enemy earth" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }
    if (currentSceneName == "MainDark") 
    {
        string[] Scenes = { "enemy dark buff", "enemy dark damage", "enemy dark heal", "enemy dark" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);      
    }
}

}