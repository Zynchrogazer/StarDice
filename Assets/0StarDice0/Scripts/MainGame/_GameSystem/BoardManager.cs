using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    // 🆔 บัตรประชาชน: สุ่มเลขประจำตัวให้ BoardManager ตัวนี้
    private int myID;

    [Header("References")]
    [SerializeField] private EventManager eventManager;

    private void Awake()
    {
        myID = Random.Range(1000, 9999); // สุ่มเลข 4 หลัก
        ResolveEventManager();
    }
    // ✅ 1. ใช้ Start เพื่อเชื่อมต่อ "ตอนเริ่มเกมครั้งแรก" (แก้ปัญหา Event ไม่ติดตอนเริ่ม)
    private void Start()
    {
        Debug.Log($"🔎 [BoardManager #{myID}] 🟢 เริ่มทำงานและกำลังตามหา GameTurnManager...");
        ConnectToEvent();

        // --- 🕵️‍♂️ โซนนักสืบ: ตรวจสอบสถานะ Manager ---

        // 1. เช็คว่า GameTurnManager มีชีวิตอยู่ไหม?
        if (!GameTurnManager.TryGet(out var gameTurnManager))
        {
            Debug.LogError("😱 [CRITICAL] GameTurnManager หายสาบสูญ! (Instance is NULL)");
            Debug.LogError("👉 สาเหตุที่เป็นไปได้: ไม่มี GameTurnManager ใน Board Scene หรือถูกทำลายผิดจังหวะ");
            return;
        }
        else
        {
            Debug.Log($"✅ พบ GameTurnManager (State: {gameTurnManager.currentState})");
        }

        // 2. เช็คว่า GameEventManager มีชีวิตอยู่ไหม?
        if (!GameEventManager.TryGet(out _))
        {
            Debug.LogError("😱 [CRITICAL] GameEventManager หายสาบสูญ! (Instance is NULL)");
        }
        else
        {
            // สั่งล้างค่าสุ่มค้างไว้ก่อนเลย
            GameEventManager.SetRandomSpinning(false);
        }

        // --- ⚡ โซนเครื่องปั๊มหัวใจ: ปลุกเดี๋ยวนี้! ---

        // เรียกใช้ฟังก์ชันที่เราเพิ่งแก้เป็น public เมื่อกี้
        Debug.Log("⚡ [BoardManager] กำลังสั่ง GameTurnManager.HandleReturnFromBattle() แบบ Direct Call");
        gameTurnManager.HandleReturnFromBattle();
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
        if (eventManager != null)
        {
            eventManager.OnPlayerLandedOnNode -= HandleTileEffect;
        }
    }

    // ฟังก์ชันสำหรับเชื่อมต่อ (เขียนแยกออกมาจะได้เรียกใช้ซ้ำได้)
    private void ConnectToEvent()
    {
        ResolveEventManager();
        if (eventManager != null)
        {
            // 🛡️ เทคนิคสำคัญ: สั่ง "ถอดสายเก่าออกก่อน" เสมอ (ถึงไม่มีก็ไม่ error)
            // เพื่อป้องกันการเชื่อมซ้ำ 2 รอบ ซึ่งจะทำให้ Event เบิ้ล
            eventManager.OnPlayerLandedOnNode -= HandleTileEffect;

            // แล้วค่อยเสียบสายใหม่
            eventManager.OnPlayerLandedOnNode += HandleTileEffect;

            // Debug.Log("[BoardManager] 🟢 Connected to EventManager");
        }
        else
        {
            Debug.LogWarning("[BoardManager] EventManager not found in scene.");
        }
    }

    private void ResolveEventManager()
    {
        if (eventManager == null)
        {
            eventManager = FindFirstObjectByType<EventManager>();
        }
    }

    // 🟢 รับ Event ตกช่อง -> ตัดสินใจว่าจะทำอะไรต่อ
   private void HandleTileEffect(NodeConnection nodeData, GameObject playerObject)
    {
        Debug.Log($"[BoardManager #{myID}] 🏁 {playerObject.name} landed on Tile ID: {nodeData.tileID}");
        
        if (CheckForBattle(playerObject, nodeData.tileID))
        {
            return; 
        }

        PlayerState pState = playerObject.GetComponent<PlayerState>();
        bool isAI = (pState != null && pState.isAI);

        // -----------------------------------------------------------------------
        // 🟢 ลูกเล่นพิเศษฉาก MainWater: AI เหยียบช่อง iceeffect แล้วแยกร่าง!
        // -----------------------------------------------------------------------
       if (isAI && nodeData.eventName == "iceeffect")
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == "MainWater")
            {
                // 1. นับจำนวน AI ทั้งหมดที่มีอยู่บนกระดานตอนนี้
                int currentAICount = 0;
                PlayerState[] allPlayersInScene = FindObjectsOfType<PlayerState>();
                foreach (PlayerState p in allPlayersInScene)
                {
                    if (p.isAI) currentAICount++;
                }

                // 2. เช็คเงื่อนไข: ต้องไม่ใช่ร่างโคลนมาเหยียบซ้ำ และ AI รวมต้องน้อยกว่า 3 ตัว
                if (!playerObject.name.EndsWith("_Clone"))
                {
                    if (currentAICount < 3)
                    {
                        Debug.Log($"❄️ [MainWater Gimmick] AI เหยียบช่องน้ำแข็ง! กำลังใช้วิชาแยกเงา... (ปัจจุบันมี AI {currentAICount}/3 ตัว)");
                        CloneAI(playerObject, nodeData.tileID);
                    }
                    else
                    {
                        Debug.Log("❄️ [MainWater Gimmick] AI ถึงขีดจำกัดแล้ว (3 ตัว) ไม่สามารถแยกร่างเพิ่มได้อีก!");
                    }
                }
                
                // จบเทิร์นทันที ข้าม Event การลดเต๋า
                StartCoroutine(FinishTurnRoutine());
                return;
            }
        }
        switch (nodeData.type)
        {
            // กลุ่มช่องพื้นฐาน (จบเทิร์นเลย)
            case TileType.Normal:
                StartCoroutine(FinishTurnRoutine());
                break;
            case TileType.Start:
                // ถ้าเป็นคน (ไม่ใช่ AI) ให้เช็ค Norma ก่อน
                if (!isAI && NormaSystem.TryGet(out var normaSystem))
                {
                    // 🟢 ต้องเรียก CanLevelUp() นะครับ (อย่าใช้ CheckNormaCondition)
                    bool readyToLevelUp = normaSystem.CanLevelUp();

                    if (readyToLevelUp)
                    {
                        Debug.Log("[BoardManager] 🎉 เงื่อนไขครบแล้ว! เปิดหน้าต่างให้ผู้เล่นกดยืนยันส่งเควส...");
                        
                        // ไปตามหา UIManager แล้วสั่งเปิด Panel ส่งเควส
                        NormaUIManager uiManager = FindFirstObjectByType<NormaUIManager>();
                        if (uiManager != null) 
                        {
                            uiManager.ShowSubmitPanel();
                        }
                        else
                        {
                            Debug.LogError("หา NormaUIManager ไม่เจอ! หน้าต่างเลยไม่เด้ง");
                        }
                        
                        return; // 🛑 หยุดเทิร์นไว้ตรงนี้ก่อน รอผู้เล่นกดปุ่มใน UI
                    }
                }
                StartCoroutine(FinishTurnRoutine());
                break;

            // 🌀 เงื่อนไขที่ 2: ช่อง Teleport (AI ต้องวาร์ปได้)
            case TileType.Teleport:
                Debug.Log($"[BoardManager] 🌀 Teleport Tile! Triggering Warp Event.");
                if (GameEventManager.TryGet(out _))
                {
                    // สั่งวาร์ปทั้งคนทั้ง AI
                    GameEventManager.TryTriggerEvent("warp", playerObject);
                }
                else
                {
                    StartCoroutine(FinishTurnRoutine());
                }
                break;

            // 🛑 กลุ่มช่องอื่นๆ ทั้งหมด (Trap, Event, Monster, Treasure, Minigame, etc.)
           // 🛑 กลุ่มช่องอื่นๆ ทั้งหมด (Trap, Event, Monster, Treasure, Minigame, etc.)
            default:
                if (isAI)
                {
                    // 🟢 เพิ่มข้อยกเว้น: ถ้าช่องนั้นคือ "Lava" ให้ AI โดน Event ด้วย!
                    if (!string.IsNullOrEmpty(nodeData.eventName) && nodeData.eventName.ToLower() == "lava")
                    {
                        Debug.Log($"[BoardManager] 🌋 AI {playerObject.name} เหยียบช่อง Lava! ส่งต่อให้ EventManager จัดการ...");
                        GameEventManager.TryTriggerEvent("lava", playerObject);
                    }
                    else
                    {
                        // 🤖 ถ้าเป็น AI แล้วเป็นช่องอื่นๆ (หีบ, มอนสเตอร์, มินิเกม) -> "เมินหมด!"
                        Debug.Log($"[BoardManager] 🤖 AI {playerObject.name} เมินช่อง {nodeData.type} ({nodeData.eventName}) -> จบเทิร์นทันที");
                        StartCoroutine(FinishTurnRoutine());
                    }
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
    // ฟังก์ชันช่วย Trigger Event สำหรับคนเล่น
    private void TriggerEventForHuman(NodeConnection nodeData, GameObject playerObject)
    {
        if (!GameEventManager.TryGet(out _))
        {
            StartCoroutine(FinishTurnRoutine());
            return;
        }

        switch (nodeData.type)
        {
            // 1. มอนสเตอร์ทั่วไป -> ไป TestFight
            case TileType.Monster:
                Debug.Log($"[BoardManager] ⚔️ Monster Encounter!");
                GameEventManager.TryTriggerEvent("battle", playerObject);
                break;

            // 2. บอส -> ไปฉากบอส (เปิดสวิตช์จบเกมด้วย!)
            case TileType.Boss:
            // case TileType.SpecialBoss:
                Debug.Log($"[BoardManager] 👿 BOSS FIGHT! Triggering Boss Event.");
                
                // -------------------------------------------------------------
                // 🟢 เปิดสวิตช์ล่องหน เพื่อบอกระบบหลังต่อสู้จบว่า "นี่คือบอส!"
                PlayerPrefs.SetInt("IsBossBattle", 1);
                PlayerPrefs.Save();
                // -------------------------------------------------------------
                
                // ส่ง Event ชื่อ "boss" เพื่อให้ GameEventManager โหลดฉากบอส
                GameEventManager.TryTriggerEvent("boss", playerObject);
                break;

            // 3. กรณีอื่นๆ -> ใช้ชื่อ Event ตามที่ตั้งไว้ใน RouteManager
            default:
                Debug.Log($"[BoardManager] ✨ Triggering Event: {nodeData.eventName}");
                GameEventManager.TryTriggerEvent(nodeData.eventName, playerObject);
                break;
        }
    }

    private IEnumerator FinishTurnRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.RequestEndTurn();
        }
    }

  private bool CheckForBattle(GameObject currentPlayer, int currentTileID)
    {
        // 1. ดึงข้อมูลว่าคนที่กำลังเดินอยู่ (Attacker) เป็น AI หรือเปล่า
        PlayerState currentPState = currentPlayer.GetComponent<PlayerState>();
        bool isCurrentPlayerAI = (currentPState != null && currentPState.isAI);

        PlayerPathWalker[] allPlayers = FindObjectsOfType<PlayerPathWalker>();
        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer.gameObject == currentPlayer) continue;

            if (otherPlayer.currentNodeID == currentTileID)
            {
                // 2. ดึงข้อมูลคนที่ยืนรออยู่ (Defender)
                PlayerState otherPState = otherPlayer.GetComponent<PlayerState>();
                bool isOtherPlayerAI = (otherPState != null && otherPState.isAI);

                // 🛑 กฎเหล็ก: ถ้า "คนเดิน" เป็น AI และ "คนรอ" ก็เป็น AI -> ห้ามตีกันเด็ดขาด!
                if (isCurrentPlayerAI && isOtherPlayerAI)
                {
                    Debug.Log($"🤖 [BoardManager] AI เดินชนกันเอง ({currentPlayer.name} ชนกับ {otherPlayer.gameObject.name}) -> เมินใส่กัน ไม่สู้!");
                    
                    // ใช้ continue เพื่อข้ามคนนี้ไปเลย (เผื่อมี "ผู้เล่นคนจริง" ยืนซ้อนอยู่ในช่องนี้อีกคน จะได้ข้ามไปตีคนเล่นแทน!)
                    continue; 
                }

                GameObject attacker = currentPlayer;
                GameObject defender = otherPlayer.gameObject;

                // 🟢 รวบรวมร่างโคลนกลับมาเป็นตัวเดียวก่อนตัดเข้าฉากสู้!
                MergeAllAIClones(ref attacker, ref defender);

                // เจอศัตรูที่ไม่ใช่พวกเดียวกันยืนช่องเดียวกัน -> สู้!
                StartBattle(attacker, defender);
                return true;
            }
        }
        return false;
    }

    private void StartBattle(GameObject attacker, GameObject defender)
    {
        Debug.Log($"Loading Battle Scene (PvP): {attacker.name} vs {defender.name}");

        string currentSceneName = SceneManager.GetActiveScene().name; 

        PlayerPrefs.SetString(GameEventManager.LastBoardSceneKey, currentSceneName);
        PlayerPrefs.Save();
        Debug.Log($"<color=magenta>💾 [BoardManager] บันทึกชื่อด่าน '{currentSceneName}' ก่อนเข้าฉากสู้เรียบร้อย!</color>");
    
    string battleSceneName = "TestFight"; 

    if (currentSceneName == "MainLight")
    {
        string[] randomScenes = { "Light buff", "Light damage", "Light heal", "Lightone" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        
        battleSceneName = randomScenes[randomIndex];
    }
    else if (currentSceneName == "TestMain")
    {
        string[] randomScenes = { "enemyfire buff", "enemyfire damage", "enemyfire heal", "enemyfire1" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        battleSceneName = randomScenes[randomIndex];
    }
    else if (currentSceneName == "MainWater")
    {
        string[] randomScenes = { "enemy water", "enemy water buff", "enemy water damage", "enemy water heal" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        battleSceneName = randomScenes[randomIndex];
    }
     else if (currentSceneName == "MainWind")
    {
        string[] randomScenes = { "enemy wind", "enemy wind buff", "enemy wind damage", "enemy wind heal" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        battleSceneName = randomScenes[randomIndex];
    }
    else if (currentSceneName == "MainEarth")
    {
        string[] randomScenes = { "enemy earth", "enemy earth buff", "enemy earth damage", "enemy earth heal" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        battleSceneName = randomScenes[randomIndex];
    }
     else if (currentSceneName == "MainDark")
    {
        string[] randomScenes = { "enemy dark", "enemy dark buff", "enemy dark damage", "enemy dark heal" };
        
        int randomIndex = Random.Range(0, randomScenes.Length);
        battleSceneName = randomScenes[randomIndex];
    }

    if (!GameEventManager.TryLoadBattleSceneAdditive(battleSceneName))
    {
        Debug.LogError($"[BoardManager] Failed to load PvP battle scene '{battleSceneName}' additively.");
    }
    }

    // ==========================================
    // 👥 โซนเวทมนตร์แยกร่าง/รวมร่าง AI
    // ==========================================
    
    // ฟังก์ชันสำหรับแยกร่าง AI
    private void CloneAI(GameObject originalAI, int tileID)
    {
        // ก๊อปปี้ตัวละครขึ้นมาใหม่
        GameObject cloneAI = Instantiate(originalAI, originalAI.transform.position, originalAI.transform.rotation);
        cloneAI.name = originalAI.name + "_Clone"; // เติมชื่อให้รู้ว่าเป็นร่างโคลน

        // บอกร่างโคลนว่าตัวเองยืนอยู่ช่องไหน
        PlayerPathWalker cloneWalker = cloneAI.GetComponent<PlayerPathWalker>();
        if (cloneWalker != null)
        {
            cloneWalker.currentNodeID = tileID;
        }

        // ยัดร่างโคลนเข้าคิวใน GameTurnManager เพื่อให้มันทอยเต๋าเดินไล่ล่าในเทิร์นหน้าได้!
        if (GameTurnManager.TryGet(out var turnManager))
        {
            PlayerState cloneState = cloneAI.GetComponent<PlayerState>();
            if (cloneState != null)
            {
                turnManager.allPlayers.Add(cloneState);
                Debug.Log($"👥 แยกร่างสำเร็จ! นำ {cloneAI.name} เข้าสู่คิวเรียบร้อย");
            }
        }
    }

    // ฟังก์ชันสำหรับสลายร่างโคลนทั้งหมดให้กลับมาเป็นตัวจริง
    private void MergeAllAIClones(ref GameObject attacker, ref GameObject defender)
    {
        PlayerState atkState = attacker.GetComponent<PlayerState>();
        PlayerState defState = defender.GetComponent<PlayerState>();

        // ถ้าสู้กันเองระหว่างคนเล่น ไม่ต้องทำอะไร
        bool isAiInvolved = (atkState != null && atkState.isAI) || (defState != null && defState.isAI);
        if (!isAiInvolved) return;

        PlayerState originalAI = null;
        List<PlayerState> clones = new List<PlayerState>();

        // แยกหาว่าใครคือตัวจริง ใครคือตัวโคลน
        foreach (var p in FindObjectsOfType<PlayerState>())
        {
            if (p.isAI)
            {
                if (p.gameObject.name.EndsWith("_Clone")) clones.Add(p);
                else originalAI = p;
            }
        }

        if (originalAI == null || clones.Count == 0) return; // ไม่มีโคลนให้รวม

        // ถ้าร่างที่เดินไปชน (Attacker) ดันเป็นโคลน ให้เอาตัวจริงมาสวมรอยแทน
        if (atkState != null && atkState.isAI && attacker.name.EndsWith("_Clone"))
        {
            originalAI.transform.position = attacker.transform.position;
            originalAI.GetComponent<PlayerPathWalker>().currentNodeID = attacker.GetComponent<PlayerPathWalker>().currentNodeID;
            attacker = originalAI.gameObject; // สลับเป็นตัวจริง
        }

        // ถ้าร่างที่โดนชน (Defender) เป็นโคลน ให้เอาตัวจริงมาสวมรอยแทน
        if (defState != null && defState.isAI && defender.name.EndsWith("_Clone"))
        {
            originalAI.transform.position = defender.transform.position;
            originalAI.GetComponent<PlayerPathWalker>().currentNodeID = defender.GetComponent<PlayerPathWalker>().currentNodeID;
            defender = originalAI.gameObject; // สลับเป็นตัวจริง
        }

        // เคลียร์ร่างโคลนที่เหลือทิ้งให้หมด!
        if (GameTurnManager.TryGet(out var turnManager))
        {
            foreach (var clone in clones)
            {
                turnManager.allPlayers.Remove(clone);
                Destroy(clone.gameObject);
            }
        }
        
        Debug.Log("💥 วิชาคลายเงา! AI ทุกตัวสลายร่างโคลนกลับมารวมที่ตัวจริง เพื่อเตรียมต่อสู้แล้ว!");
    }
}//
