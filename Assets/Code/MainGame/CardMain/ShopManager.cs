using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Data")]
    public List<DiceLockCardItem> allPossibleCards; 

    [Header("UI")]
    public GameObject shopPanel;    
    public ShopSlot[] shopSlots;    
    [SerializeField] private TMP_Text shopMoneyText;
    [SerializeField] private string moneyPrefix = "Money: ";

    private void Awake()
    {
        Instance = this;
        TryAutoAssignMoneyText();
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
        RefreshMoneyText();
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

    public bool TryBuyCard(DiceLockCardItem card)
    {
        if (card == null) return false;

        if (GameTurnManager.CurrentPlayer == null)
        {
            Debug.LogError("[Shop] CurrentPlayer ไม่พร้อมใช้งาน");
            return false;
        }

        if (PlayerCardInventory.Instance == null)
        {
            Debug.LogError("[Shop] ไม่พบ PlayerCardInventory.Instance");
            return false;
        }

        PlayerState buyer = GameTurnManager.CurrentPlayer;
        if (buyer.PlayerMoney < card.price)
        {
            Debug.Log($"[Shop] เงินไม่พอสำหรับ {card.cardName} (ต้องการ {card.price}, มี {buyer.PlayerMoney})");
            return false;
        }

        buyer.PlayerMoney -= card.price;
        if (GameData.Instance?.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetMoney(buyer.PlayerMoney);
        }
        PlayerCardInventory.Instance.ObtainCard(card);
        RefreshMoneyText();
        Debug.Log($"[Shop] ซื้อ {card.cardName} สำเร็จ เหลือเงิน {buyer.PlayerMoney}");
        return true;
    }

    private void TryAutoAssignMoneyText()
    {
        if (shopMoneyText != null || shopPanel == null) return;

        TMP_Text[] texts = shopPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (var txt in texts)
        {
            if (txt == null) continue;

            string objectName = txt.name.ToLower();
            string textValue = txt.text.ToLower();

            if (objectName.Contains("money") || textValue.Contains("money") || textValue.Contains("coin"))
            {
                shopMoneyText = txt;
                break;
            }
        }
    }

    private void RefreshMoneyText()
    {
        TryAutoAssignMoneyText();
        if (shopMoneyText == null || GameTurnManager.CurrentPlayer == null) return;

        shopMoneyText.text = $"{moneyPrefix}{GameTurnManager.CurrentPlayer.PlayerMoney}";
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
