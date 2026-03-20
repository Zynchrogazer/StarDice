using System.Collections.Generic;
using UnityEngine;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("ใส่ EquipmentData ทั้ง 39 อันที่นี่")]
    public List<EquipmentData> allEquipmentList; 

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

        foreach (var item in allEquipmentList)
        {
            if (item != null && !equipmentMap.ContainsKey(item.itemID))
            {
                equipmentMap.Add(item.itemID, item);
            }
        }

        LoadOwnershipStates();
    }

    // --- แก้ไข: เปลี่ยนจาก void เป็น bool เพื่อให้บอกได้ว่า "ได้ของใหม่" หรือ "ได้ของซ้ำ" ---
    // รับค่า duplicateReward เข้ามาด้วย เผื่ออยากให้ไอเท็มแต่ละชิ้นให้เงินคืนไม่เท่ากัน (ค่าเริ่มต้น 150)
    public bool UnlockItem(ItemID idToUnlock, int duplicateReward = 300)
    {
        if (equipmentMap.ContainsKey(idToUnlock))
        {
            EquipmentData item = equipmentMap[idToUnlock];
            
            // 1. เช็คว่าถ้ามีไอเท็มชิ้นนี้อยู่แล้ว
            if (item.isOwned)
            {
                Debug.Log($"[EquipmentManager] ได้ไอเท็มซ้ำ: {item.itemName}! เปลี่ยนเป็นเงิน {duplicateReward} แทน");
                GiveCreditToPlayer(duplicateReward); // เรียกใช้ฟังก์ชันแจกเงิน
                return false; // คืนค่า false บ่งบอกว่าได้ของซ้ำ
            }

            // 2. ถ้ายังไม่มี ให้ตั้งค่าเป็น True
            item.isOwned = true;
            PlayerPrefs.SetInt(OwnershipPrefKeyPrefix + idToUnlock, 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[EquipmentManager] ปลดล็อกไอเท็มใหม่สำเร็จ: {item.itemName} ({item.itemID})");
            return true; // คืนค่า true บ่งบอกว่าได้ของใหม่
        }
        else
        {
            Debug.LogWarning($"[EquipmentManager] ไม่พบไอเท็ม ID: {idToUnlock} ในระบบ");
            return false;
        }
    }

    // --- ฟังก์ชันใหม่: ค้นหาตัวผู้เล่นหลัก และแจกเงิน ---
    private void GiveCreditToPlayer(int amount)
    {
        // อิงจากโค้ดเดิมของคุณ ซิงค์ข้อมูลกับ GameData ก่อนเพื่อความชัวร์เวลาเปลี่ยนด่าน
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.AddCredit(amount);
        }

        // ค้นหา PlayerState ในฉากปัจจุบัน (เผื่อไว้ในกรณีที่ใช้แค่ใน Scene ไม่ได้พึ่ง GameData)
        PlayerState[] players = FindObjectsOfType<PlayerState>(true);
        foreach (PlayerState player in players)
        {
            // หาคนที่ไม่ใช่ AI
            if (!player.isAI)
            {
                player.PlayerCredit += amount;
                Debug.Log($"[EquipmentManager] โอนเงินชดเชย +{amount} เข้ากระเป๋า {player.gameObject.name} สำเร็จ (ตอนนี้มี {player.PlayerCredit})");
                return; 
            }
        }

        Debug.LogWarning("[EquipmentManager] หา PlayerState ผู้เล่นหลักไม่เจอ! เงินชดเชยไม่ได้ถูกเพิ่ม");
    }

    public bool CheckIfOwned(ItemID id)
    {
        if (equipmentMap.ContainsKey(id))
        {
            return equipmentMap[id].isOwned;
        }
        return false;
    }
    
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
        // 1. ลบไฟล์เซฟทิ้ง
        foreach (ItemID id in Enum.GetValues(typeof(ItemID)))
        {
            if (id == ItemID.None) continue;
            PlayerPrefs.DeleteKey(OwnershipPrefKeyPrefix + id);
        }

        // 2. ล้างค่าใน Instance (ถ้ามี)
        if (Instance != null)
        {
            foreach (var item in Instance.allEquipmentList)
            {
                if (item == null) continue;
                item.isOwned = false;
            }
        }

        // 3. 🟢 เพิ่มประกันความเสี่ยง: กวาดล้าง ScriptableObject ทั้งโปรเจกต์
        // (เผื่อถูกเรียกตอนที่ Instance ยังไม่เกิด เช่น หน้า Main Menu)
        EquipmentData[] allData = Resources.FindObjectsOfTypeAll<EquipmentData>();
        foreach (var item in allData)
        {
            if (item != null) item.isOwned = false;
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
            else
            {
                // 🟢 เพิ่ม else เข้ามา: ถ้าไม่มีไฟล์เซฟ (เช่น เพิ่งกด New game) ต้องริบของคืน!
                pair.Value.isOwned = false; 
            }
        }
    }

    [ContextMenu("Reset All Equipment Save")]
    public void ResetAllEquipmentSave()
    {
        foreach (var item in allEquipmentList)
        {
            if (item != null)
            {
                item.isOwned = false; 
                PlayerPrefs.DeleteKey(OwnershipPrefKeyPrefix + item.itemID); 
            }
        }
        
        PlayerPrefs.Save(); 
        Debug.Log("<color=red>รีเซ็ตไอเท็มและลบเซฟเก่าทิ้งทั้งหมดเรียบร้อยแล้ว!</color>");
    }
}