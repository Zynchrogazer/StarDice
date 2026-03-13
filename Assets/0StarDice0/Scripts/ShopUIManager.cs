using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [FormerlySerializedAs("PlayerCredit")]
    [SerializeField] private int playerCredit = 0;
    public TMP_Text creditText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int PlayerCredit
    {
        get
        {
            if (TryResolveCreditData(out PlayerData data))
            {
                playerCredit = Mathf.Max(0, data.Credit);
            }

            return Mathf.Max(0, playerCredit);
        }
        set
        {
            int normalizedValue = Mathf.Max(0, value);
            playerCredit = normalizedValue;

            if (TryResolveCreditData(out PlayerData data))
            {
                data.SetCredit(normalizedValue);
            }

            OnStatsUpdated?.Invoke();
        }
    }

     private bool TryResolveCreditData(out PlayerData data)
    {
        data = selectedPlayerPreset;

        if (data != null)
        {
            return true;
        }

        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            data = GameData.Instance.selectedPlayer;
            selectedPlayerPreset = data;
            return true;
        }

        return false;
    }
}
