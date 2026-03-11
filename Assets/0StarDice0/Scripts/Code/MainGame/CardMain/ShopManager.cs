using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    public List<DiceLockCardItem> allPossibleCards; 

    [Header("UI")]
    public GameObject shopPanel;    
    public ShopSlot[] shopSlots;    
    [SerializeField] private TMP_Text shopCreditText;
    [SerializeField] private string creditPrefix = "Credit: ";

    private void Awake()
    {
        if (FindObjectsOfType<ShopManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        TryAutoAssignCreditText();
        // ถ้าต้องการให้เริ่มเกมมาแล้วปิดร้านทันที ให้เอา Comment ออก
        // if (shopPanel != null) shopPanel.SetActive(false);
    }

    // ⭐ เพิ่มตรงนี้: ทันทีที่หน้าร้านถูกเปิด (SetActive true) มันจะสุ่มของให้เองทันที
    private void OnEnable()
    {
        if (shopPanel != null && shopPanel.activeInHierarchy)
        {
            RefreshShopItems();
        }
    }

    // ----------------------------------------------------------------
    // ส่วนปุ่มกด
    // ----------------------------------------------------------------

    public void OpenShop()
    {
        HandleShopOpened();
    }

    public void HandleShopOpened()
    {
        if (shopPanel == null)
        {
            Debug.LogError("[Shop] shopPanel ยังไม่ได้ตั้งค่าใน ShopManager");
            return;
        }

        shopPanel.SetActive(true);
        RefreshShopItems();
        RefreshCreditText();
    }

    public void CloseShop()
    {
        // 1. ปิดหน้าต่าง Shop
        shopPanel.SetActive(false);

        // 2. ✅ สำคัญมาก: บอกเกมว่า "ซื้อเสร็จแล้ว จบเทิร์นได้"
        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            Debug.Log("[Shop] ซื้อของเสร็จสิ้น -> จบเทิร์น");
            gameTurnManager.RequestEndTurn();
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

    public bool TryBuyCard(DiceLockCardItem card)
    {
        if (card == null) return false;

        if (GameTurnManager.CurrentPlayer == null)
        {
            Debug.LogError("[Shop] CurrentPlayer ไม่พร้อมใช้งาน");
            return false;
        }

        PlayerCardInventory playerCardInventory = FindObjectOfType<PlayerCardInventory>();
        if (playerCardInventory == null)
        {
            Debug.LogError("[Shop] ไม่พบ PlayerCardInventory ใน scene");
            return false;
        }

        PlayerState buyer = GameTurnManager.CurrentPlayer;
        if (buyer.PlayerCredit < card.price)
        {
            Debug.Log($"[Shop] เครดิตไม่พอสำหรับ {card.cardName} (ต้องการ {card.price}, มี {buyer.PlayerCredit})");
            return false;
        }

        buyer.PlayerCredit -= card.price;
        if (GameData.Instance?.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetCredit(buyer.PlayerCredit);
        }
        playerCardInventory.ObtainCard(card);
        RefreshCreditText();
        Debug.Log($"[Shop] ซื้อ {card.cardName} สำเร็จ เหลือเครดิต {buyer.PlayerCredit}");
        return true;
    }

    private void TryAutoAssignCreditText()
    {
        if (shopCreditText != null || shopPanel == null) return;

        TMP_Text[] texts = shopPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (var txt in texts)
        {
            if (txt == null) continue;

            string objectName = txt.name.ToLower();
            string textValue = txt.text.ToLower();

            if (objectName.Contains("credit") || textValue.Contains("credit") || textValue.Contains("coin"))
            {
                shopCreditText = txt;
                break;
            }
        }
    }

    private void RefreshCreditText()
    {
        TryAutoAssignCreditText();
        if (shopCreditText == null || GameTurnManager.CurrentPlayer == null) return;

        shopCreditText.text = $"{creditPrefix}{GameTurnManager.CurrentPlayer.PlayerCredit}";
    }

    private void RefreshShopItems()
    {
        if (allPossibleCards == null || shopSlots == null)
        {
            Debug.LogWarning("[Shop] ยังตั้งค่า allPossibleCards หรือ shopSlots ไม่ครบ");
            return;
        }

        Debug.Log("🔄 Shop: กำลังจัดเรียงสินค้า...");

        // 1. สร้าง List สำรอง
        List<DiceLockCardItem> tempDeck = new List<DiceLockCardItem>(allPossibleCards);

        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] == null) continue;

            // ถ้าการ์ดหมดกองแล้ว
            if (tempDeck.Count == 0) 
            {
                shopSlots[i].ClearSlot();
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
