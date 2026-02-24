using UnityEngine;
using System.Collections.Generic;

// ย้าย Class นี้ออกมาไว้นอก PlayerState เพื่อให้ไฟล์อื่นเรียกใช้ได้ง่าย
[System.Serializable]
public class InventoryCard
{
    public string cardName;
    public int quantity;

    public InventoryCard(string name, int qty = 1)
    {
        cardName = name;
        quantity = qty;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // รายการของในกระเป๋า
    public List<InventoryCard> items = new List<InventoryCard>();

    // รายการการ์ดที่เลือกมา (Deck)
    public List<CardData> selectedDeck = new List<CardData>();

    private void Awake()
    {
        // ทำ Singleton เหมือนกัน เพราะ Inventory ก็ควรติดตัวผู้เล่นไปตลอด
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- Logic การจัดการของ (ย้ายมาจาก PlayerState) ---

    public void AddCard(string cardName, int amount = 1)
    {
        InventoryCard existingCard = items.Find(x => x.cardName == cardName);

        if (existingCard != null)
        {
            existingCard.quantity += amount;
        }
        else
        {
            items.Add(new InventoryCard(cardName, amount));
        }
        Debug.Log($"🎒 [Inventory] Added {cardName} x{amount}");
    }

    public bool UseCard(string cardName)
    {
        InventoryCard existingCard = items.Find(x => x.cardName == cardName);

        if (existingCard != null && existingCard.quantity > 0)
        {
            existingCard.quantity--;
            if (existingCard.quantity <= 0) items.Remove(existingCard);

            Debug.Log($"🎒 [Inventory] Used {cardName}");
            return true;
        }
        return false;
    }

    public void SetSelectedDeck(List<CardData> cards)
    {
        selectedDeck = new List<CardData>(cards);
    }
}