using UnityEngine;

public class PopupNode : MonoBehaviour
{
    public GameObject popupPanel; // ลาก UI Panel ที่ต้องการโชว์มาใส่ใน Inspector

    public void ActivatePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Debug.Log("Popup panel activated!");
        }
        else
        {
            Debug.LogWarning("Popup panel not assigned!");
        }
    }

    // ถ้าต้องการปิด popup ก็สร้างฟังก์ชันนี้ไว้เรียกใช้งานได้
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            Debug.Log("Popup panel closed.");
        }
    }
}
