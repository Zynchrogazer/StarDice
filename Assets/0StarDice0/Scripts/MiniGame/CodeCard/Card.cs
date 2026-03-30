using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    public int cardId;
    public GameObject front;
    public GameObject back;
    public bool isMatched = false;

    private TMP_Text frontText;

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

        // เช็คว่า GameManager ตัวไหนกำลังทำงาน
        if (GameManagerLevel1.Instance != null && !GameManagerLevel1.Instance.IsBusy)
        {
            Show();
            GameManagerLevel1.Instance.OnCardSelected(this);
        }
        else if (GameManagerLevel2.Instance != null && !GameManagerLevel2.Instance.IsBusy)
        {
            Show();
            GameManagerLevel2.Instance.OnCardSelected(this);
        }
        else if (GameManagerLevel3.Instance != null && !GameManagerLevel3.Instance.IsBusy)
        {
            Show();
            GameManagerLevel3.Instance.OnCardSelected(this);
        }
    }
}
