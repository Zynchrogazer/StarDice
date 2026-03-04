using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    public const string LastBoardSceneKey = "LastBoardSceneName";

    [Header("Scene Settings")]
    public string boardGameSceneName = "TestMain";
    public static GameEventManager Instance { get; private set; }

    [Header("Random Event Settings")]
    public string[] randomEventKeys;
    public string[] randomMinigameEventKeys;
    public float randomSpinDuration = 3f;
    public float spinInterval = 0.3f;

    // --- ตัวแปรภายใน (รักษาชื่อเดิมของคุณไว้ทั้งหมด) ---
    private Dictionary<string, GameObject> eventPanels = new Dictionary<string, GameObject>();
    private Transform panelParent;
    private GameObject[] randomEventPanels = new GameObject[0];
    private GameObject[] randomMinigameEventPanels = new GameObject[0];
    public bool isRandomSpinning = false;
    private bool isFirstLoad = true;
    private bool hasStartedGame = false;
    public GameObject shopPanel;
    private GameObject currentEventTarget;

    public int[] windTeleportTargetIDs; 
    public string windTeleportPanelName = "windteleportpanel";
    // ✅ ตัวแปรเช็คสถานะการเล่น (เชื่อมกับ State Machine)

    public Sprite moneySprite; // 🖼️ ลากรูปเหรียญมาใส่ตรงนี้
    
    public bool isEventProcessing => isRandomSpinning || (GameTurnManager.Instance != null && GameTurnManager.Instance.currentState == GameState.EventProcessing);

    #region Unity Lifecycle & Scene Management

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == boardGameSceneName)
        {
            SetupReferences();
            if (isFirstLoad) { isFirstLoad = false; hasStartedGame = true; }
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    public static System.Action OnBoardSceneReady; // ✅ ช่องสัญญาณ Global

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. ลองหาดูว่าในซีนนี้มีระบบแผนที่ (RouteManager) อยู่ไหม?
        // (สมมติว่า RouteManager ไม่ได้เป็น DontDestroyOnLoad หรือถ้าเป็น ก็เช็คว่ามี Node ไหม)
        RouteManager mapSystem = FindObjectOfType<RouteManager>();

        // ✅ ถ้าเจอ RouteManager แสดงว่าเป็นฉากบอร์ดเกมแน่นอน (ไม่สนว่าชื่ออะไร)
        if (mapSystem != null && mapSystem.nodeConnections != null && mapSystem.nodeConnections.Count > 0)
        {
            Debug.Log($"<color=cyan>[EventManager] พบแผนที่ในซีน '{scene.name}' -> ถือเป็นฉาก Board Game</color>");

            SetupReferences();

            if (isFirstLoad)
            {
                isFirstLoad = false;
                return;
            }

            // ล้างค่าสุ่มค้าง
            isRandomSpinning = false;

            // ตะโกนบอก Manager ว่าพร้อมแล้ว
            OnBoardSceneReady?.Invoke();
        }
        else
        {
            Debug.Log($"[EventManager] ซีน '{scene.name}' ไม่มี RouteManager -> ไม่ทำอะไร (น่าจะเป็นฉากสู้/เมนู)");
        }
    }

    private void SetupReferences()
    {
        FindAndSetupMainPanels();
        FindAndSetupRandomEventPanels();
        FindAndSetupChoiceUI();

        RouteManager newMap = FindObjectOfType<RouteManager>();
        if (GameTurnManager.Instance != null && newMap != null)
        {
            foreach (var playerState in GameTurnManager.Instance.allPlayers)
            {
                playerState?.GetComponent<PlayerPathWalker>()?.ReconnectReferences(newMap);
            }
        }
    }

   

    #endregion

    #region Setup Functions (รักษารูปแบบเดิมของคุณ)

    private void FindAndSetupMainPanels()
    {
        GameObject foundParent = GameObject.Find("PanelParent");
        if (foundParent != null) { panelParent = foundParent.transform; InitializePanelDictionary(); }
    }

    private void InitializePanelDictionary()
    {
        eventPanels.Clear();
        if (panelParent == null) return;
        foreach (Transform child in panelParent)
        {
            eventPanels[child.name.ToLower()] = child.gameObject;
            child.gameObject.SetActive(false);
        }
    }

    private void FindAndSetupRandomEventPanels()
    {
        GameObject container = GameObject.Find("RandomEventPanelsContainer");
        if (container != null)
        {
            randomEventPanels = new GameObject[container.transform.childCount];
            for (int i = 0; i < container.transform.childCount; i++)
            {
                randomEventPanels[i] = container.transform.GetChild(i).gameObject;
                randomEventPanels[i].SetActive(false);
            }
        }

        GameObject minigameContainer = GameObject.Find("RandomMinigamePanelsContainer");
        if (minigameContainer != null)
        {
            randomMinigameEventPanels = new GameObject[minigameContainer.transform.childCount];
            for (int i = 0; i < minigameContainer.transform.childCount; i++)
            {
                randomMinigameEventPanels[i] = minigameContainer.transform.GetChild(i).gameObject;
                randomMinigameEventPanels[i].SetActive(false);
            }
        }
    }

    private void FindAndSetupChoiceUI()
    {
        ChoiceUIManager foundUI = FindObjectOfType<ChoiceUIManager>(true);
        if (foundUI != null && GameTurnManager.Instance != null)
        {
            foreach (var p in GameTurnManager.Instance.allPlayers)
                p?.GetComponent<PlayerPathWalker>()?.SetChoiceUIManager(foundUI);
        }
    }

    #endregion

    #region Main Event Logic

    public void TriggerEvent(string eventName, GameObject player)
    {
        // ✅ ล็อค State ทันทีที่เกิด Event
        if (GameTurnManager.Instance != null) GameTurnManager.Instance.SetState(GameState.EventProcessing);

        GameObject target = player != null ? player : currentEventTarget;
        if (target == null) target = GameTurnManager.CurrentPlayer?.gameObject;
        if (target == null) { GameTurnManager.Instance?.RequestEndTurn(); return; }

        switch (eventName.ToLower())
        {
            case "treasurebox": ApplyTreasureBoxEffect(target); break;
            case "trap": TrapEffect(target); break;
            case "drop": DropStarEffect(target); break;
            case "warp": case "teleport": RandomWarp(target); break;
            case "star": StarGain(target); break;
            case "heal": Heal(target); break;
            case "randomevent": TriggerRandomEvent(target); break;
            case "randomminigame": TriggerMinigameEvent(target); break;
            case "battle": StartCoroutine(MonsterBattleCoroutine()); break;
            case "boss":StartCoroutine(BossBattleCoroutine());break;
            case "specialboss":StartCoroutine(SpecialBossBattleCoroutine());break;
            case "shop":
                if (shopPanel != null) shopPanel.SetActive(true);
                else GameTurnManager.Instance?.RequestEndTurn();
                break;
            case "draw": Draw(target); break;
            case "windteleport": WindTeleportEffect(target); break;
            case "iceeffect": ApplyIceEffect(target); break;
            default:
                if (eventPanels.ContainsKey(eventName.ToLower())) ShowPanel(eventName, true);
                else GameTurnManager.Instance?.RequestEndTurn();
                break;
        }
    }

    private void ApplyIceEffect(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p != null)
        {
            p.hasIceEffect = true; // ✅ ติดสถานะแช่แข็ง
            Debug.Log($"<color=cyan>❄️ Player {target.name} ติดสถานะ Ice Effect! (ทอยครั้งหน้าหารครึ่ง)</color>");
        }
        
        // แสดง Panel แจ้งเตือน (อย่าลืมสร้าง Panel ชื่อ icepanel ใน Unity)
        ShowPanel("icepanel", true); 
    }

    private void WindTeleportEffect(GameObject target)
    {
        // 1. เช็คความปลอดภัย: ถ้าไม่ได้ใส่ ID ไว้เลย หรือ Player/Map ไม่พร้อม ให้หยุด
        if (windTeleportTargetIDs == null || windTeleportTargetIDs.Length == 0)
        {
            Debug.LogError("WindTeleport: ยังไม่ได้กำหนด ID ปลายทางใน Inspector!");
            GameTurnManager.Instance?.RequestEndTurn(); // จบเทิร์นกันเกมค้าง
            return;
        }

        PlayerPathWalker walker = target.GetComponent<PlayerPathWalker>();

        if (walker != null && RouteManager.Instance != null)
        {
            // 2. ✨ [สุ่ม] เลือก ID หนึ่งตัวจากรายการที่ใส่ไว้
            int randomIndex = Random.Range(0, windTeleportTargetIDs.Length);
            int chosenID = windTeleportTargetIDs[randomIndex];

            Debug.Log($"<color=cyan>WindTeleport: สุ่มได้ Node ID {chosenID} (จากทั้งหมด {windTeleportTargetIDs.Length} จุด)</color>");

            // 3. ค้นหา Node จาก ID ที่สุ่มได้
            var targetConnection = RouteManager.Instance.nodeConnections.Find(x => x.tileID == chosenID);

            if (targetConnection != null && targetConnection.node != null)
            {
                // วาปไปที่ Node นั้น
                walker.TeleportToNode(targetConnection.node);
            }
            else
            {
                Debug.LogError($"<color=red>WindTeleport Error: ไม่พบ Node ID {chosenID} ใน RouteManager!</color>");
            }
        }

        // แสดง Panel และรอจบเทิร์น
        ShowPanel(windTeleportPanelName, true);
    }


    [Header("ลากรูปใส่ตรงนี้ (เรียงตามลำดับ 0, 1, 2...)")]
    public Sprite[] itemImages; 

    [Header("ลาก UI Image เปล่าๆ มาใส่ตรงนี้")]
    public Image showImage;
    
    public ItemID Sword = ItemID.Sword; // สมมติกล่องนี้ดรอปดาบไฟ
    public ItemID Armor = ItemID.Armor; 
    public ItemID DawnRing = ItemID.DawnRign; 
    public ItemID WhiteFeather = ItemID.WhiteFeather; 
    public ItemID RecoverRing = ItemID.RecoverRing; 
    public ItemID HearthNeckless = ItemID.HearthNeckless; 
   
   public ItemID KnightSword = ItemID.KnightSword; 
   public ItemID KnightArmor = ItemID.KnightArmor; 
   public ItemID KnightShoes = ItemID.KnightShoes; 

   public ItemID LightSpear = ItemID.LightSpear; 
   public ItemID FireLegendarySword = ItemID.FireLegendarySword; 
   public ItemID WaterLegendaryArmor = ItemID.WaterLegendaryArmor; 
   public ItemID WindSpear = ItemID.WindSpear; 
   public ItemID EarthLegendaryArmor = ItemID.EarthLegendaryArmor; 
   public ItemID DarkLegendaryRing = ItemID.DarkLegendaryRing; 
    private void ApplyTreasureBoxEffect(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p == null) return;

        // 1. เปิด Panel กล่องสมบัติ (ต้องมั่นใจว่าใน Unity ตั้งชื่อ Panel ว่า "treasurebox" หรือชื่อที่คุณใช้)
        // autoClose = false เพราะเราจะคุมการปิดเอง
        if (!eventPanels.TryGetValue("openchestpanel", out var panel))
        {
            Debug.LogError("หา Panel 'openchestpanel' ไม่เจอ!");
            GameTurnManager.Instance.RequestEndTurn();
            return;
        }

        panel.SetActive(true);

        // 2. หาตัว BoxOpener ใน Panel นั้น
        BoxOpener boxScript = panel.GetComponentInChildren<BoxOpener>();
        if (boxScript == null)
        {
            Debug.LogError("ใน Panel ไม่มีสคริปต์ BoxOpener!");
            StartCoroutine(HidePanelAfterDelay(panel)); // กันตาย: ใช้ระบบเดิมไปก่อน
            return;
        }

        // 3. คำนวณรางวัล และเลือกรูป
        Sprite resultSprite = null;

        
            // 💰 ได้เงิน
            p.PlayerMoney += 100;
            resultSprite = moneySprite;
            Debug.Log("Treasure: Got Money");

             int roll = Random.Range(1, 101);
            
            if (roll < 51)
            {
                int randomItem = Random.Range(0, 6); 

            if (randomItem == 0)
                {
                    EquipmentManager.Instance.UnlockItem(Sword);
                    showImage.sprite = itemImages[0]; 
                    showImage.gameObject.SetActive(true);
                    Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
                }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(Armor);
                showImage.sprite = itemImages[1]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(DawnRing);
                showImage.sprite = itemImages[2]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 3){
                EquipmentManager.Instance.UnlockItem(WhiteFeather);
                showImage.sprite = itemImages[3]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 4){
                EquipmentManager.Instance.UnlockItem(RecoverRing);
                showImage.sprite = itemImages[4]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 5){
                EquipmentManager.Instance.UnlockItem(HearthNeckless);
                showImage.sprite = itemImages[5]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
                 
    
            }
            
            else if (roll < 71 && roll > 50)
        {
                            int randomItem = Random.Range(0, 3); 

            if (randomItem == 0)
                {
                    EquipmentManager.Instance.UnlockItem(KnightSword);
                    showImage.sprite = itemImages[6]; 
                    showImage.gameObject.SetActive(true);
                    Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
                }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(KnightArmor);
                showImage.sprite = itemImages[7]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(DawnRing);
                showImage.sprite = itemImages[8]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
        }

        else if (roll > 99)
        {
                            int randomItem = Random.Range(0, 6); 

            if (randomItem == 0)
                {
                    EquipmentManager.Instance.UnlockItem(LightSpear);
                    showImage.sprite = itemImages[9]; 
                    showImage.gameObject.SetActive(true);
                    Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
                }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(FireLegendarySword);
                showImage.sprite = itemImages[10]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(WaterLegendaryArmor);
                showImage.sprite = itemImages[11]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 3){
                EquipmentManager.Instance.UnlockItem(WindSpear);
                showImage.sprite = itemImages[12]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 4){
                EquipmentManager.Instance.UnlockItem(EarthLegendaryArmor);
                showImage.sprite = itemImages[13]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
             else if(randomItem == 5){
                EquipmentManager.Instance.UnlockItem(DarkLegendaryRing);
                showImage.sprite = itemImages[14]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }

            else
            {
                showImage.sprite = itemImages[15]; 
                 showImage.gameObject.SetActive(true);
                 Debug.Log("ผู้เล่นไม่ได้ไอเท็ม");
                
            }
        }

          
        
        

        // 4. สั่งให้กล่องเปิด! (พร้อมส่ง Callback ว่าถ้าเสร็จแล้วให้ทำอะไร)
        boxScript.OpenBox(resultSprite, () =>
        {
            // สิ่งที่จะทำเมื่อกล่องเปิดเสร็จแล้ว (3 วิหลังจากโชว์ของ)
            panel.SetActive(false); // ปิดหน้าต่าง
            GameTurnManager.Instance.RequestEndTurn(); // จบเทิร์น
        });
    }

    private void TrapEffect(GameObject target)
    {
        target.GetComponent<PlayerState>()?.TakeDamage(15);
        ShowPanel("trappanel", true);
    }

    private void StarGain(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p != null) p.PlayerStar += 15;
        ShowPanel("starpanel", true);
    }

    private void DropStarEffect(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p == null) return;
        p.PlayerStar = Mathf.Max(0, p.PlayerStar - Random.Range(5, 11));
        ShowPanel("droppanel", true);
    }

    private void RandomWarp(GameObject target)
    {
        PlayerPathWalker walker = target.GetComponent<PlayerPathWalker>();
        if (walker != null && RouteManager.Instance != null)
        {
            var nodes = RouteManager.Instance.nodeConnections.FindAll(x => x.node != null && x.tileID != walker.currentNodeID);
            if (nodes.Count > 0) walker.TeleportToNode(nodes[Random.Range(0, nodes.Count)].node);
        }
        ShowPanel("warppanel", true);
    }

    private void Heal(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p != null) p.PlayerHealth += 10;
        ShowPanel("heal", true);
    }
    private void Draw(GameObject target)
    {
        PlayerState p = target.GetComponent<PlayerState>();
        if (p != null) p.PlayerHealth += 10;
        ShowPanel("DrawCard", true);
    }
    public void TriggerRandomEvent(GameObject target) { currentEventTarget = target; StartCoroutine(RandomEventCoroutine()); }
    public void TriggerMinigameEvent(GameObject target) { currentEventTarget = target; StartCoroutine(RandomMinigameEventCoroutine()); }

    // ในไฟล์ GameEventManager.cs

    private IEnumerator RandomEventCoroutine()
    {
        isRandomSpinning = true;

        // 1. 🎯 ล็อกผลล่วงหน้า (สุ่ม Index ไว้ก่อนเลย)
        // เงื่อนไข: จำนวน Panel (รูปในวงล้อ) กับ Keys (ชื่อ Event) ควรมีจำนวนเท่ากันและเรียงตรงกัน
        // ถ้าไม่เท่ากัน เราจะสุ่ม Key แยกต่างหาก
        int resultIndex = Random.Range(0, randomEventKeys.Length);
        string selectedKey = randomEventKeys[resultIndex];

        // ตรวจสอบความปลอดภัย: ถ้ามีรูปวงล้อ ให้พยายามหยุดที่รูปที่ตรงกับ Index
        int finalPanelIndex = -1;
        if (randomEventPanels.Length > 0)
        {
            // ถ้าจำนวนรูป = จำนวน Event -> หยุดที่รูปนั้นเลย
            if (randomEventPanels.Length == randomEventKeys.Length)
            {
                finalPanelIndex = resultIndex;
            }
            else
            {
                // ถ้าไม่เท่ากัน ก็สุ่มรูปที่จะหยุดมั่วๆ ไปก่อน
                finalPanelIndex = Random.Range(0, randomEventPanels.Length);
            }
        }

        // 2. 🌀 เริ่มหมุน! (แบบช้าลงเรื่อยๆ)
        float currentInterval = 0.05f; // เริ่มต้นหมุนเร็วมาก (0.05วิ)
        float slowDownFactor = 1.1f;   // ช้าลงทีละ 10%
        int totalSpins = 20;           // จำนวนครั้งที่จะสลับภาพก่อนหยุด (ปรับได้)

        int currentPanelIdx = 0;

        for (int i = 0; i < totalSpins; i++)
        {
            // สลับรูป
            if (randomEventPanels.Length > 0)
            {
                // ถ้าเป็นรอบสุดท้าย ต้องบังคับให้ไปตกที่ finalPanelIndex
                if (i == totalSpins - 1 && finalPanelIndex != -1)
                {
                    currentPanelIdx = finalPanelIndex;
                }
                else
                {
                    // รอบปกติ: ขยับไปรูปถัดไปเรื่อยๆ (Loop)
                    currentPanelIdx = (currentPanelIdx + 1) % randomEventPanels.Length;
                }

                // แสดงผล
                for (int p = 0; p < randomEventPanels.Length; p++)
                {
                    randomEventPanels[p]?.SetActive(p == currentPanelIdx);
                }
            }

            // รอเวลา (ยิ่งรอบหลังๆ currentInterval จะยิ่งเยอะขึ้น = ช้าลง)
            yield return new WaitForSeconds(currentInterval);

            // เพิ่มเวลาหน่วงสำหรับรอบถัดไป (แต่ไม่ให้ช้าเกิน 0.5 วิ)
            currentInterval = Mathf.Min(currentInterval * slowDownFactor, 0.5f);
        }

        // 3. ✋ หยุดค้างไว้ (Hold) เพื่อให้คนดูรู้ว่า "ได้อันนี้แหละ!"
        Debug.Log($"[EventManager] หยุดที่: {selectedKey} (รอ 3 วินาที)");
        yield return new WaitForSeconds(3f); // ค้างที่ผลลัพธ์ประมาณ 3 วินาที

        // 4. 🧹 ซ่อนวงล้อ
        foreach (var p in randomEventPanels) p?.SetActive(false);

        // 5. 🎉 ส่ง Event จริง
        TriggerEvent(selectedKey, currentEventTarget);

        // (หมายเหตุ: isRandomSpinning จะถูกเคลียร์ใน TriggerEvent > ShowPanel หรือ ResetEventStatus)
        isRandomSpinning = false;
    }

    private IEnumerator RandomMinigameEventCoroutine()
    {
        isRandomSpinning = true;
        float elapsed = 0f;
        while (elapsed < randomSpinDuration)
        {
            int index = Random.Range(0, randomMinigameEventPanels.Length);
            for (int i = 0; i < randomMinigameEventPanels.Length; i++) randomMinigameEventPanels[i]?.SetActive(i == index);
            elapsed += spinInterval;
            yield return new WaitForSeconds(spinInterval);
        }
        foreach (var p in randomMinigameEventPanels) p?.SetActive(false);

        if (randomMinigameEventKeys.Length > 0)
        {
            string selected = randomMinigameEventKeys[Random.Range(0, randomMinigameEventKeys.Length)];
            TriggerEvent(selected, currentEventTarget);
        }
        else { isRandomSpinning = false; GameTurnManager.Instance.RequestEndTurn(); }
        isRandomSpinning = false;
    }

    private IEnumerator MonsterBattleCoroutine()
    {
        RememberCurrentBoardScene();
        ShowPanel("monster", false);
        yield return new WaitForSeconds(1f);
        string[] Scenes = { "fightDarkNormal", "fightEarthNormal", "fightLightNormal", "fightWaterNormal", "fightWindNormal", "TestFight" };
        int randomIndex = Random.Range(0, Scenes.Length);
        SceneManager.LoadScene(Scenes[randomIndex]);
    }

    public int countroundbattle = 0; // ตัวนับรอบที่ตีกับศัตรู
    public int countbattle = 0; // ตัวนับการเจอศัตรู

    public void ResetForNewBoardSession()
    {
        currentEventTarget = null;
        countbattle = 0;
        countroundbattle = 0;
        ResetEventStatus();
    }

    public void AddCount1(int amount)
    {
        countroundbattle += amount;
        Debug.Log("Count 1 ตอนนี้คือ: " + countroundbattle);
    }

    public void AddCount2(int amount)
    {
        countbattle += amount;
        Debug.Log("Count 2 ตอนนี้คือ: " + countbattle);
    }

    private IEnumerator BossBattleCoroutine()
    {
       RememberCurrentBoardScene();
       ShowPanel("boss", false);
    yield return new WaitForSeconds(1f);
    int bosslevel = 0;

    if(countbattle > 0){
     bosslevel = countroundbattle/countbattle ;
    }
    // 1. ดึงชื่อ Scene ปัจจุบันออกมาเช็ค
    string currentSceneName = SceneManager.GetActiveScene().name;


    // 2. สร้างเงื่อนไข (if-else)

    if (currentSceneName == "MainLight") 
    {

        if(bosslevel < 11)
            {
        SceneManager.LoadScene("FinalBoss hard"); 
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("FianlBoss medium");
            }
         else 
            {
                SceneManager.LoadScene("FinalBoss");
            }

        
    }

   else if (currentSceneName == "TestMain") 
    {

        if(bosslevel < 11)
            {
        SceneManager.LoadScene("bossfire hard"); 
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("bossfire medium");
            }
         else 
            {
                SceneManager.LoadScene("bossfire");
            }
    }

    
    else if (currentSceneName == "MainWater") 
    {
        if(bosslevel < 11)
            {
        SceneManager.LoadScene("boss water hard"); 
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("boss water medium");
            }
         else 
            {
                SceneManager.LoadScene("boss water");
            }
    }
    
    else if (currentSceneName == "MainWind")
    {
          if(bosslevel < 11)
            {
        SceneManager.LoadScene("boss wind hard"); 
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("boss wind medium");
            }
         else 
            {
                SceneManager.LoadScene("boss wind");
            }
    }
        

     else if (currentSceneName == "MainEarth")
    {
          if(bosslevel < 11)
            {
        SceneManager.LoadScene("boss earth hard"); 
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("boss earth medium");
            }
         else 
            {
                SceneManager.LoadScene("boss earth");
            }
    }
         

     else if (currentSceneName == "MainDark")
    {
          if(bosslevel < 11)
            {
        SceneManager.LoadScene("boss dark hard");
            }
        else if(bosslevel >=11  && bosslevel <= 15)
            {
                SceneManager.LoadScene("boss dark medium");
            }
         else 
            {
                SceneManager.LoadScene("boss dark");
            }

    }

countbattle = 0;
countroundbattle = 0;

    }

    private IEnumerator SpecialBossBattleCoroutine()
    {
         RememberCurrentBoardScene();
         string currentSceneName = SceneManager.GetActiveScene().name;
        ShowPanel("specialboss", false);
    yield return new WaitForSeconds(1f);

    if(currentSceneName == "MainLight")
        {
            SceneManager.LoadScene("SpecialBoss"); 
        }
           
        else if (currentSceneName == "TestMain") 
    {
        SceneManager.LoadScene("specialbossfire"); 
            
    }
    else if (currentSceneName == "MainWater") 
    {
        SceneManager.LoadScene("Special boss water"); 
            
    }
     else if (currentSceneName == "MainWind") 
    {
        SceneManager.LoadScene("special boss wind"); 
            
    }
     else if (currentSceneName == "MainEarth") 
    {
        SceneManager.LoadScene("specialboss earth"); 
            
    }
     else if (currentSceneName == "MainDark") 
    {
        SceneManager.LoadScene("special boss dark"); 
            
    }
    }

    private void ShowPanel(string panelKey, bool autoClose)
    {
        foreach (var p in eventPanels.Values) p?.SetActive(false);
        if (eventPanels.TryGetValue(panelKey.ToLower(), out var panel))
        {
            panel.SetActive(true);
            if (autoClose) StartCoroutine(HidePanelAfterDelay(panel));
        }
        else if (autoClose) StartCoroutine(WaitAndEndTurn());
    }

    private IEnumerator HidePanelAfterDelay(GameObject panel)
    {
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
        // ✅ จบ Event แล้วเรียก RequestEndTurn
        GameTurnManager.Instance.RequestEndTurn();
    }

    private IEnumerator WaitAndEndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        GameTurnManager.Instance.RequestEndTurn();
    }

    public void ForceResetEventStatus()
    {
        StopAllCoroutines(); // หยุด Coroutine การสุ่มที่อาจค้างอยู่
        isRandomSpinning = false; // ✅ ปลดล็อคตัวแปร
        Debug.Log("<color=orange>[EventManager] 🧹 ล้างสถานะ Event ค้างเรียบร้อย</color>");
    }
    public void ResetEventStatus()
    {
        StopAllCoroutines();
        isRandomSpinning = false; // ✅ ปลดล็อคกำแพงตัวที่ 1
        Debug.Log("<color=orange>[EventManager] 🧹 ล้างสถานะสุ่มค้างเรียบร้อย</color>");
    }

    private void RememberCurrentBoardScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentSceneName)) return;

        boardGameSceneName = currentSceneName;
        PlayerPrefs.SetString(LastBoardSceneKey, currentSceneName);
        PlayerPrefs.Save();

        Debug.Log($"[EventManager] จำฉากบอร์ดล่าสุดเป็น '{currentSceneName}'");
    }
    #endregion
}
