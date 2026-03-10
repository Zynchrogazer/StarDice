using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq; 

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public static bool TryGet(out DeckManager manager)
    {
        manager = Instance;
        return manager != null;
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
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

        // 2. โหลดเด็คที่เคยจัดไว้ล่าสุดจาก PlayerPrefs
        LoadDeckFromPrefs();

        // 3. เริ่มต้นระบบ UI
        BindRemoveButtons(); 
        UpdateUseCardUI();
        SortAndRefreshCards(); 
    }

    // --- ส่วนจัดการ PlayerPrefs (Save/Load) ---

    // โหลดสถานะ Usable ของการ์ดทุกใบ
    void LoadCardStates()
    {
        foreach (var card in allCards)
        {
            string key = "CardState_" + card.cardName;
            // ถ้ามีค่าบันทึกไว้ ให้ใช้ค่านั้น (1=True, 0=False)
            if (PlayerPrefs.HasKey(key))
            {
                card.isUsable = PlayerPrefs.GetInt(key) == 1;
            }
            // ถ้าไม่มี (เพิ่งเล่นครั้งแรก) จะใช้ค่า Default ที่ตั้งใน Inspector
        }
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
        foreach (var c in cardUse)
        {
            names.Add(c != null ? c.cardName : "EMPTY");
        }
        
        string deckStr = string.Join(",", names);
        PlayerPrefs.SetString("CurrentDeckData", deckStr);
        PlayerPrefs.Save();
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
            if (c == card) { Debug.Log("การ์ด " + card.cardName + " ถูกเลือกไปแล้ว"); return; }
        }

        // หาช่องว่างแล้วใส่
        for (int i = 0; i < cardUse.Length; i++)
        {
            if (cardUse[i] == null) 
            { 
                cardUse[i] = card; 
                UpdateUseCardUI(); 
                
                SaveCurrentDeck(); // <--- เพิ่ม: ใส่ปุ๊บเซฟปั๊บ
                return; 
            }
        }
        Debug.Log("เด็คเต็มแล้ว!");
    }

    public void RemoveCard(int index)
    {
        if (cardUse[index] != null) 
        { 
            cardUse[index] = null; 
            UpdateUseCardUI(); 
            
            SaveCurrentDeck(); // <--- เพิ่ม: ลบปุ๊บเซฟปั๊บ
        }
    }

    // --- UI Logic (เหมือนเดิม) ---

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FirstDeck")
        {
            ResetDeckForFirstDeckScene();
        }

        StartCoroutine(WaitAndBindUI());
    }

    void ResetDeckForFirstDeckScene()
    {
        // เคลียร์เด็คที่เคยจัดไว้ทั้งหมด
        for (int i = 0; i < cardUse.Length; i++)
        {
            cardUse[i] = null;
        }

        // ปลดล็อกเฉพาะการ์ดระดับ Common และล็อกความหายากอื่น
        foreach (var card in allCards)
        {
            if (card == null) continue;

            bool shouldBeUsable = card.rarity == CardRarity.Common;
            card.isUsable = shouldBeUsable;
            PlayerPrefs.SetInt("CardState_" + card.cardName, shouldBeUsable ? 1 : 0);
        }

        SaveCurrentDeck();
        PlayerPrefs.Save();

        UpdateUseCardUI();
        SortAndRefreshCards();
        Debug.Log("♻️ Entered FirstDeck: Deck reset and only Common cards are unlocked.");
    }

    private IEnumerator WaitAndBindUI()
    {
        yield return null;
        var addObjs = GameObject.FindGameObjectsWithTag("AddButton");
        var removeObjs = GameObject.FindGameObjectsWithTag("RemoveButton");
        var useCardObjs = GameObject.FindGameObjectsWithTag("UseCardImage");

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
        if (removeButtons != null && removeButtons.Length > 0)
        {
            for (int i = 0; i < removeButtons.Length; i++)
            {
                int index = i;
                removeButtons[i].onClick.RemoveAllListeners();
                removeButtons[i].onClick.AddListener(() => RemoveCard(index));
            }
        }
    }

   public void UpdateUseCardUI()
{
    // 1. เช็คว่า Array UI มีอยู่จริงไหม
    if (useCardImages == null) return;

    // 2. เช็คว่า Array Data มีอยู่จริงไหม (กัน cardUse.Length พัง)
    if (cardUse == null) 
    {
        // ถ้าไม่มีข้อมูลการ์ด ให้เคลียร์รูปทิ้งให้หมด (ถ้าต้องการ) หรือแค่ return
        return; 
    }

    for (int i = 0; i < useCardImages.Length; i++)
    {
        // 3. (สำคัญมาก) เช็คว่าช่องใส่รูปใน Inspector ถูกลากใส่หรือยัง?
        // ถ้าช่องนี้ว่าง (None) ให้ข้ามไปเลย กัน Error
        if (useCardImages[i] == null) 
        {
            Debug.LogWarning($"DeckManager: ช่อง useCardImages[{i}] ใน Inspector ยังไม่ได้ลากใส่ Image!");
            continue; 
        }

        // --- เริ่มการทำงาน ---
        if (i < cardUse.Length && cardUse[i] != null)
        {
            // ใส่รูปการ์ด
            useCardImages[i].sprite = cardUse[i].icon;
        }
        else 
        {
            // ใส่รูป Default (ถ้ามี) หรือเคลียร์เป็น null
            if (defaultSprite != null)
            {
                useCardImages[i].sprite = defaultSprite;
            }
            else
            {
                // ถ้าไม่มี defaultSprite ให้ใส่ null (รูปจะหายไป) หรือปล่อยไว้
                useCardImages[i].sprite = null; 
            }
        }
    }
}

    public void SortAndRefreshCards()
    {
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

            // จัดการสถานะ isUsable (สีเทา/สีปกติ)
            Image img = cardObj.GetComponent<Image>();
            if (card == null || !card.isUsable)
            {
                btn.interactable = false;
                if (img != null) img.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                btn.interactable = true;
                if (img != null) img.color = Color.white;
            }
        }
    }

    // ฟังก์ชันเดิม (ยังคงไว้เผื่อมีการเปลี่ยน Scene)
    public void ConfirmDeckAndGoNextScene(string nextScene)
    {
        SaveCurrentDeck(); // เซฟก่อนเปลี่ยนฉากเพื่อความชัวร์
        SceneManager.LoadScene(nextScene, LoadSceneMode.Additive);
        
        var canvasList = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvasList)
        {
            if (canvas.gameObject.scene.name == SceneManager.GetActiveScene().name)
                canvas.enabled = false;
        }
    }
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
