﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    private static GameEventManager cachedManager;

    public static bool TryGet(out GameEventManager manager)
    {
        if (cachedManager != null)
        {
            manager = cachedManager;
            return true;
        }

        manager = FindFirstObjectByType<GameEventManager>();
        if (manager != null)
        {
            cachedManager = manager;
        }

        return manager != null;
    }

    public static void TryAddCount1(int amount)
    {
        if (TryGet(out var manager))
            manager.AddCount1(amount);
    }

    public static void TryAddCount2(int amount)
    {
        if (TryGet(out var manager))
            manager.AddCount2(amount);
    }

    public static void TryTriggerEvent(string eventName, GameObject target)
    {
        if (TryGet(out var manager))
            manager.TriggerEvent(eventName, target);
    }

    public static void SetRandomSpinning(bool value)
    {
        if (TryGet(out var manager))
            manager.isRandomSpinning = value;
    }

    public static bool TryLoadBattleSceneAdditive(string battleSceneName)
    {
        if (!TryGet(out var manager) || string.IsNullOrWhiteSpace(battleSceneName))
        {
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(battleSceneName))
        {
            Debug.LogError($"[EventManager] Cannot load battle scene '{battleSceneName}'. Check Build Profiles.");
            return false;
        }

        manager.StartCoroutine(manager.LoadBattleSceneAdditiveCoroutine(battleSceneName));
        return true;
    }

    public const string LastBoardSceneKey = "LastBoardSceneName";

    [Header("Scene Settings")]
    public string boardGameSceneName = "TestMain";

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
    [Header("References (Refactor Prep)")]
    [SerializeField] private GameTurnManager gameTurnManager;
    [SerializeField] private RouteManager routeManager;
    private bool isFirstLoad = true;
    private bool hasStartedGame = false;
    public GameObject shopPanel;
    private GameObject currentEventTarget;
    private readonly List<GameObject> hiddenBoardSceneRoots = new List<GameObject>();
    private string hiddenBoardSceneName;
    private Material boardSkyboxBeforeBattle;
    private bool hasBoardSkyboxBeforeBattle;

    public int[] windTeleportTargetIDs;
    public string windTeleportPanelName = "windteleportpanel";
    // ✅ ตัวแปรเช็คสถานะการเล่น (เชื่อมกับ State Machine)

    public Sprite creditSprite; // 🖼️ ลากรูปเหรียญมาใส่ตรงนี้

    public bool isEventProcessing => isRandomSpinning || (ResolveGameTurnManager() != null && ResolveGameTurnManager().currentState == GameState.EventProcessing);

    private GameTurnManager ResolveGameTurnManager()
    {
        if (gameTurnManager == null)
            gameTurnManager = FindFirstObjectByType<GameTurnManager>();

        return gameTurnManager;
    }

    private RouteManager ResolveRouteManager()
    {
        if (routeManager == null)
            routeManager = FindFirstObjectByType<RouteManager>();

        return routeManager;
    }

    #region Unity Lifecycle & Scene Management

    private void Awake()
    {
        GameEventManager[] managers = FindObjectsByType<GameEventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers.Length > 1) { Destroy(gameObject); return; }

        cachedManager = this;
    }

    private void OnDestroy()
    {
        if (cachedManager == this)
            cachedManager = null;
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

    public static void NotifyBoardSceneReady(string sceneName)
    {
        int listenerCount = OnBoardSceneReady?.GetInvocationList().Length ?? 0;
        Debug.Log($"[EventManager] Board scene ready: '{sceneName}' (listeners: {listenerCount})");
        OnBoardSceneReady?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsBoardScene(scene))
        {
            Debug.Log($"<color=cyan>[EventManager] ซีน '{scene.name}' ถูกระบุเป็น Board Game</color>");

            SetupReferences();
            RestoreHiddenBoardSceneRoots();

            if (isFirstLoad)
            {
                isFirstLoad = false;
                return;
            }

            // ล้างค่าสุ่มค้าง
            isRandomSpinning = false;

            // ตะโกนบอก Manager ว่าพร้อมแล้ว
            NotifyBoardSceneReady(scene.name);
        }
        else
        {
            Debug.Log($"[EventManager] ซีน '{scene.name}' ไม่ใช่ Board -> คงสถานะซ่อน Board ไว้");
        }
    }

    private bool IsBoardScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(scene.name) && scene.name == boardGameSceneName)
        {
            return true;
        }

        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null)
            {
                continue;
            }

            RouteManager route = roots[i].GetComponentInChildren<RouteManager>(true);
            if (route != null && route.nodeConnections != null && route.nodeConnections.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void SetupReferences()
    {
        FindAndSetupMainPanels();
        FindAndSetupRandomEventPanels();
        FindAndSetupChoiceUI();

        RouteManager newMap = ResolveRouteManager() ?? FindObjectOfType<RouteManager>();
        if (newMap != null) routeManager = newMap;

        if (ResolveGameTurnManager() != null && newMap != null)
        {
            foreach (var playerState in ResolveGameTurnManager().allPlayers)
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
        if (foundUI != null && ResolveGameTurnManager() != null)
        {
            foreach (var p in ResolveGameTurnManager().allPlayers)
                p?.GetComponent<PlayerPathWalker>()?.SetChoiceUIManager(foundUI);
        }
    }

    #endregion

    #region Main Event Logic

    public void TriggerEvent(string eventName, GameObject player)
    {
        // ✅ ล็อค State ทันทีที่กิด Event
        if (ResolveGameTurnManager() != null) ResolveGameTurnManager().SetState(GameState.EventProcessing);

        GameObject target = player != null ? player : currentEventTarget;
        if (target == null) target = GameTurnManager.CurrentPlayer?.gameObject;
        if (target == null) { ResolveGameTurnManager()?.RequestEndTurn(); return; }

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
                    ResolveGameTurnManager()?.RequestEndTurn();
                }
                break;
            case "draw": Draw(target); break;
            case "lava": LavaEffect(target); break;
            case "move": RandomMoveEffect(target); break;
            case "windteleport": WindTeleportEffect(target); break;
            case "mainlighthealgimmick": TriggerMainLightHealGimmickEvent(); break;
            case "iceeffect": ApplyIceEffect(target); break;
            case "minigamefappy":
            case "level 1":
            case "minigamespotmemory":
            case "minigamemath":
                StartCoroutine(LoadMinigameSceneCoroutine(eventName));
                break;
            default:
                if (eventPanels.ContainsKey(eventName.ToLower())) ShowPanel(eventName, true);
                else ResolveGameTurnManager()?.RequestEndTurn();
                break;
        }
    }


    private void TriggerMainLightHealGimmickEvent()
    {
        RouteManager routeManager = ResolveRouteManager();
        if (routeManager != null && routeManager.TriggerMainLightHealGimmick())
        {
            ResolveGameTurnManager()?.RequestEndTurn();
            return;
        }

        Debug.LogWarning("[GameEventManager] Trigger mainlighthealgimmick ไม่สำเร็จ");
        ResolveGameTurnManager()?.RequestEndTurn();
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
            ResolveGameTurnManager()?.RequestEndTurn(); // จบเทิร์นกันเกมค้าง
            return;
        }

        PlayerPathWalker walker = target.GetComponent<PlayerPathWalker>();

        if (walker != null && ResolveRouteManager() != null)
        {
            // 2. ✨ [สุ่ม] เลือก ID หนึ่งตัวจากรายการที่ใส่ไว้
            int randomIndex = Random.Range(0, windTeleportTargetIDs.Length);
            int chosenID = windTeleportTargetIDs[randomIndex];

            Debug.Log($"<color=cyan>WindTeleport: สุ่มได้ Node ID {chosenID} (จากทั้งหมด {windTeleportTargetIDs.Length} จุด)</color>");

            // 3. ค้นหา Node จาก ID ที่สุ่มได้
            var targetConnection = ResolveRouteManager().nodeConnections.Find(x => x.tileID == chosenID);

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

    if (!eventPanels.TryGetValue("openchestpanel", out var panel))
    {
        Debug.LogError("หา Panel 'openchestpanel' ไม่เจอ!");
        ResolveGameTurnManager().RequestEndTurn();
        return;
    }

    SafeSetActive(panel, true);

    if (!TryGetComponentInChildrenSafe(panel, out BoxOpener boxScript))
    {
        Debug.LogError("ใน Panel ไม่มีสคริปต์ BoxOpener หรือ panel ถูกทำลายระหว่างทาง!");
        StartCoroutine(WaitAndEndTurn());
        return;
    }

    p.PlayerCredit += 100;
    Sprite resultSprite = creditSprite;
    Debug.Log("Treasure: Got Credit");

    Sprite targetItemSprite = null; // ตัวแปรสำหรับเก็บรูปไอเท็มที่จะโชว์

    int roll = Random.Range(1, 101); // สุ่ม 1-100

    if (roll <= 50)
    {
        int randomItem = Random.Range(0, 6);
        if (randomItem == 0) { EquipmentManager.Instance.UnlockItem(Sword); targetItemSprite = itemImages[0]; }
        else if (randomItem == 1) { EquipmentManager.Instance.UnlockItem(Armor); targetItemSprite = itemImages[1]; }
        else if (randomItem == 2) { EquipmentManager.Instance.UnlockItem(DawnRing); targetItemSprite = itemImages[2]; }
        else if (randomItem == 3) { EquipmentManager.Instance.UnlockItem(WhiteFeather); targetItemSprite = itemImages[3]; }
        else if (randomItem == 4) { EquipmentManager.Instance.UnlockItem(RecoverRing); targetItemSprite = itemImages[4]; }
        else if (randomItem == 5) { EquipmentManager.Instance.UnlockItem(HearthNeckless); targetItemSprite = itemImages[5]; }
    }
    else if (roll > 50 && roll <= 70)
    {
        int randomItem = Random.Range(0, 3);
        if (randomItem == 0) { EquipmentManager.Instance.UnlockItem(KnightSword); targetItemSprite = itemImages[6]; }
        else if (randomItem == 1) { EquipmentManager.Instance.UnlockItem(KnightArmor); targetItemSprite = itemImages[7]; }
        else if (randomItem == 2) { EquipmentManager.Instance.UnlockItem(DawnRing); targetItemSprite = itemImages[8]; }
    }
    else if (roll >= 100) // roll > 99 ก็คือ 100
    {
        int randomItem = Random.Range(0, 6);
        if (randomItem == 0) { EquipmentManager.Instance.UnlockItem(LightSpear); targetItemSprite = itemImages[9]; }
        else if (randomItem == 1) { EquipmentManager.Instance.UnlockItem(FireLegendarySword); targetItemSprite = itemImages[10]; }
        else if (randomItem == 2) { EquipmentManager.Instance.UnlockItem(WaterLegendaryArmor); targetItemSprite = itemImages[11]; }
        else if (randomItem == 3) { EquipmentManager.Instance.UnlockItem(WindSpear); targetItemSprite = itemImages[12]; }
        else if (randomItem == 4) { EquipmentManager.Instance.UnlockItem(EarthLegendaryArmor); targetItemSprite = itemImages[13]; }
        else if (randomItem == 5) { EquipmentManager.Instance.UnlockItem(DarkLegendaryRing); targetItemSprite = itemImages[14]; }
    }
    else 
    {
      
        targetItemSprite = itemImages[15]; 
        Debug.Log("ผู้เล่นไม่ได้ไอเท็ม (ได้แค่เงิน)");
    }

    // 🟢 ถ้าสุ่มได้ไอเท็ม ให้เรียก Coroutine หน่วงเวลา 5 วินาที
    if (targetItemSprite != null)
    {
        Debug.Log("ผู้เล่นได้รับไอเท็มแล้ว! (กำลังรอโชว์รูป...)");
        StartCoroutine(ShowItemImageAfterDelay(targetItemSprite, 2f));
    }

    // เปิดกล่องแอนิเมชันตามปกติ
    boxScript.OpenBox(resultSprite, () =>
    {
        SafeSetActive(panel, false);
        ResolveGameTurnManager().RequestEndTurn();
    });
}

// 🟢 ฟังก์ชัน Coroutine สำหรับหน่วงเวลาโชว์รูป
private IEnumerator ShowItemImageAfterDelay(Sprite itemSprite, float delayTime)
{
    // ซ่อนรูปไว้ก่อนเผื่อมันเปิดค้างอยู่
    if (showImage != null) SafeSetActive(showImage.gameObject, false);
    
    yield return new WaitForSeconds(delayTime); // รอเวลา

    // พอครบเวลา ค่อยใส่รูปแล้วเปิดให้มองเห็น
    if (showImage != null)
    {
        showImage.sprite = itemSprite;
        SafeSetActive(showImage.gameObject, true);
    }
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
        if (walker != null && ResolveRouteManager() != null)
        {
            var nodes = ResolveRouteManager().nodeConnections.FindAll(x => x.node != null && x.tileID != walker.currentNodeID);
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
        ShowPanel("DrawCard", false);
       
    }

public void OnCardSelected()
{
    
    if (eventPanels.TryGetValue("drawcard", out var panel) && panel != null)
    {
        SafeSetActive(panel, false);
    }

    StartCoroutine(WaitAndEndTurn());
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
            ResolveGameTurnManager()?.RequestEndTurn();
            return;
        }

        PlayerPathWalker walker = target.GetComponent<PlayerPathWalker>();
        if (walker == null)
        {
            Debug.LogWarning("[EventManager] Random move event หา PlayerPathWalker ไม่เจอ -> จบเทิร์น");
            ResolveGameTurnManager()?.RequestEndTurn();
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
            ResolveGameTurnManager()?.RequestEndTurn();
            return;
        }

        StartCoroutine(RandomEventCoroutine());
    }

    public void TriggerMinigameEvent(GameObject target)
    {
        currentEventTarget = target;
        if (!CanSpinRandomEvent(randomMinigameEventKeys, "RandomMinigame"))
        {
            ResolveGameTurnManager()?.RequestEndTurn();
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
                ResolveGameTurnManager()?.RequestEndTurn();
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
                ResolveGameTurnManager()?.RequestEndTurn();
                yield break;
            }

            TriggerEvent(selected, currentEventTarget);
        }
        finally
        {
            isRandomSpinning = false;
        }
    }

    private bool HideBoardSceneRootsForBattle()
    {
        if (!hasBoardSkyboxBeforeBattle)
        {
            boardSkyboxBeforeBattle = RenderSettings.skybox;
            hasBoardSkyboxBeforeBattle = true;
        }

        string boardSceneNameToHide = boardGameSceneName;
        if (string.IsNullOrEmpty(boardSceneNameToHide))
        {
            boardSceneNameToHide = PlayerPrefs.GetString(LastBoardSceneKey, string.Empty);
        }

        if (string.IsNullOrEmpty(boardSceneNameToHide))
        {
            return false;
        }

        Scene boardScene = SceneManager.GetSceneByName(boardSceneNameToHide);
        if (!boardScene.IsValid() || !boardScene.isLoaded)
        {
            Debug.LogWarning($"[EventManager] ไม่พบ board scene '{boardSceneNameToHide}' ที่โหลดอยู่ -> ซ่อนไม่ได้");
            return false;
        }

        hiddenBoardSceneRoots.Clear();
        var roots = boardScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null || !root.activeSelf)
            {
                continue;
            }

            hiddenBoardSceneRoots.Add(root);
            root.SetActive(false);
        }

        hiddenBoardSceneName = boardScene.name;
        Debug.Log($"[EventManager] ซ่อนวัตถุใน board scene '{hiddenBoardSceneName}' จำนวน {hiddenBoardSceneRoots.Count} roots");
        return true;
    }

    private void RestoreHiddenBoardSceneRoots()
    {
        if (hiddenBoardSceneRoots.Count > 0)
        {
            for (int i = 0; i < hiddenBoardSceneRoots.Count; i++)
            {
                GameObject root = hiddenBoardSceneRoots[i];
                if (root == null)
                {
                    continue;
                }

                root.SetActive(true);
            }

            Debug.Log($"[EventManager] แสดง board scene '{hiddenBoardSceneName}' กลับมาแล้ว");
            hiddenBoardSceneRoots.Clear();
        }

        hiddenBoardSceneName = null;

        if (hasBoardSkyboxBeforeBattle)
        {
            RenderSettings.skybox = boardSkyboxBeforeBattle;
            DynamicGI.UpdateEnvironment();
            hasBoardSkyboxBeforeBattle = false;
            boardSkyboxBeforeBattle = null;
        }
    }

    private bool TryApplyBattleSkybox(Scene battleScene)
    {
        if (!battleScene.IsValid() || !battleScene.isLoaded)
        {
            return false;
        }

        var roots = battleScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Skybox[] sceneSkyboxes = roots[i].GetComponentsInChildren<Skybox>(true);
            for (int j = 0; j < sceneSkyboxes.Length; j++)
            {
                Material skyboxMaterial = sceneSkyboxes[j] != null ? sceneSkyboxes[j].material : null;
                if (skyboxMaterial == null)
                {
                    continue;
                }

                RenderSettings.skybox = skyboxMaterial;
                DynamicGI.UpdateEnvironment();
                Debug.Log($"[EventManager] ใช้ skybox จาก battle scene '{battleScene.name}'");
                return true;
            }
        }

        Debug.LogWarning($"[EventManager] battle scene '{battleScene.name}' ไม่มี Skybox component/material ให้ใช้");
        return false;
    }

    private IEnumerator MonsterBattleCoroutine()
    {
        RememberCurrentBoardScene();
        ShowPanel("monster", false);
        yield return new WaitForSeconds(1f);
        string[] Scenes = { "fightDarkNormal", "fightEarthNormal", "fightLightNormal", "fightWaterNormal", "fightWindNormal", "TestFight" };
        int randomIndex = Random.Range(0, Scenes.Length);
        string battleSceneName = Scenes[randomIndex];

        yield return LoadBattleSceneAdditiveCoroutine(battleSceneName);
    }

    private IEnumerator LoadBattleSceneAdditiveCoroutine(string battleSceneName)
    {
        if (string.IsNullOrWhiteSpace(battleSceneName))
        {
            yield break;
        }

        if (SceneManager.GetSceneByName(battleSceneName).isLoaded)
        {
            bool hidBoardForLoadedBattle = HideBoardSceneRootsForBattle();
            Scene existingBattleScene = SceneManager.GetSceneByName(battleSceneName);
            if (existingBattleScene.IsValid() && existingBattleScene.isLoaded)
            {
                SceneManager.SetActiveScene(existingBattleScene);
                TryApplyBattleSkybox(existingBattleScene);
            }
            else if (hidBoardForLoadedBattle)
            {
                RestoreHiddenBoardSceneRoots();
            }

            Debug.LogWarning($"[EventManager] Battle scene '{battleSceneName}' ถูกโหลดอยู่แล้ว -> ไม่โหลดซ้ำ");
            yield break;
        }

        bool hidBoardScene = HideBoardSceneRootsForBattle();

        AsyncOperation loadBattleSceneOperation = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        if (loadBattleSceneOperation == null)
        {
            Debug.LogError($"[EventManager] โหลด battle scene '{battleSceneName}' แบบ additive ไม่สำเร็จ");
            if (hidBoardScene)
            {
                RestoreHiddenBoardSceneRoots();
            }
            yield break;
        }

        yield return loadBattleSceneOperation;

        Scene loadedBattleScene = SceneManager.GetSceneByName(battleSceneName);
        if (loadedBattleScene.IsValid() && loadedBattleScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedBattleScene);
            TryApplyBattleSkybox(loadedBattleScene);
            Debug.Log($"[EventManager] โหลด battle scene '{battleSceneName}' แบบ additive สำเร็จ และซ่อน BoardGame ชั่วคราว");
        }
        else if (hidBoardScene)
        {
            RestoreHiddenBoardSceneRoots();
        }
    }

    private IEnumerator LoadMinigameSceneCoroutine(string minigameKey)
    {
        string sceneName = ResolveMinigameSceneName(minigameKey);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[EventManager] ไม่พบ Scene ของ minigame key '{minigameKey}' -> จบเทิร์น");
            ResolveGameTurnManager()?.RequestEndTurn();
            yield break;
        }

        RememberCurrentBoardScene();
        yield return null;

        if (SceneFlowController.TryRequestScene(sceneName))
        {
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[EventManager] Cannot load minigame scene '{sceneName}'. Check Build Profiles.");
            ResolveGameTurnManager()?.RequestEndTurn();
            yield break;
        }

        yield return LoadSceneAdditiveThenUnloadCurrent(sceneName);
    }

    private IEnumerator LoadSceneAdditiveThenUnloadCurrent(string targetSceneName)
    {
        Scene currentActiveScene = SceneManager.GetActiveScene();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"[EventManager] Failed to start additive load for '{targetSceneName}'.");
            ResolveGameTurnManager()?.RequestEndTurn();
            yield break;
        }

        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            SceneManager.SetActiveScene(targetScene);
        }

        if (currentActiveScene.IsValid()
            && currentActiveScene.isLoaded
            && !string.Equals(currentActiveScene.name, targetSceneName, System.StringComparison.OrdinalIgnoreCase)
            && !string.Equals(currentActiveScene.name, "RuntimeHub", System.StringComparison.OrdinalIgnoreCase))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }
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
       string battleSceneName = string.Empty;

       if (currentSceneName == "MainLight")
       {
           if(bosslevel < 11) battleSceneName = "FinalBoss hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "FianlBoss medium";
           else battleSceneName = "FinalBoss";
       }
       else if (currentSceneName == "TestMain")
       {
           if(bosslevel < 11) battleSceneName = "bossfire hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "bossfire medium";
           else battleSceneName = "bossfire";
       }
       else if (currentSceneName == "MainWater")
       {
           if(bosslevel < 11) battleSceneName = "boss water hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "boss water medium";
           else battleSceneName = "boss water";
       }
       else if (currentSceneName == "MainWind")
       {
           if(bosslevel < 11) battleSceneName = "boss wind hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "boss wind medium";
           else battleSceneName = "boss wind";
       }
       else if (currentSceneName == "MainEarth")
       {
           if(bosslevel < 11) battleSceneName = "boss earth hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "boss earth medium";
           else battleSceneName = "boss earth";
       }
       else if (currentSceneName == "MainDark")
       {
           if(bosslevel < 11) battleSceneName = "boss dark hard";
           else if(bosslevel >=11  && bosslevel <= 15) battleSceneName = "boss dark medium";
           else battleSceneName = "boss dark";
       }

       if (!string.IsNullOrEmpty(battleSceneName))
       {
           yield return LoadBattleSceneAdditiveCoroutine(battleSceneName);
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
         string battleSceneName = string.Empty;

         if(currentSceneName == "MainLight") battleSceneName = "SpecialBoss";
         else if (currentSceneName == "TestMain") battleSceneName = "specialbossfire";
         else if (currentSceneName == "MainWater") battleSceneName = "Special boss water";
         else if (currentSceneName == "MainWind") battleSceneName = "special boss wind";
         else if (currentSceneName == "MainEarth") battleSceneName = "specialboss earth";
         else if (currentSceneName == "MainDark") battleSceneName = "special boss dark";

         if (!string.IsNullOrEmpty(battleSceneName))
         {
             yield return LoadBattleSceneAdditiveCoroutine(battleSceneName);
         }
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

        ResolveGameTurnManager()?.RequestEndTurn();
    }

    private IEnumerator WaitAndEndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        ResolveGameTurnManager()?.RequestEndTurn();
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
