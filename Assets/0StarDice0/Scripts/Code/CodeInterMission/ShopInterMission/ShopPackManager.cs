using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackManager : MonoBehaviour
{
    public List<CardData> allCards;       // ScriptableObject 47 ใบ
    public Button[] packButtons = new Button[4]; // ปุ่มซื้อซอง 4 ปุ่ม

    [Header("Pack Prices")]
    [Tooltip("ราคาแต่ละซองตามลำดับปุ่ม หากใส่ไม่ครบจะใช้ 100 เป็นค่าเริ่มต้น")]
    public int[] packPrices = new int[] { 60, 100, 150, 220 };

    [Header("Pack Result UI")]
    public GameObject packResultPanel; // Panel แสดงผลการ์ด
    public Image[] cardSlots;          // Image 5 ช่องที่อยู่ใน Panel
    public Button closeButton;         // ปุ่มปิด Panel

    public int cardsPerPack = 5;

    void Start()
    {
        // ปุ่มซื้อซอง
        for (int i = 0; i < packButtons.Length; i++)
        {
            int index = i;
            packButtons[i].onClick.AddListener(() => OpenPack(index));
        }

        // ปุ่มปิด Panel
        closeButton.onClick.AddListener(() => packResultPanel.SetActive(false));

        packResultPanel.SetActive(false); // ปิดไว้ก่อน
    }

    void OpenPack(int packIndex)
    {
        if (GameData.Instance == null || GameData.Instance.selectedPlayer == null)
        {
            Debug.LogWarning("เปิดซองไม่สำเร็จ: ไม่พบข้อมูลผู้เล่น");
            return;
        }

        int price = GetPackPrice(packIndex);
        if (!GameData.Instance.selectedPlayer.TrySpendMoney(price))
        {
            Debug.Log($"เงินไม่พอสำหรับเปิดซอง {packIndex + 1}. ต้องใช้ {price} แต่มี {GameData.Instance.selectedPlayer.Money}");
            return;
        }

        Debug.Log($"เปิดซอง {packIndex + 1} (จ่าย {price}, เงินคงเหลือ {GameData.Instance.selectedPlayer.Money})");

        // เปิด Panel
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
            if (card == null) continue;

            card.isUsable = true;

            // แสดงผลในช่อง
            cardSlots[i].sprite = card.icon;
            cardSlots[i].gameObject.SetActive(true);

            Debug.Log($"ได้รับการ์ด: {card.cardName} ({card.rarity})");
        }
    }

    int GetPackPrice(int packIndex)
    {
        if (packPrices != null && packIndex >= 0 && packIndex < packPrices.Length)
        {
            return Mathf.Max(0, packPrices[packIndex]);
        }

        return 100;
    }

    CardData GetRandomCardForPack(int packIndex)
    {
        float rand = Random.value;
        CardRarity rarity = CardRarity.Common;

        switch (packIndex)
        {
            case 0: rarity = (rand <= 0.7f) ? CardRarity.Common : CardRarity.Rare; break;
            case 1: if (rand <= 0.5f) rarity = CardRarity.Common;
                    else if (rand <= 0.8f) rarity = CardRarity.Rare;
                    else rarity = CardRarity.SR; break;
            case 2: if (rand <= 0.5f) rarity = CardRarity.Rare;
                    else if (rand <= 0.9f) rarity = CardRarity.SR;
                    else rarity = CardRarity.SSR; break;
            case 3: rarity = (rand <= 0.6f) ? CardRarity.SR : CardRarity.SSR; break;
        }

        List<CardData> pool = allCards.FindAll(c => !c.isUsable && c.rarity == rarity);

        if (pool.Count == 0)
            pool = allCards.FindAll(c => !c.isUsable && c.rarity == CardRarity.Common);

        if (pool.Count == 0)
            pool = allCards.FindAll(c => !c.isUsable);

        if (pool.Count == 0) return null;

        int index = Random.Range(0, pool.Count);
        return pool[index];
    }
}
