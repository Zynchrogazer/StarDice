using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackManager : MonoBehaviour
{
    public List<CardData> allCards;       // ScriptableObject 47 ใบ
    public Button[] packButtons = new Button[4]; // ปุ่มซื้อซอง 4 ปุ่ม

    [Header("Pack Result UI")]
    public GameObject packResultPanel; // Panel แสดงผลการ์ด
    public Image[] cardSlots;          // Image 5 ช่องที่อยู่ใน Panel
    public Button closeButton;         // ปุ่มปิด Panel

    public int cardsPerPack = 5;

    void Start()
    {
        // ... (โค้ดส่วนนี้เหมือนเดิม) ...
        for (int i = 0; i < packButtons.Length; i++)
        {
            int index = i;
            packButtons[i].onClick.AddListener(() => OpenPack(index));
        }

        closeButton.onClick.AddListener(() => packResultPanel.SetActive(false));
        packResultPanel.SetActive(false);
    }

    void OpenPack(int packIndex)
    {
        Debug.Log($"เปิดซอง {packIndex + 1}");
        packResultPanel.SetActive(true);

        // ล้างช่องการ์ดก่อน
        foreach (var slot in cardSlots)
        {
            slot.sprite = null;
            slot.gameObject.SetActive(false);
        }

        // สุ่มการ์ด 5 ใบ
        for (int i = 0; i < cardsPerPack && i < cardSlots.Length; i++)
        {
            CardData card = GetRandomCardForPack(packIndex);
            
            // ถ้าได้ null (ไม่มีการ์ดใน pool นั้นแล้ว) ให้ข้ามไปเลย ช่องนั้นจะว่าง (SetActive false)
            if (card == null) continue;

            // Mark ว่าการ์ดถูกใช้แล้ว
            card.isUsable = true;

            // แสดงผล
            cardSlots[i].sprite = card.icon;
            cardSlots[i].gameObject.SetActive(true);

            Debug.Log($"ได้รับการ์ด: {card.cardName} ({card.rarity})");
        }
    }

    CardData GetRandomCardForPack(int packIndex)
    {
        float rand = Random.value; // สุ่มค่า 0.0 - 1.0
        CardRarity rarity = CardRarity.Common;

        switch (packIndex)
        {
            case 0: 
                // แบบแรก: Normal 70%, Rare 30%
                rarity = (rand <= 0.7f) ? CardRarity.Common : CardRarity.Rare;
                break;

            case 1: 
                // แบบสอง: Normal 30%, Rare 60%, SR 10%
                if (rand <= 0.3f) 
                    rarity = CardRarity.Common; // 0.0 - 0.3 (30%)
                else if (rand <= 0.9f) 
                    rarity = CardRarity.Rare;   // 0.3 - 0.9 (60%)
                else 
                    rarity = CardRarity.SR;     // 0.9 - 1.0 (10%)
                break;

            case 2: 
                // แบบสาม: Rare 60%, SR 30%, SSR 10%
                if (rand <= 0.6f) 
                    rarity = CardRarity.Rare;   // 0.0 - 0.6 (60%)
                else if (rand <= 0.9f) 
                    rarity = CardRarity.SR;     // 0.6 - 0.9 (30%)
                else 
                    rarity = CardRarity.SSR;    // 0.9 - 1.0 (10%)
                break;

            case 3: 
                // แบบสี่: SR 70%, SSR 30%
                rarity = (rand <= 0.7f) ? CardRarity.SR : CardRarity.SSR;
                break;
        }

        // ดึงการ์ดจาก Pool ตาม Rarity ที่สุ่มได้ และต้องยังไม่ถูกใช้ (!isUsable)
        List<CardData> pool = allCards.FindAll(c => !c.isUsable && c.rarity == rarity);

        // **แก้ไขตรงนี้**: ตัดส่วน Fallback ทิ้งทั้งหมด
        // ถ้าไม่มีการ์ดใน Pool นี้เหลือแล้ว ให้ return null ทันที เพื่อให้ช่องนั้น "ว่าง"
        if (pool.Count == 0) return null;

        int index = Random.Range(0, pool.Count);
        return pool[index];
    }
}