using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class BackupSaveManager : MonoBehaviour
{
    [Header("DATABASE")]
    public List<CardData> allCardsDatabase;

    [Header("UI References")]
    public GameObject savePanelObject;
    public GameObject loadPanelObject;
    public List<SaveSlot> allSlots; 
    public Button saveModeButton;
    public Button loadModeButton;
    public TMP_InputField nameInputField;

    [Header("Slot Images (เพิ่มใหม่!)")]
    public Sprite emptySlotSprite;  // รูปที่จะโชว์ตอนช่องว่าง
    public Sprite savedSlotSprite;  // รูปที่จะโชว์ตอนมีเซฟ

    [Header("New Game System")]
    public Button newGameButton; 
    public GameObject confirmPanel; 
    public Button confirmYesButton; 
    public Button confirmNoButton; 

    [Header("Button Colors Setting")]
    public Color normalColor = Color.white;
    public Color activeColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private bool isSaveMode = true;

    private string[] keysToBackup = new string[] 
    { 
        "MonsterWater", "MonsterEarth", "MonsterWind", "MonsterLight", "MonsterDark",
    };

    private void Start()
    {
        foreach (var slot in allSlots) slot.Setup(this);
        
        saveModeButton.onClick.AddListener(SetSaveMode);
        loadModeButton.onClick.AddListener(SetLoadMode);
        
        if(newGameButton) { newGameButton.onClick.RemoveAllListeners(); newGameButton.onClick.AddListener(OpenConfirmPanel); }
        if(confirmYesButton) { confirmYesButton.onClick.RemoveAllListeners(); confirmYesButton.onClick.AddListener(ResetActiveGame); }
        if(confirmNoButton) { confirmNoButton.onClick.RemoveAllListeners(); confirmNoButton.onClick.AddListener(CloseConfirmPanel); }
        
        if(confirmPanel) confirmPanel.SetActive(false);

        SetSaveMode();
    }

    public void OpenConfirmPanel() { if(confirmPanel) confirmPanel.SetActive(true); RefreshButtonColors(); }
    public void CloseConfirmPanel() { if(confirmPanel) confirmPanel.SetActive(false); RefreshButtonColors(); }

    public void SetSaveMode() 
    { 
        isSaveMode = true; 
        savePanelObject.SetActive(true); 
        loadPanelObject.SetActive(false); 
        if(confirmPanel) confirmPanel.SetActive(false);
        RefreshAllSlots(); 
        RefreshButtonColors(); 
    }

    public void SetLoadMode() 
    { 
        isSaveMode = false; 
        savePanelObject.SetActive(false); 
        loadPanelObject.SetActive(true); 
        if(confirmPanel) confirmPanel.SetActive(false);
        RefreshAllSlots(); 
        RefreshButtonColors(); 
    }

    void RefreshButtonColors()
    {
        if (saveModeButton) saveModeButton.image.color = normalColor;
        if (loadModeButton) loadModeButton.image.color = normalColor;
        if (newGameButton) newGameButton.image.color = normalColor;
        
        if (confirmPanel != null && confirmPanel.activeSelf) { if (newGameButton) newGameButton.image.color = activeColor; }
        else if (isSaveMode) { if (saveModeButton) saveModeButton.image.color = activeColor; }
        else { if (loadModeButton) loadModeButton.image.color = activeColor; }
    }

 public void RefreshAllSlots()
    {
        for (int i = 0; i < allSlots.Count; i++)
        {
            // ⭐️ แก้ไขจุดที่ 1: ใช้ .slotNumber ของปุ่มนั้นๆ (ไม่ใช่ i)
            // เพื่อให้ปุ่มในหน้า Load (ซึ่งเป็นตัวที่ 6-10 ในลิสต์) รู้ว่าตัวเองต้องไปดึงข้อมูล Slot 0-4 มาโชว์
            int realSlotID = allSlots[i].slotNumber;
            string prefix = "Slot" + realSlotID + "_";
            
            bool hasSave = PlayerPrefs.HasKey(prefix + "HasSave");

            // ⭐️ แก้ไขจุดที่ 2: ใส่โค้ดสลับหน้าตา (Toggle Visuals) กลับเข้าไป
            if (hasSave)
            {
                // มีเซฟ: เปิดตัว HasSave, ปิดตัว NoSave
                if (allSlots[i].hasSaveState != null) allSlots[i].hasSaveState.SetActive(true);
                if (allSlots[i].noSaveState != null) allSlots[i].noSaveState.SetActive(false);
                
                // โชว์ปุ่มลบ
                if (allSlots[i].deleteButton != null) allSlots[i].deleteButton.gameObject.SetActive(true);

                // ใส่ข้อความ
                allSlots[i].slotNameText.text = PlayerPrefs.GetString(prefix + "SaveName");
                if(allSlots[i].detailsText) allSlots[i].detailsText.text = PlayerPrefs.GetString(prefix + "SaveDate");
            }
            else
            {
                // ว่างเปล่า: ปิดตัว HasSave, เปิดตัว NoSave
                if (allSlots[i].hasSaveState != null) allSlots[i].hasSaveState.SetActive(false);
                if (allSlots[i].noSaveState != null) allSlots[i].noSaveState.SetActive(true);

                // ซ่อนปุ่มลบ
                if (allSlots[i].deleteButton != null) allSlots[i].deleteButton.gameObject.SetActive(false);

                // ใส่ข้อความ
                allSlots[i].slotNameText.text = "EMPTY SLOT " + (realSlotID + 1); // ใช้ realSlotID แสดงเลข
                if(allSlots[i].detailsText) allSlots[i].detailsText.text = "-";
            }
        }
    }
    public void OnSlotClicked(int slotIndex)
    {
        string prefix = "Slot" + slotIndex + "_"; 

        if (isSaveMode)
        {
            // ================= SAVE =================
            
            // ⭐️ แก้ไขตรงนี้: เปลี่ยนชื่อเซฟเป็น วันที่+เวลา แบบละเอียด
            // รูปแบบ: จันทร์ 1 ธ.ค. 2025 - 16:30
            // หรือใช้แบบสากล: yyyy-MM-dd HH:mm
            
            string dateString = DateTime.Now.ToString("dd/MM/yyyy HH:mm"); // 01/12/2025 16:30
            
            // ถ้าอยากได้ชื่อวันด้วย (เช่น Mon, Tue) ให้ใช้ "ddd dd/MM/yyyy HH:mm"
            // string dateString = DateTime.Now.ToString("ddd dd/MM/yyyy HH:mm");

            // บันทึกชื่อเป็นวันที่เลย
            PlayerPrefs.SetString(prefix + "SaveName", dateString);
            
            // ส่วน SaveDate ก็เก็บเหมือนเดิม (หรือจะเก็บค่าเดียวกันก็ได้)
            PlayerPrefs.SetString(prefix + "SaveDate", dateString);
            
            PlayerPrefs.SetInt(prefix + "HasSave", 1);

            // ... (ส่วน Backup Monster/Card/Deck เหมือนเดิม ไม่ต้องแก้) ...
            foreach (string key in keysToBackup) { int val = PlayerPrefs.GetInt(key, 0); PlayerPrefs.SetInt(prefix + key, val); }
            foreach (var card in allCardsDatabase) { string cardKey = "CardState_" + card.cardName; int state = card.isUsable ? 1 : 0; PlayerPrefs.SetInt(prefix + cardKey, state); PlayerPrefs.SetInt(cardKey, state); }
            
            string currentDeckString = "";
            DeckManager activeDeck = FindObjectOfType<DeckManager>();
            if (activeDeck != null) { List<string> names = new List<string>(); foreach(var c in activeDeck.cardUse) names.Add(c != null ? c.cardName : "EMPTY"); currentDeckString = string.Join(",", names); }
            else { currentDeckString = PlayerPrefs.GetString("CurrentDeckData", ""); }
            PlayerPrefs.SetString(prefix + "DeckData", currentDeckString);
            PlayerPrefs.SetString("CurrentDeckData", currentDeckString);

            PlayerPrefs.Save();
            Debug.Log($"✅ Saved to Slot {slotIndex}");
            RefreshAllSlots();
        }
        else
        {
            // ... (ส่วน Load เหมือนเดิม) ...
            if (PlayerPrefs.HasKey(prefix + "HasSave"))
            {
                // ... (Restore Logic) ...
                foreach (string key in keysToBackup) { int val = PlayerPrefs.GetInt(prefix + key, 0); PlayerPrefs.SetInt(key, val); }
                foreach (var card in allCardsDatabase) { string cardKey = "CardState_" + card.cardName; int state = PlayerPrefs.GetInt(prefix + cardKey, 0); PlayerPrefs.SetInt(cardKey, state); card.isUsable = (state == 1); }
                string backupDeckString = PlayerPrefs.GetString(prefix + "DeckData", "");
                PlayerPrefs.SetString("CurrentDeckData", backupDeckString);
                PlayerPrefs.Save();
                ApplyDataToGame(backupDeckString);
            }
        }
    }

    public void ResetActiveGame()
    {
        CloseConfirmPanel();

        foreach (string key in keysToBackup) PlayerPrefs.DeleteKey(key);
        PlayerPrefs.DeleteKey("CurrentDeckData");
        //PlayerData.ResetSharedCredit();
        SkillManager.ClearSavedUnlockedSkills();
        PassiveSkillManager.ClearSavedProgress();
        PlayerDataManager.ClearSavedEquipSlots();
        EquipmentManager.ClearSavedOwnershipStates();

        if (allCardsDatabase != null)
        {
            foreach (var card in allCardsDatabase)
            {
                if (card == null) continue;
                string cardKey = "CardState_" + card.cardName;
                PlayerPrefs.DeleteKey(cardKey);
                card.isUsable = false; 
            }
        }

        PlayerPrefs.Save();
        
        try { ApplyDataToGame(""); } catch (System.Exception e) { Debug.LogWarning(e.Message); }
        Debug.Log("🗑️ New Game Started");
    }

    void ApplyDataToGame(string deckString)
    {
        MonsterUnlockUI monsterUI = FindObjectOfType<MonsterUnlockUI>();
        if (monsterUI != null) monsterUI.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

        DeckManager activeDeck = FindObjectOfType<DeckManager>();
        if (activeDeck != null)
        {
            activeDeck.SortAndRefreshCards();
            for(int k=0; k<20; k++) activeDeck.cardUse[k] = null; 
            if (!string.IsNullOrEmpty(deckString))
            {
                string[] splitNames = deckString.Split(',');
                for (int i = 0; i < splitNames.Length; i++)
                {
                    if (i >= 20) break;
                    string cName = splitNames[i];
                    if (cName != "EMPTY" && !string.IsNullOrEmpty(cName))
                    {
                        CardData found = allCardsDatabase.Find(x => x.cardName == cName);
                        if (found != null) activeDeck.cardUse[i] = found;
                    }
                }
            }
            activeDeck.UpdateUseCardUI();
        }
    }
    
    public void OnDeleteFileClicked(int slotIndex)
    {
        string prefix = "Slot" + slotIndex + "_";
        PlayerPrefs.DeleteKey(prefix + "HasSave");
        PlayerPrefs.Save();
        RefreshAllSlots();
    }
}