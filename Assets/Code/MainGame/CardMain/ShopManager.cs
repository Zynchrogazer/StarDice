using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Data")]
    public List<DiceLockCardItem> allPossibleCards; 

    [Header("UI")]
    public GameObject shopPanel;    
    public ShopSlot[] shopSlots;    

    private void Awake()
    {
        Instance = this;
        // ถ้าต้องการให้เริ่มเกมมาแล้วปิดร้านทันที ให้เอา Comment ออก
        // if (shopPanel != null) shopPanel.SetActive(false);
    }

    // ⭐ เพิ่มตรงนี้: ทันทีที่หน้าร้านถูกเปิด (SetActive true) มันจะสุ่มของให้เองทันที
    private void OnEnable()
    {
        // เช็คก่อนว่ามีข้อมูลครบไหม (กัน Error ตอนเริ่มเกม)
        if (allPossibleCards != null && allPossibleCards.Count > 0 && shopSlots != null)
        {
            RefreshShopItems();
        }
    }

    // ----------------------------------------------------------------
    // ส่วนปุ่มกด
    // ----------------------------------------------------------------

    public void OpenShop()
    {
        shopPanel.SetActive(true); 
        // ไม่ต้องเรียก RefreshShopItems() ตรงนี้แล้ว เพราะ OnEnable จะทำงานให้เองอัตโนมัติ
    }

    public void CloseShop()
    {
        // 1. ปิดหน้าต่าง Shop
        shopPanel.SetActive(false);

        // 2. ✅ สำคัญมาก: บอกเกมว่า "ซื้อเสร็จแล้ว จบเทิร์นได้"
        if (GameTurnManager.Instance != null)
        {
            Debug.Log("[Shop] ซื้อของเสร็จสิ้น -> จบเทิร์น");
            GameTurnManager.Instance.RequestEndTurn();
        }
    }

    public void OnRefreshButtonClicked()
    {
        Debug.Log("🛒 กด Refresh: สุ่มรายการสินค้าใหม่");
        RefreshShopItems();
    }

    // ----------------------------------------------------------------
    // Logic การทำงาน
    // ----------------------------------------------------------------

    private void RefreshShopItems()
    {
        Debug.Log("🔄 Shop: กำลังจัดเรียงสินค้า...");

        // 1. สร้าง List สำรอง
        List<DiceLockCardItem> tempDeck = new List<DiceLockCardItem>(allPossibleCards);

        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] == null) continue;

            // ถ้าการ์ดหมดกองแล้ว
            if (tempDeck.Count == 0) 
            {
                // สามารถเลือกที่จะปิด Slot หรือปล่อยว่างไว้
                // shopSlots[i].gameObject.SetActive(false); 
                continue; 
            }

            // 2. สุ่ม
            int randomIndex = Random.Range(0, tempDeck.Count);
            DiceLockCardItem pickedCard = tempDeck[randomIndex];

            // 3. ใส่ข้อมูลลง Slot
            shopSlots[i].Setup(pickedCard);
            // shopSlots[i].gameObject.SetActive(true);

            // 4. ลบออกจากกองสำรอง (จะได้ไม่ซ้ำ)
            tempDeck.RemoveAt(randomIndex);
        }
    }
}