using TMPro;
using UnityEngine;

public class IntermissionMoneyUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Display")]
    [SerializeField] private string prefix = "Money: ";

    private void Awake()
    {
        TryFindMoneyText();
    }

    private void OnEnable()
    {
        RefreshMoney();
    }

    private void Update()
    {
        RefreshMoney();
    }

    private void TryFindMoneyText()
    {
        if (moneyText != null) return;

        moneyText = GetComponent<TMP_Text>();
    }

    public void RefreshMoney()
    {
        if (moneyText == null)
        {
            TryFindMoneyText();
            if (moneyText == null) return;
        }

        int money = 0;

        // Intermission (นอกด่าน) ใช้เงินจากข้อมูลถาวรเท่านั้น
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            money = GameData.Instance.selectedPlayer.Money;
        }

        moneyText.text = $"{prefix}{money}";
    }
}
