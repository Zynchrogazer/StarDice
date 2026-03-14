using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text creditText;
    [SerializeField] private string creditPrefix = "Credit: ";

    [Header("Data Source")]
    [Tooltip("Optional fallback when GameData.Instance.selectedPlayer is not available.")]
    [SerializeField] private PlayerData fallbackPlayerData;

    [FormerlySerializedAs("PlayerCredit")]
    [SerializeField, Min(0)] private int playerCredit;

    private PlayerData boundPlayerData;

    public int PlayerCredit
    {
        get
        {
            if (TryResolvePlayerData(out PlayerData data))
            {
                return Mathf.Max(0, data.Credit);
            }

            return Mathf.Max(0, playerCredit);
        }
        set
        {
            int normalizedValue = Mathf.Max(0, value);
            playerCredit = normalizedValue;

            if (TryResolvePlayerData(out PlayerData data))
            {
                data.SetCredit(normalizedValue);
            }

            RefreshCreditText();
        }
    }

    private void Awake()
    {
        TryFindCreditText();
    }

    private void OnEnable()
    {
        RebindPlayerData();
        RefreshCreditText();
    }

    private void OnDisable()
    {
        UnbindPlayerData();
    }

    private void Update()
    {
        if (HasPlayerDataChanged())
        {
            RebindPlayerData();
            RefreshCreditText();
        }
    }

    private bool HasPlayerDataChanged()
    {
        return ResolvePreferredPlayerData() != boundPlayerData;
    }

    private void RebindPlayerData()
    {
        PlayerData nextData = ResolvePreferredPlayerData();
        if (nextData == boundPlayerData) return;

        UnbindPlayerData();

        boundPlayerData = nextData;
        if (boundPlayerData != null)
        {
            boundPlayerData.OnCreditChanged += HandleCreditChanged;
            playerCredit = Mathf.Max(0, boundPlayerData.Credit);
        }
    }

    private void UnbindPlayerData()
    {
        if (boundPlayerData == null) return;

        boundPlayerData.OnCreditChanged -= HandleCreditChanged;
        boundPlayerData = null;
    }

    private void HandleCreditChanged(int newCredit)
    {
        playerCredit = Mathf.Max(0, newCredit);
        RefreshCreditText();
    }

    private PlayerData ResolvePreferredPlayerData()
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            return GameData.Instance.selectedPlayer;
        }

        return fallbackPlayerData;
    }

    private bool TryResolvePlayerData(out PlayerData data)
    {
        data = ResolvePreferredPlayerData();
        return data != null;
    }

    private void TryFindCreditText()
    {
        if (creditText != null) return;

        creditText = GetComponentInChildren<TMP_Text>(true);
    }

    public void RefreshCreditText()
    {
        if (creditText == null)
        {
            TryFindCreditText();
            if (creditText == null) return;
        }

        creditText.text = $"{creditPrefix}{PlayerCredit}";
    }
}