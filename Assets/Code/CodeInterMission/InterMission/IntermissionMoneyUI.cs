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

        // Priority: ใช้ค่าจากตัวผู้เล่นในบอร์ดก่อน เพราะเป็นค่าล่าสุดระหว่างเล่น
        // (บาง flow กลับเข้า Intermission ก่อนที่จะ sync กลับลง GameData ทันที)
        if (GameTurnManager.CurrentPlayer != null && !GameTurnManager.CurrentPlayer.isAI)
        {
            money = GameTurnManager.CurrentPlayer.PlayerMoney;

            // sync กลับลงข้อมูลหลักเพื่อให้ระบบอื่นใน InterMission เห็นค่าตรงกัน
            if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.SetMoney(money);
            }
        }
        else if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            money = GameData.Instance.selectedPlayer.Money;
        }

        moneyText.text = $"{prefix}{money}";
    }
}
