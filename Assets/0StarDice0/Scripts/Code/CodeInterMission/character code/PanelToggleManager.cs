using UnityEngine;

public class PanelToggleManager : MonoBehaviour
{
    public GameObject[] panels; // ใส่ Panel ทั้งหมด

    [Header("Toggle Behavior")]
    [SerializeField] private bool allowToggleOffWhenTargetAlreadyOpen = false;

    public void TogglePanel(int index)
    {
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogWarning("❌ Index ผิดพลาด: " + index);
            return;
        }

        GameObject targetPanel = panels[index];
        Debug.Log($"👉 TogglePanel({index}) เรียกกับ {targetPanel.name}, active = {targetPanel.activeSelf}");

        // ถ้า Panel ตัวเองเปิดอยู่
        if (targetPanel.activeSelf)
        {
            if (!allowToggleOffWhenTargetAlreadyOpen)
            {
                Debug.Log($"ℹ {targetPanel.name} เปิดอยู่แล้ว → ข้ามการปิด (ป้องกันปิดจากการกด UI ภายใน)");
                return;
            }

            Debug.Log($"🔴 ปิด {targetPanel.name}");
            targetPanel.SetActive(false);
            return;
        }

        // ถ้า Panel ตัวเองปิดอยู่ → เช็คว่ามี panel อื่นเปิดหรือไม่
        foreach (GameObject panel in panels)
        {
            if (panel != targetPanel && panel.activeSelf)
            {
                Debug.Log($"⚠ {panel.name} กำลังเปิดอยู่ → ไม่เปิด {targetPanel.name}");
                return;
            }
        }

        // เปิด Panel
        Debug.Log($"🟢 เปิด {targetPanel.name}");
        targetPanel.SetActive(true);
    }

    public void ClosePanel(int index)
    {
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogWarning("❌ Index ผิดพลาด: " + index);
            return;
        }

        GameObject targetPanel = panels[index];
        if (!targetPanel.activeSelf)
        {
            return;
        }

        Debug.Log($"🔴 ClosePanel({index}) ปิด {targetPanel.name}");
        targetPanel.SetActive(false);
    }
}
