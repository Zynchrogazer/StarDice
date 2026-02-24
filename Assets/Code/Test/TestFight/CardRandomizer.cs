using UnityEngine;
using System.Collections.Generic;

public class CardRandomizer : MonoBehaviour
{
    public List<CardData> selectedCards = new List<CardData>();

    public void RandomizeCards()
    {
        selectedCards.Clear();

        // ดึงข้อมูลจาก DeckManager
        CardData[] playerDeck = DeckManager.Instance.cardUse;

        // สร้าง tempList จาก cardUse ที่ไม่เป็น null
        List<CardData> tempList = new List<CardData>();
        foreach (var card in playerDeck)
        {
            if (card != null)
                tempList.Add(card);
        }

        // สุ่มการ์ดจาก tempList
        for (int i = 0; i < 4 && tempList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            selectedCards.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex); // ลบออกเพื่อไม่ให้ซ้ำ
        }

        // แสดงผล
        Debug.Log("การ์ดที่สุ่มได้:");
        foreach (var card in selectedCards)
        {
            Debug.Log(card.cardName);
        }

        // ส่งต่อไป GameData
        GameData.Instance.selectedCards = new List<CardData>(selectedCards);
    }
}
