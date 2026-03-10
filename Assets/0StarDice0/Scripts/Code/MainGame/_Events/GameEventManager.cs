﻿using UnityEngine;
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

    public Sprite creditSprite; // 🖼️ ลากรูปเหรียญมาใส่ตรงนี้
    
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

    private static void SafeSetActive(GameObject target, bool active)
    {
        if (target == null)
        {
            return;
        }

        try
        {
            target.SetActive(active);
        }
        catch (MissingReferenceException)
        {
            // object ถูก Destroy ระหว่างเฟรม
        }
        catch (System.NullReferenceException)
        {
            // safety เพิ่มเติมสำหรับ object ที่หายระหว่าง native/managed bridge
        }
    }

    private static bool TryGetComponentInChildrenSafe<T>(GameObject target, out T component) where T : Component
    {
        component = null;
        if (target == null)
        {
            return false;
        }

        try
        {
            component = target.GetComponentInChildren<T>(true);
            return component != null;
        }
        catch (MissingReferenceException)
        {
            return false;
        }
        catch (System.NullReferenceException)
        {
            return false;
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
            SafeSetActive(child.gameObject, false);
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
                SafeSetActive(randomEventPanels[i], false);
            }
        }

        GameObject minigameContainer = GameObject.Find("RandomMinigamePanelsContainer");
        if (minigameContainer != null)
        {
            randomMinigameEventPanels = new GameObject[minigameContainer.transform.childCount];
            for (int i = 0; i < minigameContainer.transform.childCount; i++)
            {
                randomMinigameEventPanels[i] = minigameContainer.transform.GetChild(i).gameObject;
                SafeSetActive(randomMinigameEventPanels[i], false);
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
        // ✅ ล็อค State ทันทีที่กิด Event
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
                ShopManager shopManager = FindObjectOfType<ShopManager>();
                if (shopManager != null)
                {
                    shopManager.HandleShopOpened();
                }
                else if (shopPanel != null)
                {
                    SafeSetActive(shopPanel, true);
                }
                else
                {
                    GameTurnManager.Instance?.RequestEndTurn();
                }
                break;
            case "draw": Draw(target); break;
            case "lava": LavaEffect(target); break;
            case "move": RandomMoveEffect(target); break;
            case "windteleport": WindTeleportEffect(target); break;
            case "iceeffect": ApplyIceEffect(target); break;
            case "minigamefappy":
            case "level 1":
            case "minigamespotmemory":
            case "minigamemath":
                StartCoroutine(LoadMinigameSceneCoroutine(eventName));
                break;
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

        SafeSetActive(panel, true);

        // 2. หาตัว BoxOpener ใน Panel นั้น
        if (!TryGetComponentInChildrenSafe(panel, out BoxOpener boxScript))
        {
            Debug.LogError("ใน Panel ไม่มีสคริปต์ BoxOpener หรือ panel ถูกทำลายระหว่างทาง!");
            StartCoroutine(WaitAndEndTurn());
            return;
        }

        // 3. คำนวณรางวัล และเลือกรูป
        Sprite resultSprite = null;
        p.PlayerCredit += 100;
        resultSprite = creditSprite;
        Debug.Log("Treasure: Got Credit");

        int roll = Random.Range(1, 101);
        if (roll < 51)
        {
            int randomItem = Random.Range(0, 6); 

            if (randomItem == 0)
            {
                EquipmentManager.Instance.UnlockItem(Sword);
                showImage.sprite = itemImages[0]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(Armor);
                showImage.sprite = itemImages[1]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(DawnRing);
                showImage.sprite = itemImages[2]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 3){
                EquipmentManager.Instance.UnlockItem(WhiteFeather);
                showImage.sprite = itemImages[3]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 4){
                EquipmentManager.Instance.UnlockItem(RecoverRing);
                showImage.sprite = itemImages[4]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 5){
                EquipmentManager.Instance.UnlockItem(HearthNeckless);
                showImage.sprite = itemImages[5]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
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
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(KnightArmor);
                showImage.sprite = itemImages[7]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(DawnRing);
                showImage.sprite = itemImages[8]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
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
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 1){
                EquipmentManager.Instance.UnlockItem(FireLegendarySword);
                showImage.sprite = itemImages[10]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 2){
                EquipmentManager.Instance.UnlockItem(WaterLegendaryArmor);
                showImage.sprite = itemImages[11]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 3){
                EquipmentManager.Instance.UnlockItem(WindSpear);
                showImage.sprite = itemImages[12]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 4){
                EquipmentManager.Instance.UnlockItem(EarthLegendaryArmor);
                showImage.sprite = itemImages[13]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else if(randomItem == 5){
                EquipmentManager.Instance.UnlockItem(DarkLegendaryRing);
                showImage.sprite = itemImages[14]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว!");
            }
            else
            {
                showImage.sprite = itemImages[15]; 
                SafeSetActive(showImage != null ? showImage.gameObject : null, true);
                Debug.Log("ผู้เล่นไม่ได้ไอเท็ม");
            }
        }

        boxScript.OpenBox(resultSprite, () =>
        {
            SafeSetActive(panel, false); // ปิดหน้าต่าง
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

    private void LavaEffect(GameObject target)
    {
        PlayerState player = target.GetComponent<PlayerState>();
        if (player != null)
        {
            player.TakeDamage(25);
            player.ApplyBurnDebuff(3);
            Debug.Log($"<color=orange>🔥 {target.name} ติดสถานะ Burn 3 เทิร์น</color>");
        }

        ShowPanel("lavapanel", true);
    }

    private void RandomMoveEffect(GameObject target)
    {
        if (target == null)
        {
            GameTurnManager.Instance?.RequestEndTurn();
            return;
        }

        PlayerPathWalker walker = target.GetComponent<PlayerPathWalker>();
        if (walker == null)
        {
            Debug.LogWarning("[EventManager] Random move event หา PlayerPathWalker ไม่เจอ -> จบเทิร์น");
            GameTurnManager.Instance?.RequestEndTurn();
            return;
        }

        int randomSteps = Random.Range(1, 7);
        Debug.Log($"[EventManager] Random move event: {target.name} เดินเพิ่ม {randomSteps} ช่อง");
        walker.ExecuteMove(randomSteps);
    }

    public void TriggerRandomEvent(GameObject target)
    {
        currentEventTarget = target;
        if (!CanSpinRandomEvent(randomEventKeys, "RandomEvent"))
        {
            GameTurnManager.Instance?.RequestEndTurn();
            return;
        }

        StartCoroutine(RandomEventCoroutine());
    }

    public void TriggerMinigameEvent(GameObject target)
    {
        currentEventTarget = target;
        if (!CanSpinRandomEvent(randomMinigameEventKeys, "RandomMinigame"))
        {
            GameTurnManager.Instance?.RequestEndTurn();
            return;
        }

        StartCoroutine(RandomMinigameEventCoroutine());
    }

    private bool CanSpinRandomEvent(string[] eventKeys, string source)
    {
        if (eventKeys == null || eventKeys.Length == 0)
        {
            Debug.LogWarning($"[EventManager] {source} keys ว่าง -> จบเทิร์นเพื่อกันเกมค้าง");
            return false;
        }

        return true;
    }

    private static bool HasAnyValidPanels(GameObject[] panels)
    {
        if (panels == null || panels.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator RandomEventCoroutine()
    {
        isRandomSpinning = true;

        try
        {
            int resultIndex = Random.Range(0, randomEventKeys.Length);
            string selectedKey = randomEventKeys[resultIndex];

            if (string.IsNullOrEmpty(selectedKey))
            {
                Debug.LogWarning("[EventManager] RandomEvent ได้ key ว่าง -> จบเทิร์น");
                GameTurnManager.Instance?.RequestEndTurn();
                yield break;
            }

            if (!HasAnyValidPanels(randomEventPanels))
            {
                Debug.LogWarning("[EventManager] RandomEvent ไม่มี panel ที่ใช้งานได้ -> ข้ามแอนิเมชันแล้วยิง event ทันที");
                yield return null;
                TriggerEvent(selectedKey, currentEventTarget);
                yield break;
            }

            int finalPanelIndex = -1;
            if (randomEventPanels.Length == randomEventKeys.Length)
            {
                finalPanelIndex = resultIndex;
            }
            else
            {
                finalPanelIndex = Random.Range(0, randomEventPanels.Length);
                Debug.LogWarning("[EventManager] randomEventPanels กับ randomEventKeys จำนวนไม่เท่ากัน -> ใช้การสุ่มรูปแยก");
            }

            float currentInterval = 0.05f;
            float slowDownFactor = 1.1f;
            int totalSpins = 20;
            int currentPanelIdx = 0;

            for (int i = 0; i < totalSpins; i++)
            {
                if (randomEventPanels != null && randomEventPanels.Length > 0)
                {
                    if (i == totalSpins - 1 && finalPanelIndex != -1)
                    {
                        currentPanelIdx = finalPanelIndex;
                    }
                    else
                    {
                        currentPanelIdx = (currentPanelIdx + 1) % randomEventPanels.Length;
                    }

                    for (int p = 0; p < randomEventPanels.Length; p++)
                    {
                        SafeSetActive(randomEventPanels[p], p == currentPanelIdx);
                    }
                }

                yield return new WaitForSeconds(currentInterval);
                currentInterval = Mathf.Min(currentInterval * slowDownFactor, 0.5f);
            }

            Debug.Log($"[EventManager] หยุดที่: {selectedKey} (รอ 3 วินาที)");
            yield return new WaitForSeconds(3f);

            if (randomEventPanels != null)
            {
                foreach (var panel in randomEventPanels)
                {
                    SafeSetActive(panel, false);
                }
            }

            TriggerEvent(selectedKey, currentEventTarget);
        }
        finally
        {
            isRandomSpinning = false;
        }
    }

    private IEnumerator RandomMinigameEventCoroutine()
    {
        isRandomSpinning = true;

        try
        {
            float elapsed = 0f;

            while (elapsed < randomSpinDuration)
            {
                if (HasAnyValidPanels(randomMinigameEventPanels))
                {
                    int index = Random.Range(0, randomMinigameEventPanels.Length);
                    for (int i = 0; i < randomMinigameEventPanels.Length; i++)
                    {
                        SafeSetActive(randomMinigameEventPanels[i], i == index);
                    }
                }

                elapsed += spinInterval;
                yield return new WaitForSeconds(spinInterval);
            }

            if (randomMinigameEventPanels != null)
            {
                foreach (var panel in randomMinigameEventPanels)
                {
                    SafeSetActive(panel, false);
                }
            }

            string selected = randomMinigameEventKeys[Random.Range(0, randomMinigameEventKeys.Length)];
            if (string.IsNullOrEmpty(selected))
            {
                Debug.LogWarning("[EventManager] RandomMinigame ได้ key ว่าง -> จบเทิร์น");
                GameTurnManager.Instance?.RequestEndTurn();
                yield break;
            }

            TriggerEvent(selected, currentEventTarget);
        }
        finally
        {
            isRandomSpinning = false;
        }
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

    private IEnumerator LoadMinigameSceneCoroutine(string minigameKey)
    {
        string sceneName = ResolveMinigameSceneName(minigameKey);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[EventManager] ไม่พบ Scene ของ minigame key '{minigameKey}' -> จบเทิร์น");
            GameTurnManager.Instance?.RequestEndTurn();
            yield break;
        }

        RememberCurrentBoardScene();
        yield return null;
        SceneManager.LoadScene(sceneName);
    }

    private string ResolveMinigameSceneName(string minigameKey)
    {
        if (string.IsNullOrWhiteSpace(minigameKey)) return null;

        switch (minigameKey.Trim().ToLower())
        {
            case "minigamefappy": return "MiniGameFappy";
            case "level 1": return "Level 1";
            case "minigamespotmemory": return "MiniGameSpotMemory";
            case "minigamemath": return "MiniGameMath";
            default: return null;
        }
    }

    public int countroundbattle = 0;
    public int countbattle = 0;

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

       string currentSceneName = SceneManager.GetActiveScene().name;

       if (currentSceneName == "MainLight") 
       {
           if(bosslevel < 11) SceneManager.LoadScene("FinalBoss hard"); 
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("FianlBoss medium");
           else SceneManager.LoadScene("FinalBoss");
       }
       else if (currentSceneName == "TestMain") 
       {
           if(bosslevel < 11) SceneManager.LoadScene("bossfire hard"); 
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("bossfire medium");
           else SceneManager.LoadScene("bossfire");
       }
       else if (currentSceneName == "MainWater") 
       {
           if(bosslevel < 11) SceneManager.LoadScene("boss water hard"); 
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("boss water medium");
           else SceneManager.LoadScene("boss water");
       }
       else if (currentSceneName == "MainWind")
       {
           if(bosslevel < 11) SceneManager.LoadScene("boss wind hard"); 
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("boss wind medium");
           else SceneManager.LoadScene("boss wind");
       }
       else if (currentSceneName == "MainEarth")
       {
           if(bosslevel < 11) SceneManager.LoadScene("boss earth hard"); 
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("boss earth medium");
           else SceneManager.LoadScene("boss earth");
       }
       else if (currentSceneName == "MainDark")
       {
           if(bosslevel < 11) SceneManager.LoadScene("boss dark hard");
           else if(bosslevel >=11  && bosslevel <= 15) SceneManager.LoadScene("boss dark medium");
           else SceneManager.LoadScene("boss dark");
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

         if(currentSceneName == "MainLight") SceneManager.LoadScene("SpecialBoss"); 
         else if (currentSceneName == "TestMain") SceneManager.LoadScene("specialbossfire"); 
         else if (currentSceneName == "MainWater") SceneManager.LoadScene("Special boss water"); 
         else if (currentSceneName == "MainWind") SceneManager.LoadScene("special boss wind"); 
         else if (currentSceneName == "MainEarth") SceneManager.LoadScene("specialboss earth"); 
         else if (currentSceneName == "MainDark") SceneManager.LoadScene("special boss dark"); 
    }

    private void ShowPanel(string panelKey, bool autoClose)
    {
        if (string.IsNullOrEmpty(panelKey))
        {
            if (autoClose)
            {
                StartCoroutine(WaitAndEndTurn());
            }

            return;
        }

        CleanupMissingEventPanelReferences();

        foreach (var panelEntry in eventPanels)
        {
            GameObject panelObject = panelEntry.Value;
            if (panelObject != null)
            {
                SafeSetActive(panelObject, false);
            }
        }

        if (eventPanels.TryGetValue(panelKey.ToLower(), out var panel) && panel != null)
        {
            SafeSetActive(panel, true);
            if (autoClose)
            {
                StartCoroutine(HidePanelAfterDelay(panel));
            }
        }
        else if (autoClose)
        {
            StartCoroutine(WaitAndEndTurn());
        }
    }

    private void CleanupMissingEventPanelReferences()
    {
        if (eventPanels == null || eventPanels.Count == 0)
        {
            return;
        }

        List<string> keysToRemove = null;
        foreach (var panelEntry in eventPanels)
        {
            if (panelEntry.Value == null)
            {
                if (keysToRemove == null)
                {
                    keysToRemove = new List<string>();
                }

                keysToRemove.Add(panelEntry.Key);
            }
        }

        if (keysToRemove == null)
        {
            return;
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            eventPanels.Remove(keysToRemove[i]);
        }
    }

    private IEnumerator HidePanelAfterDelay(GameObject panel)
    {
        yield return new WaitForSeconds(2f);

        if (panel != null)
        {
            SafeSetActive(panel, false);
        }

        GameTurnManager.Instance?.RequestEndTurn();
    }

    private IEnumerator WaitAndEndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        GameTurnManager.Instance?.RequestEndTurn();
    }

    public void ForceResetEventStatus()
    {
        StopAllCoroutines();
        isRandomSpinning = false;
        Debug.Log("<color=orange>[EventManager] 🧹 ล้างสถานะ Event ค้างเรียบร้อย</color>");
    }
    public void ResetEventStatus()
    {
        StopAllCoroutines();
        isRandomSpinning = false;
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
