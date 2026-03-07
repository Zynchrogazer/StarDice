using UnityEngine;
using UnityEngine.UI;

public class PlayerCardInventory : MonoBehaviour
{
    public static PlayerCardInventory Instance { get; private set; }

    [Header("UI References (On Main Screen)")]
    public Button useCardButton; // ปุ่มกดใช้การ์ดบนหน้าจอ
    public Image useCardImage;   // รูปการ์ดบนปุ่ม

    [Header("Current State")]
    public DiceLockCardItem currentCard; // การ์ดที่ถืออยู่ (เก็บได้ใบเดียว)

    private void Awake()
    {
        Instance = this;
        UpdateUI(); // เริ่มเกมมาอัปเดต UI ให้ว่างเปล่า
    }

    // ฟังก์ชันสำหรับ "รับการ์ดใหม่" (ถูกเรียกเมื่อซื้อของ)
    public void ObtainCard(DiceLockCardItem newCard)
    {
        currentCard = newCard; // แทนที่การ์ดเก่าทันที (Replace)
        Debug.Log($"ได้รับการ์ด: {newCard.cardName}");
        UpdateUI();
    }

    // ฟังก์ชันสำหรับ "กดใช้การ์ด" (ผูกกับปุ่ม useCardButton)
    // ในสคริปต์ PlayerCardInventory.cs
    public void OnUseCardButtonPress()
    {
        // 🔴 จุดเช็คที่ 1: ปุ่มถูกกดจริงไหม?
        Debug.Log("<color=cyan>[Inventory] 1. ปุ่มถูกกดแล้ว! (OnUseCardButtonPress Called)</color>");

        if (currentCard == null)
        {
            // 🔴 ถ้าเข้าตรงนี้ แสดงว่าตัวแปร currentCard ว่างเปล่า (ซื้อของมาไม่เข้า หรือเผลอลบไปแล้ว)
            Debug.LogError("[Inventory] ❌ ผิดพลาด: ไม่มีไอเทมในมือ (currentCard is NULL)");
            return;
        }

        // 🔴 จุดเช็คที่ 2: มีการ์ดในมือคือใบไหน?
        Debug.Log($"[Inventory] 2. พบการ์ดในมือชื่อ: {currentCard.cardName} เตรียมใช้งาน...");

        // จำค่าไว้ชั่วคราว
        DiceLockCardItem cardToUse = currentCard;

        // ลบออกจากตัว
        currentCard = null;
        UpdateUI(); 

        // 🔴 จุดเช็คที่ 3: กำลังจะสั่งให้การ์ดทำงาน
        Debug.Log("[Inventory] 3. กำลังเรียกคำสั่ง cardToUse.Use()...");
        
        cardToUse.Use();
    }

    // อัปเดตหน้าตาปุ่มตามการ์ดที่มี
    private void UpdateUI()
    {
        if (currentCard != null)
        {
            // มีการ์ด: โชว์รูปและให้กดได้
            useCardImage.sprite = currentCard.cardImage;
            useCardImage.color = Color.white; // ปรับสีให้ชัด
            useCardButton.interactable = true;
        }
        else
        {
            // ไม่มีการ์ด: ซ่อนรูปหรือทำจางๆ และห้ามกด
            useCardImage.sprite = null; 
            useCardImage.color = new Color(1, 1, 1, 0); // โปร่งใส
            useCardButton.interactable = false;
        }
    }
}