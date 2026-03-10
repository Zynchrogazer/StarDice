using System;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    public int cardId;
    public GameObject front;
    public GameObject back;
    public bool isMatched = false;

    private TMP_Text frontText;
    private Action<Card> onCardSelected;
    private Func<bool> canSelectCard;

    public void Setup(int id)
    {
        cardId = id;

        frontText = front.GetComponentInChildren<TMP_Text>();
        if (frontText != null)
        {
            frontText.text = id.ToString();
        }
        else
        {
            Debug.LogError("No TMP_Text found in front!");
        }

        Hide();
    }

    public void ConfigureSelection(Action<Card> onSelected, Func<bool> canSelect)
    {
        onCardSelected = onSelected;
        canSelectCard = canSelect;
    }

    public void Show()
    {
        front.SetActive(true);
        back.SetActive(false);
    }

    public void Hide()
    {
        front.SetActive(false);
        back.SetActive(true);
    }

    public void OnClick()
    {
        if (isMatched) return;
        if (onCardSelected == null || canSelectCard == null) return;
        if (!canSelectCard()) return;

        Show();
        onCardSelected(this);
    }
}
