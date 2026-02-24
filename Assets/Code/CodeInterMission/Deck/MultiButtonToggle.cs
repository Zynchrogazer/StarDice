using UnityEngine;

public class MultiButtonToggle : MonoBehaviour
{
    public GameObject[] buttonsToToggle; // ปุ่มที่ต้องการให้โผล่/หาย
    private bool isActive = false;       // สถานะปัจจุบันของปุ่ม

    /// <summary>
    /// เรียกฟังก์ชันนี้จากปุ่มหลัก
    /// </summary>
    public void ToggleButtons()
    {
        isActive = !isActive; // เปลี่ยนสถานะ

        foreach (GameObject button in buttonsToToggle)
        {
            button.SetActive(isActive); // เปิด/ปิดปุ่ม
        }

        Debug.Log("ปุ่มอื่น ๆ ตอนนี้ " + (isActive ? "แสดง" : "ซ่อน"));
    }
}
