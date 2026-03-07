using UnityEngine;
using TMPro;
public class DisplayStatus : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerState playerState;
    public PlayerData playerData;
    public TextMeshProUGUI hpTxt;

    // Update is called once per frame
    void Update()
    {
        if (playerState != null && hpTxt != null)
        {
            int maxHp = playerState.MaxHealth > 0
                ? playerState.MaxHealth
                : (playerData != null ? Mathf.Max(1, playerData.maxHP) : 1);

            hpTxt.text = $"HP : {playerState.PlayerHealth}/{maxHp}";
        }
    }
}
