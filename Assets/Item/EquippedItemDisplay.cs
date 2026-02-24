using UnityEngine;
using UnityEngine.UI; // *สำคัญ* ต้องมีบรรทัดนี้เพื่อคุม Image/Button

public class EquippedItemDisplay : MonoBehaviour
{
    [Header("ลาก Button หรือ Image ที่อยากให้เปลี่ยนรูปมาใส่")]
    public Image slot1Image; // รูปสำหรับช่องที่ 1 (เช่น ปุ่มโจมตี)
    public Image slot2Image; // รูปสำหรับช่องที่ 2 (เช่น ปุ่มป้องกัน)
    
    [Header("ตั้งค่ารูปเปล่า (ตอนไม่ใส่ของ)")]
    public Sprite emptySprite; // ใส่รูปว่างๆ หรือ Transparent ไว้

    void Start()
    {
        // เริ่ม Scene มาให้อัปเดตทันที (เผื่อข้าม Scene มา)
        UpdateUI();
    }

    // ฟังก์ชันสำหรับสั่งให้เปลี่ยนรูป
    public void UpdateUI()
    {
        // 1. เช็คว่า Manager ยังอยู่มั้ย
        if (PlayerDataManager.Instance == null) return;

        EquipmentData[] items = PlayerDataManager.Instance.equippedItems;

        // --- อัปเดตช่องที่ 1 ---
        if (slot1Image != null)
        {
            if (items[0] != null)
                slot1Image.sprite = items[0].icon; // เปลี่ยนเป็นรูปไอเท็ม
            else
                slot1Image.sprite = emptySprite;   // เปลี่ยนเป็นรูปว่าง
        }

        // --- อัปเดตช่องที่ 2 ---
        if (slot2Image != null)
        {
            if (items[1] != null)
                slot2Image.sprite = items[1].icon;
            else
                slot2Image.sprite = emptySprite;
        }
    }
}