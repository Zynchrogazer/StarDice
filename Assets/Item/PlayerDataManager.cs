using UnityEngine;
using System;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    public EquipmentData[] equippedItems = new EquipmentData[2];
    public Action OnEquipmentChanged; 
    private const string EquipSlotPrefKeyPrefix = "EquipSlot_";

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        LoadEquippedItems();
    }

    private void Start()
    {
        // กันกรณีลำดับ Awake สลับกัน แล้ว EquipmentManager ยังไม่พร้อมตอน Awake
        if (EquipmentManager.Instance != null)
        {
            LoadEquippedItems();
            OnEquipmentChanged?.Invoke();
        }
    }

    // เพิ่ม int slotIndex เข้ามาในวงเล็บ
    public void EquipItem(EquipmentData newItem, int slotIndex)
    {
        if (newItem == null) return;
        if (slotIndex < 0 || slotIndex >= equippedItems.Length)
        {
            Debug.LogWarning($"[PlayerDataManager] slotIndex {slotIndex} is out of range");
            return;
        }

        if (newItem.itemID != ItemID.None && !newItem.isOwned)
        {
            Debug.LogWarning($"[PlayerDataManager] cannot equip unowned item: {newItem.itemName}");
            return;
        }

        // 1. เช็คก่อนว่า "ของชิ้นนี้" มันถูกใส่อยู่ใน "ช่องอื่น" หรือเปล่า?
        // ถ้าใส่ซ้ำช่องอื่นอยู่ ให้ลบออกจากช่องเดิมก่อน (กันไอเท็มซ้ำ 2 ช่อง)
        for (int i = 0; i < equippedItems.Length; i++)
        {
            if (equippedItems[i] == newItem && i != slotIndex)
            {
                equippedItems[i] = null; // ถอดของเดิมออก
            }
        }

        // 2. ยัดใส่ช่องที่ระบุมาเลย (ทับของเก่าไปเลย)
        equippedItems[slotIndex] = newItem;

        // 3. แจ้งเตือน UI
        SaveEquippedItems();
        OnEquipmentChanged?.Invoke();
    }
    public bool IsItemEquipped(EquipmentData itemToCheck)
    {
        foreach (var equipped in equippedItems)
        {
            // ถ้าเจอว่าไอเท็มในตัว ตรงกับ ไอเท็มที่ส่งมาเช็ค แสดงว่า "ใส่อยู่"
            if (equipped == itemToCheck)
            {
                return true;
            }
        }
        return false; // หาไม่เจอ แปลว่าไม่ได้ใส่อยู่
    }

    private void SaveEquippedItems()
    {
        for (int i = 0; i < equippedItems.Length; i++)
        {
            ItemID id = equippedItems[i] != null ? equippedItems[i].itemID : ItemID.None;
            PlayerPrefs.SetInt(EquipSlotPrefKeyPrefix + i, (int)id);
        }

        PlayerPrefs.Save();
    }

    private void LoadEquippedItems()
    {
        if (EquipmentManager.Instance == null)
        {
            return;
        }

        for (int i = 0; i < equippedItems.Length; i++)
        {
            ItemID id = (ItemID)PlayerPrefs.GetInt(EquipSlotPrefKeyPrefix + i, (int)ItemID.None);
            equippedItems[i] = id == ItemID.None ? null : EquipmentManager.Instance.GetEquipmentById(id);
        }
    }
}
