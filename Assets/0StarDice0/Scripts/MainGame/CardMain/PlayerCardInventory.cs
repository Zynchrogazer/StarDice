using UnityEngine;
using UnityEngine.UI;

public class PlayerCardInventory : MonoBehaviour
{
    [Header("UI References (On Main Screen)")]
    public Button useCardButton; 
    public Image useCardImage;   

    [Header("Current State")]
    public DiceLockCardItem currentCard; 
    
    // 🟢 เพิ่มตัวแปรเช็คเทิร์น (ตัวจัดการเทิร์นของคุณจะต้องมาสั่งเปลี่ยนค่านี้)
    public bool isPlayerTurn = false; 

    private void Awake()
    {
        // ... (โค้ดเช็คตัวซ้ำเหมือนเดิม) ...
        UpdateUI(); 
    }

    public void ObtainCard(DiceLockCardItem newCard)
    {
        currentCard = newCard; 
        Debug.Log($"ได้รับการ์ด: {newCard.cardName}");
        UpdateUI();
    }

    // 🟢 เพิ่มฟังก์ชันให้ระบบจัดการเทิร์น (BoardManager/GameManager) เรียกใช้
    public void UpdateTurnState(bool isMyTurn)
    {
        isPlayerTurn = isMyTurn;
        UpdateUI(); // อัปเดตปุ่มทันทีที่เปลี่ยนเทิร์น
    }

    public void OnUseCardButtonPress()
    {
        // 🛑 ป้องกันชั้นที่ 1: เช็คว่าเป็นเทิร์นของเราไหม ถ้าไม่ใช่ให้หยุดการทำงานทันที
        if (!isPlayerTurn)
        {
            Debug.LogWarning("[Inventory] ไม่สามารถใช้การ์ดได้ เพราะไม่ใช่เทิร์นของผู้เล่น!");
            return;
        }

        Debug.Log("<color=cyan>[Inventory] 1. ปุ่มถูกกดแล้ว!</color>");

        if (currentCard == null)
        {
            Debug.LogError("[Inventory] ❌ ผิดพลาด: ไม่มีไอเทมในมือ");
            return;
        }

        Debug.Log($"[Inventory] 2. พบการ์ดในมือชื่อ: {currentCard.cardName} เตรียมใช้งาน...");

        DiceLockCardItem cardToUse = currentCard;
        currentCard = null;
        UpdateUI(); 

        Debug.Log("[Inventory] 3. กำลังเรียกคำสั่ง cardToUse.Use()...");
        cardToUse.Use();
    }

    private void UpdateUI()
    {
        // 🛑 ป้องกันชั้นที่ 2: ปุ่มจะกดได้ก็ต่อเมื่อ "มีการ์ด" และ "เป็นเทิร์นของเรา"
        if (currentCard != null && isPlayerTurn)
        {
            useCardImage.sprite = currentCard.cardImage;
            useCardImage.color = Color.white; 
            useCardButton.interactable = true; // เปิดให้กดได้
        }
        else
        {
            // ถ้าไม่มีการ์ด "หรือ" ไม่ใช่เทิร์นของเรา
            if (currentCard != null) 
            {
                // มีการ์ดแต่ไม่ใช่เทิร์น โชว์รูปการ์ดปกติ แต่ล็อกปุ่ม
                useCardImage.sprite = currentCard.cardImage;
                useCardImage.color = new Color(0.5f, 0.5f, 0.5f, 1); // ทำให้รูปมืดลงนิดหน่อย (สีเทา)
            }
            else
            {
                // ไม่มีการ์ดเลย ซ่อนรูป
                useCardImage.sprite = null; 
                useCardImage.color = new Color(1, 1, 1, 0); 
            }
            
            useCardButton.interactable = false; // ล็อกปุ่ม ห้ามกด
        }
    }

    private void OnEnable()
    {
        // 1. ลองหา GameTurnManager ในฉาก 
        if (GameTurnManager.TryGet(out var turnManager))
        {
            // 2. ถ้าเจอ ให้สมัครรับข้อมูลเมื่อมีการเปลี่ยนเทิร์น
            turnManager.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDisable()
    {
        // ยกเลิกการรับข้อมูลเมื่อสคริปต์นี้ถูกปิดหรือทำลาย เพื่อป้องกันบั๊ก
        if (GameTurnManager.TryGet(out var turnManager))
        {
            turnManager.OnTurnChanged -= HandleTurnChanged;
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อ GameTurnManager ประกาศเปลี่ยนเทิร์น
    private void HandleTurnChanged(bool isAI)
    {
        // ถ้าเป็นเทิร์นของ AI (isAI == true) หมายความว่า "ไม่ใช่เทิร์นของผู้เล่น"
        // ถ้าเป็นเทิร์นของคน (isAI == false) หมายความว่า "เป็นเทิร์นของผู้เล่น"
        UpdateTurnState(!isAI); 
    }
}