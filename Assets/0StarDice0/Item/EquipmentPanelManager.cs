using UnityEngine;

public class EquipmentPanelManager : MonoBehaviour
{
    // เอาไว้ให้ปุ่มกดเรียกเพื่อปิดหน้าต่าง
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}