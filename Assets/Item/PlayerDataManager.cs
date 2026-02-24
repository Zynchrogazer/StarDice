using UnityEngine;
using System;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    public EquipmentData[] equippedItems = new EquipmentData[2];
    public Action OnEquipmentChanged; 

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // เพิ่ม int slotIndex เข้ามาในวงเล็บ
    public void EquipItem(EquipmentData newItem, int slotIndex)
    {
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
}