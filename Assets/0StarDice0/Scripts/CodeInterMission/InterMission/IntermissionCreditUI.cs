using TMPro;
using UnityEngine;

public class IntermissionCreditUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text creditText;

    [Header("Display")]
    [SerializeField] private string prefix = "Credit: ";

    private void Awake()
    {
        TryFindCreditText();
    }

    private void OnEnable()
    {
        RefreshCredit();
    }

    private void Update()
    {
        RefreshCredit();
    }

    private void TryFindCreditText()
    {
        if (creditText != null) return;

        creditText = GetComponent<TMP_Text>();
    }

    public void RefreshCredit()
    {
        if (creditText == null)
        {
            TryFindCreditText();
            if (creditText == null) return;
        }

        int credit = 0;

        // Intermission (นอกด่าน) ใช้เครดิตจากข้อมูลถาวรเท่านั้น
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            credit = GameData.Instance.selectedPlayer.Credit;
        }

        creditText.text = $"{prefix}{credit}";
    }
}
