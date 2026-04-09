using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq; 

public class DeckManager : MonoBehaviour
{
    private static DeckManager cachedManager;
    private const string PrimarySceneName = "RuntimeHub";
    public const string PendingCommonOnlyResetKey = "PendingCommonOnlyCardReset";
    private static readonly CardData[] EmptyDeck = new CardData[0];

    public static bool TryGet(out DeckManager manager)
    {
        if (cachedManager != null)
        {
            manager = cachedManager;
            return true;
        }

        manager = FindBestAvailableManager();
        if (manager != null)
        {
            cachedManager = manager;
        }

        return manager != null;
    }

    public static CardData[] CurrentCardUse
    {
        get
        {
            if (TryGet(out var manager) && manager.cardUse != null)
                return manager.cardUse;

            return EmptyDeck;
        }
    }

    public static void TryLockCard(CardData card)
    {
        if (TryGet(out var manager))
            manager.LockCard(card);
    }

    public static void TryRemoveCard(int slotIndex)
    {
        if (TryGet(out var manager))
            manager.RemoveCard(slotIndex);
    }

    public static void MarkPendingCommonOnlyReset()
    {
        PlayerPrefs.SetInt(PendingCommonOnlyResetKey, 1);
        PlayerPrefs.Save();
    }

    [Header("Deck Data")]
    public List<CardData> allCards;       // การ์ดทั้งหมดในเกม (Database)
    public CardData[] cardUse = new CardData[20]; // เด็คปัจจุบัน

    [Header("UI References")]
    public Button[] addButtons;           // ปุ่มเลือกการ์ด (ใน Scroll View)
    public Button[] removeButtons;        // ปุ่มลบการ์ด (ในช่อง Deck)
    public Image[] useCardImages;         // รูปภาพในช่อง Deck
    public Sprite defaultSprite;          // รูปช่องว่าง

    private void Awake()
    {
        if (cachedManager == null)
        {
            cachedManager = this;
            return;
        }

        if (cachedManager == this)
        {
            return;
        }

        bool thisIsPrimary = IsInPrimaryScene(this);
        bool cachedIsPrimary = IsInPrimaryScene(cachedManager);

        if (thisIsPrimary && !cachedIsPrimary)
        {
            DeckManager oldManager = cachedManager;
            cachedManager = this;
            if (oldManager != null)
            {
                Destroy(oldManager.gameObject);
            }

            return;
        }

        Destroy(gameObject);
    }

    private static bool IsInPrimaryScene(DeckManager manager)
    {
        return manager != null
            && manager.gameObject.scene.IsValid()
            && string.Equals(manager.gameObject.scene.name, PrimarySceneName, System.StringComparison.OrdinalIgnoreCase);
    }

    private static DeckManager FindBestAvailableManager()
    {
        DeckManager[] managers = FindObjectsByType<DeckManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        DeckManager fallback = null;

        foreach (DeckManager manager in managers)
        {
            if (manager == null)
            {
                continue;
            }

            if (IsInPrimaryScene(manager))
            {
                return manager;
            }

            if (fallback == null)
            {
                fallback = manager;
            }
        }

        return fallback;
    }

    private void OnDestroy()
    {
        if (cachedManager == this)
        {
            cachedManager = null;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 1. โหลดสถานะการ์ด (ว่าใบไหนใช้ได้/ไม่ได้) จาก PlayerPrefs
        LoadCardStates();
        ApplyPendingCommonOnlyResetIfNeeded();

        // 2. โหลดเด็คที่เคยจัดไว้ล่าสุดจาก PlayerPrefs
        LoadDeckFromPrefs();

        // 3. เริ่มต้นระบบ UI
        BindRemoveButtons();
        UpdateUseCardUI();
        SortAndRefreshCards();

        // RuntimeHub เป็นฉากแรกหลังเมนู จึงต้อง bind UI ของฉากปัจจุบันด้วย
        // (ไม่พึ่งเฉพาะ sceneLoaded event อย่างเดียว)
        StartCoroutine(WaitAndBindUI());
    }

    private void ApplyPendingCommonOnlyResetIfNeeded()
    {
        if (PlayerPrefs.GetInt(PendingCommonOnlyResetKey, 0) != 1)
            return;

        if (ResetAllCardStatesToCommonOnly())
        {
            PlayerPrefs.DeleteKey(PendingCommonOnlyResetKey);
            PlayerPrefs.Save();
        }
    }

    private bool ResetAllCardStatesToCommonOnly()
    {
        if (allCards == null)
            return false;

        foreach (CardData card in allCards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.cardName))
                continue;

            bool isCommon = card.rarity == CardRarity.Common;
            card.isUsable = isCommon;
            PlayerPrefs.SetInt("CardState_" + card.cardName, isCommon ? 1 : 0);
        }

        if (cardUse != null)
        {
            for (int i = 0; i < cardUse.Length; i++)
            {
                cardUse[i] = null;
            }
        }

        PlayerPrefs.DeleteKey("CurrentDeckData");
        return true;
    }

    // --- ส่วนจัดการ PlayerPrefs (Save/Load) ---

    // โหลดสถานะ Usable ของการ์ดทุกใบ
    void LoadCardStates()
    {
        if (allCards == null)
        {
            Debug.LogWarning("DeckManager: allCards ยังไม่ได้ bind ใน Inspector");
            return;
        }

        foreach (var card in allCards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.cardName))
            {
                continue;
            }

            string key = "CardState_" + card.cardName;
            // ถ้ามีค่าบันทึกไว้ ให้ใช้ค่านั้น (1=True, 0=False)
            if (PlayerPrefs.HasKey(key))
            {
                card.isUsable = PlayerPrefs.GetInt(key) == 1;
            }
            else
            {
                // ถ้าไม่มีค่าใน PlayerPrefs ให้ใช้ค่าเริ่มต้นแบบ Common-only
                // เพื่อกันกรณี Inspector ติด true มาทุกใบ
                bool isCommonCard = card.rarity == CardRarity.Common;
                card.isUsable = isCommonCard;
                PlayerPrefs.SetInt(key, isCommonCard ? 1 : 0);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("📖 Loaded Card States from PlayerPrefs");
    }

    // โหลดเด็คจาก String ใน PlayerPrefs
    void LoadDeckFromPrefs()
    {
        string savedDeckString = PlayerPrefs.GetString("CurrentDeckData", ""); 
    
        if (!string.IsNullOrEmpty(savedDeckString))
        {
            // เคลียร์เด็คเก่าก่อน
            for(int k=0; k<20; k++) cardUse[k] = null;

            string[] splitNames = savedDeckString.Split(',');
            for (int i = 0; i < splitNames.Length; i++)
            {
                 if (i >= 20) break;
                 string cName = splitNames[i];
                 
                 // ค้นหาการ์ดจากชื่อ
                 if (cName != "EMPTY" && !string.IsNullOrEmpty(cName))
                 {
                     CardData found = allCards.Find(x => x.cardName == cName); 
                     if (found != null) cardUse[i] = found;
                 }
            }
            Debug.Log("📖 Loaded Deck from PlayerPrefs");
        }
    }

    // บันทึกเด็คปัจจุบันลง PlayerPrefs (Auto-Save)
    public void SaveCurrentDeck()
    {
        List<string> names = new List<string>();
        List<string> selectedIds = new List<string>();
        List<CardData> selectedCards = new List<CardData>();
        foreach (var c in cardUse)
        {
            string cardId = c != null ? c.cardName : "EMPTY";
            names.Add(cardId);

            if (c != null)
            {
                selectedIds.Add(c.cardName);
                selectedCards.Add(c);
            }
        }

        string deckStr = string.Join(",", names);
        PlayerPrefs.SetString("CurrentDeckData", deckStr);
        PlayerPrefs.Save();

        if (RunSessionStore.TryGet(out var sessionStore))
        {
            sessionStore.SetSelectedDeck(selectedIds);
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.SetSelectedCards(selectedCards);
        }

        Debug.Log("💾 Deck Auto-Saved!");
    }

    // ฟังก์ชันสำหรับปลดล็อคการ์ด (เรียกใช้จากภายนอก เช่น ตอนเปิดกาชา)
    public void UnlockCard(CardData card)
    {
        card.isUsable = true;
        PlayerPrefs.SetInt("CardState_" + card.cardName, 1);
        PlayerPrefs.Save();
        SortAndRefreshCards(); // รีเฟรชปุ่มทันที
        Debug.Log("🔓 Unlocked Card: " + card.cardName);
    }

    // --- Core Functionality (แก้ไขให้มีการ Save อัตโนมัติ) ---

    public void AddCard(CardData card)
    {
        // เช็คว่ามีซ้ำไหม
        foreach (var c in cardUse)
        {
            if (c != null && c.cardName == card.cardName) 
            { 
                Debug.Log("การ์ด " + card.cardName + " ถูกเลือกไปแล้ว"); 
                return; 
            }
        }

        // หาช่องว่างแล้วใส่
        for (int i = 0; i < cardUse.Length; i++)
        {
            if (cardUse[i] == null) 
            { 
                cardUse[i] = card; 
                UpdateUseCardUI(); 
                SaveCurrentDeck(); 
                
                // 🟢 สั่งรีเฟรชคลังการ์ดทันที เพื่อให้ใบที่เพิ่งกดใส่กลายเป็นสีมืด
                SortAndRefreshCards(); 
                return; 
            }
        }
        Debug.Log("เด็คเต็มแล้ว!");
    }

    public void RemoveCard(int index)
    {
        if (cardUse == null || index < 0 || index >= cardUse.Length)
        {
            Debug.LogWarning($"DeckManager: RemoveCard index ไม่ถูกต้อง ({index})");
            return;
        }

        if (cardUse[index] != null)
        {
            cardUse[index] = null;
            UpdateUseCardUI();
            SaveCurrentDeck();
            
            // 🟢 สั่งรีเฟรชคลังการ์ดทันที เพื่อให้ใบที่เพิ่งถอดออกกลับมาสว่างและกดได้
            SortAndRefreshCards(); 
        }
    }

    // --- UI Logic (เหมือนเดิม) ---

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitAndBindUI());
    }

    private IEnumerator WaitAndBindUI()
    {
        yield return null;
        Scene activeScene = SceneManager.GetActiveScene();

        var addObjs = GameObject.FindGameObjectsWithTag("AddButton")
            .Where(obj => obj.scene == activeScene)
            .ToArray();
        var removeObjs = GameObject.FindGameObjectsWithTag("RemoveButton")
            .Where(obj => obj.scene == activeScene)
            .ToArray();
        var useCardObjs = GameObject.FindGameObjectsWithTag("UseCardImage")
            .Where(obj => obj.scene == activeScene)
            .ToArray();

        if (addObjs.Length > 0)
        {
            // Sort UI elements based on hierarchy order
            System.Array.Sort(addObjs, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            System.Array.Sort(removeObjs, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            System.Array.Sort(useCardObjs, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            addButtons = System.Array.ConvertAll(addObjs, o => o.GetComponent<Button>());
            removeButtons = System.Array.ConvertAll(removeObjs, o => o.GetComponent<Button>());
            useCardImages = System.Array.ConvertAll(useCardObjs, o => o.GetComponent<Image>());

            BindRemoveButtons();
            BindRightClickEvents();
            UpdateUseCardUI();
            SortAndRefreshCards();
            Debug.Log("✅ DeckManager: UI Bound สำเร็จใน Scene " + SceneManager.GetActiveScene().name);
        }
    }
    
    void BindRightClickEvents()
    {
        if (useCardImages == null) return;

        for (int i = 0; i < useCardImages.Length; i++)
        {
            if (useCardImages[i] == null) continue;

            GameObject obj = useCardImages[i].gameObject;

            // 1. ลองดึง Component มาก่อน ถ้าไม่มีให้ Add เข้าไป
            DeckSlotRightClick clicker = obj.GetComponent<DeckSlotRightClick>();
            if (clicker == null)
            {
                clicker = obj.AddComponent<DeckSlotRightClick>();
            }

            // 2. ระบุว่าปุ่มนี้คือ Index ที่เท่าไหร่
            clicker.slotIndex = i;
        }
    }
    
    void BindRemoveButtons()
    {
        if (removeButtons == null || removeButtons.Length == 0)
        {
            return;
        }

        for (int i = 0; i < removeButtons.Length; i++)
        {
            Button button = removeButtons[i];
            if (button == null)
            {
                Debug.LogWarning($"DeckManager: removeButtons[{i}] เป็น null (ยังไม่ได้ผูก Button component หรือ Object ไม่ถูกต้อง)");
                continue;
            }

            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => RemoveCard(index));
        }
    }

 public void UpdateUseCardUI()
    {
        if (useCardImages == null || cardUse == null) return;

        int usableCount = 0;
        if (allCards != null)
        {
            foreach (var c in allCards)
            {
                if (c != null && c.isUsable) usableCount++;
            }
        }

        int maxActiveSlots = Mathf.Min(usableCount, useCardImages.Length);
        
        // 🟢 แอบติดกล้องวงจรปิดดูว่ามันนับการ์ดได้กี่ใบ!
        Debug.Log($"[DeckManager] มีการ์ดที่ใช้ได้ทั้งหมด: {usableCount} ใบ -> จะเปิด UI ทั้งหมด: {maxActiveSlots} ช่อง");

        for (int i = 0; i < useCardImages.Length; i++)
        {
            if (useCardImages[i] == null) continue;

            bool isSlotActive = i < maxActiveSlots;

            // 🟢 แก้ตรงนี้! ลบคำว่า .transform.parent ออก ให้มันปิดแค่ช่องของตัวเองพอ
            useCardImages[i].gameObject.SetActive(isSlotActive); 

            if (!isSlotActive) continue;

            if (i < cardUse.Length && cardUse[i] != null)
            {
                useCardImages[i].sprite = cardUse[i].icon;
            }
            else 
            {
                if (defaultSprite != null) useCardImages[i].sprite = defaultSprite;
                else useCardImages[i].sprite = null; 
            }
        }
    }
    public void SortAndRefreshCards()
    {
        if (allCards == null || addButtons == null)
        {
            return;
        }

        // เรียงการ์ดตาม Rarity -> Name
        allCards = allCards
            .OrderBy(card => card.rarity)
            .ThenBy(card => card.cardName)
            .ToList();

        int count = Mathf.Min(allCards.Count, addButtons.Length);

        for (int i = 0; i < count; i++)
        {
            CardData card = allCards[i];
            Button btn = addButtons[i];

            if (btn == null) continue; 
            Transform cardObj = btn.transform.parent;
            if (cardObj == null) continue; 

            // จัดลำดับ UI
            cardObj.SetSiblingIndex(i);

            // ผูกปุ่ม Add
            int index = i; 
            btn.onClick.RemoveAllListeners();
            if (card != null) 
            {
                btn.onClick.AddListener(() => AddCard(allCards[index])); 
            }

            // 🟢 1. เช็คว่าการ์ดใบนี้ "ถูกใส่ลงไปในเด็คแล้วหรือยัง?"
            bool isAlreadyInDeck = false;
            if (card != null && cardUse != null)
            {
                foreach (var c in cardUse)
                {
                    if (c != null && c.cardName == card.cardName)
                    {
                        isAlreadyInDeck = true;
                        break;
                    }
                }
            }

            Image img = cardObj.GetComponent<Image>();
            
            // 🟢 2. ปรับเงื่อนไข: ถ้าการ์ดโดนล็อค(ยังไม่ปลด) หรือ "อยู่ในเด็คแล้ว" ให้ปุ่มมืดและกดไม่ได้
            if (card == null || !card.isUsable || isAlreadyInDeck)
            {
                btn.interactable = false;
                if (img != null) img.color = new Color(0.5f, 0.5f, 0.5f, 1f); // สีเทามืด
            }
            else
            {
                btn.interactable = true;
                if (img != null) img.color = Color.white; // สีสว่างปกติ
            }
        }
    }

    

    // ฟังก์ชันเดิม (ยังคงไว้เผื่อมีการเปลี่ยน Scene)
    

    
    // ในไฟล์ DeckManager.cs

// ฟังก์ชันสำหรับทำให้การ์ดใช้ไม่ได้ (เช่น ใช้แล้วหมดไป หรือติด Cooldown)
public void LockCard(CardData card)
{
    if (card == null) return;

    // 1. เปลี่ยนค่าใน Runtime (เหมือนเดิม)
    card.isUsable = false;

    // 2. บันทึกลง PlayerPrefs (เหมือนเดิม)
    PlayerPrefs.SetInt("CardState_" + card.cardName, 0); 

    // --- ส่วนที่เพิ่ม: วนลูปหาการ์ดในเด็ค แล้วลบออก ---
    for (int i = 0; i < cardUse.Length; i++)
    {
        // เช็คว่าช่องนี้มีการ์ด และชื่อตรงกับใบที่กำลังล็อค
        if (cardUse[i] != null && cardUse[i].cardName == card.cardName)
        {
            cardUse[i] = null; // ลบออกจากเด็ค (Set เป็น null)
        }
    }

    // บันทึกสถานะเด็คใหม่ลง PlayerPrefs ทันที (เพื่อให้การลบมีผลถาวร)
    // (ฟังก์ชันนี้มีอยู่ใน DeckManager ที่เราเขียนกันก่อนหน้านี้แล้ว)
    SaveCurrentDeck(); 
    // ----------------------------------------------------

    PlayerPrefs.Save();

    // (แถม) รีเฟรช UI เฉพาะใน DeckManager เผื่อหน้าจอเปิดอยู่
    UpdateUseCardUI(); 
    
    Debug.Log($"🚫 Card Locked & Removed from Deck: {card.cardName}");
}
}
