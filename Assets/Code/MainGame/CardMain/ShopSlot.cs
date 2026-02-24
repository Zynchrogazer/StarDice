using UnityEngine;
using UnityEngine.UI;
using TMPro; // อย่าลืมบรรทัดนี้! เพื่อเรียกใช้ TextMeshPro

public class ShopSlot : MonoBehaviour
{
    [Header("UI Components")]
    public Image cardDisplayImage;       // รูปการ์ด
    public TextMeshProUGUI cardNameText; // 👈 เพิ่มตัวนี้: เอาไว้โชว์ชื่อการ์ด
    public TextMeshProUGUI priceText; // ✅ เพิ่มตัวนี้: ข้อความราคา
    public Button buyButton;             // ปุ่มซื้อ

    private DiceLockCardItem cardInThisSlot;

    public void Setup(DiceLockCardItem card)
    {
        // เช็คก่อนว่าการ์ดว่างไหม
        if (card == null) return;

        cardInThisSlot = card;

        // 1. ตั้งค่ารูป
        if (cardDisplayImage != null)
        {
            cardDisplayImage.sprite = card.cardImage;
            cardDisplayImage.color = Color.white;
        }

        // 2. ตั้งค่าชื่อ (ส่วนที่เพิ่มมาใหม่)
        if (cardNameText != null)
        {
            cardNameText.text = card.cardName;
        }
        else
        {
            // Debug เตือนกันลืม
            Debug.LogWarning($"[ShopSlot] {name}: ยังไม่ได้ลาก TextMeshPro ใส่ช่อง Card Name Text");
        }

        // 3. ✅ เปลี่ยนราคา! (ตรงนี้แหละที่ต้องการ)
        if (priceText != null)
        {
            priceText.text = card.price.ToString(); 
            // หรือจะใส่หน่วยเงินด้วยก็ได้ เช่น: newItem.price.ToString() + " G";
        }
        
        // 4. ตั้งค่าปุ่ม
        if (buyButton != null)
        {
            buyButton.interactable = true;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyThisCard);
        }
    }

    void BuyThisCard()
    {
        if (cardInThisSlot != null)
        {
            PlayerCardInventory.Instance.ObtainCard(cardInThisSlot);
            
            // (Optional) ถ้าอยากให้ซื้อแล้วปุ่มหายไป หรือขึ้นว่า Sold Out ก็เขียนเพิ่มตรงนี้
            // buyButton.interactable = false;
        }
    }
}