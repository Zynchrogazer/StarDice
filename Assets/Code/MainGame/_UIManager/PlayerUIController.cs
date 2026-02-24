using UnityEngine;
using TMPro;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text hpText;
    public TMP_Text moneyText;
    public TMP_Text starText;

    private void OnEnable()
    {
        UpdateUI(); // อัปเดตครั้งแรก
        // ถ้าคุณมี Event System ใน PlayerState ให้ Subscribe มาตรงนี้
    }

    private void Update()
    {
        // อัปเดต UI ทุกเฟรม (ง่ายสุด แต่เปลืองนิดหน่อย)
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (PlayerState.Instance == null) return;

        hpText.text = $"HP: {PlayerState.Instance.PlayerHealth}/100";
        moneyText.text = $"Money: {PlayerState.Instance.PlayerMoney}";
        starText.text = $"Star: {PlayerState.Instance.PlayerStar}";
    }
}
