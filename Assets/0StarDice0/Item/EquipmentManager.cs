using System.Collections.Generic;
using UnityEngine;
using System;
public class EquipmentManager : MonoBehaviour
{
    // สร้าง Singleton เพื่อให้เรียกใช้ได้ง่ายจากที่ไหนก็ได้
    public static EquipmentManager Instance;

    [Header("ใส่ EquipmentData ทั้ง 39 อันที่นี่")]
    public List<EquipmentData> allEquipmentList; 

    // ตัวแปร Dictionary เพื่อให้ค้นหาไอเท็มไวขึ้น (ไม่ต้องวนลูปทุกครั้ง)
    private Dictionary<ItemID, EquipmentData> equipmentMap = new Dictionary<ItemID, EquipmentData>();
    private const string OwnershipPrefKeyPrefix = "EquipmentOwned_";

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

        // แปลง List เป็น Dictionary ตอนเริ่มเกมเพื่อให้ค้นหาไว
        foreach (var item in allEquipmentList)
        {
            if (item != null && !equipmentMap.ContainsKey(item.itemID))
            {
                equipmentMap.Add(item.itemID, item);
            }
        }

        LoadOwnershipStates();
    }

    // --- ฟังก์ชันที่คุณต้องการ: สั่งให้ isOwned เป็น true ---
    public void UnlockItem(ItemID idToUnlock)
    {
        if (equipmentMap.ContainsKey(idToUnlock))
        {
            EquipmentData item = equipmentMap[idToUnlock];
            
            // ตั้งค่าเป็น True
            item.isOwned = true;
            PlayerPrefs.SetInt(OwnershipPrefKeyPrefix + idToUnlock, 1);
            PlayerPrefs.Save();
            
            Debug.Log($"ปลดล็อกไอเท็มสำเร็จ: {item.itemName} ({item.itemID})");

            // แนะนำ: ควรมีระบบ Save ข้อมูลตรงนี้ด้วย
            // SaveSystem.SaveOwnership(idToUnlock); 
        }
        else
        {
            Debug.LogWarning($"ไม่พบไอเท็ม ID: {idToUnlock} ในระบบ");
        }
    }

    // ฟังก์ชันเสริม: เช็คว่ามีไอเท็มนี้หรือยัง
    public bool CheckIfOwned(ItemID id)
    {
        if (equipmentMap.ContainsKey(id))
        {
            return equipmentMap[id].isOwned;
        }
        return false;
    }
    
    // ฟังก์ชันสำหรับ Debug: กดปุ่มเรียกใช้
    [ContextMenu("Test Unlock Sword")]
    public void TestUnlockSword()
    {
        UnlockItem(ItemID.Sword);
    }

    public EquipmentData GetEquipmentById(ItemID id)
    {
        equipmentMap.TryGetValue(id, out var data);
        return data;
    }

    public static void ClearSavedOwnershipStates()
    {
        foreach (ItemID id in Enum.GetValues(typeof(ItemID)))
        {
            if (id == ItemID.None)
            {
                continue;
            }

            PlayerPrefs.DeleteKey(OwnershipPrefKeyPrefix + id);
        }

        if (Instance != null)
        {
            foreach (var item in Instance.allEquipmentList)
            {
                if (item == null)
                {
                    continue;
                }

                item.isOwned = false;
            }
        }
    }

    private void LoadOwnershipStates()
    {
        foreach (var pair in equipmentMap)
        {
            string key = OwnershipPrefKeyPrefix + pair.Key;
            if (PlayerPrefs.HasKey(key))
            {
                pair.Value.isOwned = PlayerPrefs.GetInt(key) == 1;
            }
        }
    }
}
