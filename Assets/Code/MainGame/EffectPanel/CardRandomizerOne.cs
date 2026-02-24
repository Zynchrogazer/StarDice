using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CardRandomizerOne : MonoBehaviour
{
    public Image cardImage;       // ช่อง Image ที่จะแสดงผล
    public TMP_Text cardNameText; // ช่อง TMP_Text ที่แสดงชื่อการ์ด
    private CardData selectedCard;

    private void Start()
    {
        StartCoroutine(RandomizeCardWithAnimation());
    }

    private IEnumerator RandomizeCardWithAnimation()
    {
        // ดึงการ์ดจาก DeckManager
        CardData[] playerDeck = DeckManager.Instance.cardUse;

        // เอาเฉพาะการ์ดที่ไม่ null
        List<CardData> tempList = new List<CardData>();
        foreach (var card in playerDeck)
        {
            if (card != null)
                tempList.Add(card);
        }

        if (tempList.Count == 0)
        {
            Debug.LogWarning("ไม่มีการ์ดใน Deck ให้สุ่ม!");
            yield break;
        }

        // --- Animation สุ่มภาพ 10 รอบ ---
        for (int i = 0; i < 10; i++)
        {
            int randIndex = Random.Range(0, tempList.Count);
            CardData tempCard = tempList[randIndex];

            if (cardImage != null && tempCard.icon != null)
                cardImage.sprite = tempCard.icon;

            if (cardNameText != null)
                cardNameText.text = tempCard.cardName;

            yield return new WaitForSeconds(0.1f); // หน่วงเวลาแต่ละรอบ (เร็ว-ช้า ปรับได้)
        }

        // --- เลือกการ์ดจริง ---
        int finalIndex = Random.Range(0, tempList.Count);
        selectedCard = tempList[finalIndex];

        if (cardImage != null && selectedCard.icon != null)
            cardImage.sprite = selectedCard.icon;

        if (cardNameText != null)
            cardNameText.text = selectedCard.cardName;

        Debug.Log("การ์ดที่สุ่มได้จริง: " + selectedCard.cardName);
    }
}
