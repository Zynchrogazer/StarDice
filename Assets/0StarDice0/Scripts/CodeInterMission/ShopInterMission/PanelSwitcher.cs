using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject[] panels; // ใส่ Panel ทั้งหมด

    public void SwitchPanel(int index)
    {
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogWarning("❌ Index ผิดพลาด: " + index);
            return;
        }

        // ปิดทุก Panel ก่อน
        foreach (GameObject panel in panels)
        {
            if (panel.activeSelf)
            {
                Debug.Log($"🔴 ปิด {panel.name}");
                panel.SetActive(false);
            }
        }

        // เปิด Panel ที่เลือก
        GameObject targetPanel = panels[index];
        Debug.Log($"🟢 เปิด {targetPanel.name}");
        targetPanel.SetActive(true);
    }
}
