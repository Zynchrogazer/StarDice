using UnityEngine;

// ประกาศประเภทการ์ดไว้ข้างบนนี้ (จะได้ไม่ต้องสร้างไฟล์แยก)
public enum DiceCardType
{
    LockNumber, // ล็อคเลข
    Multiplier, // คูณ (x2, x3, x4)
    Warp        // วาร์ป
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Dice Lock Card")]
public class DiceLockCardItem : ScriptableObject
{
    [Header("Card Info")]
    public string cardName; 
    [TextArea] public string description; 
    public Sprite cardImage; 

    [Header("Settings")]
    [Header("Shop Settings")]
    public int price = 100; // ตั้งราคาเริ่มต้นไว้สักหน่อย
     
     // เลือกประเภทการ์ดตรงนี้
    public DiceCardType cardType;

    [Tooltip("ถ้าเป็น Lock ให้ใส่เลข 1-6 \nถ้าเป็น Multiplier ให้ใส่ตัวคูณ (เช่น 2, 3)")]
    [Range(1, 10)] 
    public int lockNumber = 1; // ใช้ตัวแปรเดิม จะได้ไม่ต้องแก้เยอะ

    public void Use()
    {
        Debug.Log($"<color=yellow>[CardItem] กดใช้การ์ด: {cardName} (Type: {cardType})</color>");

        if (DiceRollerFromPNG.Instance == null)
        {
            Debug.LogError("[CardItem] ❌ ไม่เจอ DiceRollerFromPNG ในฉาก!");
            return;
        }

        // ตรวจสอบประเภทการ์ด แล้วสั่งงานให้ถูกต้อง
        switch (cardType)
        {
            case DiceCardType.LockNumber:
                // แบบเดิม: ล็อคเลข
                Debug.Log($"[CardItem] สั่งล็อคเลขเป็น: {lockNumber}");
                DiceRollerFromPNG.Instance.RollDiceWithResult(lockNumber);
                break;

            case DiceCardType.Multiplier:
                // แบบใหม่: ไม่ทอยทันที แต่ส่งค่าไปรอไว้
                Debug.Log($"[CardItem] เปิดใช้งานสถานะคูณ: x{lockNumber}");
                
                // เรียกฟังก์ชันใหม่ที่เราเพิ่งสร้าง
                DiceRollerFromPNG.Instance.SetPendingMultiplier(lockNumber);
                break;

            case DiceCardType.Warp:
                Debug.Log("ใช้การ์ด Warp: กรุณาเลือกช่องบนหน้าจอ...");
                
                // เรียกใช้ฟังก์ชันที่เราเพิ่งเขียนใน RouteManager
                if (RouteManager.Instance != null)
                {
                    RouteManager.Instance.StartWarpSelection();
                }
                break;

        }
    }
}