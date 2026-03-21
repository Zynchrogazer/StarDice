using UnityEngine;
using System.Collections.Generic;

public class CardRandomizer : MonoBehaviour
{
    public List<CardData> selectedCards = new List<CardData>();

    public void RandomizeCards()
    {
        selectedCards = BattleCardHandResolver.GetOpeningHand(4);

        // แสดงผล
        Debug.Log("การ์ดที่สุ่มได้:");
        foreach (var card in selectedCards)
        {
            Debug.Log(card.cardName);
        }

        // ส่งต่อไป GameData
        if (GameData.Instance != null)
        {
            GameData.Instance.selectedCards = new List<CardData>(selectedCards);
        }
    }
}
