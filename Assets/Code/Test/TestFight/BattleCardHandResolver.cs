using System.Collections.Generic;
using UnityEngine;

public static class BattleCardHandResolver
{
    public static List<CardData> GetOpeningHand(int handSize)
    {
        List<CardData> sourceCards = new List<CardData>();

        if (GameData.Instance != null && GameData.Instance.selectedCards != null && GameData.Instance.selectedCards.Count > 0)
        {
            sourceCards.AddRange(GameData.Instance.selectedCards);
        }
        else if (DeckManager.Instance != null && DeckManager.Instance.cardUse != null)
        {
            foreach (var card in DeckManager.Instance.cardUse)
            {
                if (card != null)
                {
                    sourceCards.Add(card);
                }
            }
        }

        // ป้องกันการ์ดซ้ำจาก source
        HashSet<CardData> seen = new HashSet<CardData>();
        List<CardData> uniqueCards = new List<CardData>();
        foreach (var card in sourceCards)
        {
            if (card == null || !seen.Add(card)) continue;
            uniqueCards.Add(card);
        }

        // สุ่มลำดับ (Fisher-Yates)
        for (int i = uniqueCards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (uniqueCards[i], uniqueCards[j]) = (uniqueCards[j], uniqueCards[i]);
        }

        if (handSize < uniqueCards.Count)
        {
            uniqueCards = uniqueCards.GetRange(0, handSize);
        }

        Debug.Log($"[BattleCardHandResolver] Opening hand {uniqueCards.Count} cards");
        return uniqueCards;
    }
}
