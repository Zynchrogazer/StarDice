using UnityEngine;

public class PanelToggleManager : MonoBehaviour
{
    public GameObject[] panels; // ใส่ Panel ทั้งหมด

    [Header("Toggle Behavior")]
    [SerializeField] private bool allowToggleOffWhenTargetAlreadyOpen = false;

    [Header("Outside Click Close")]
    [SerializeField] private bool closeOnOutsideClick = true;

    // กันการปิดทันทีจากคลิกเดียวกับที่ใช้เปิด panel
    private int lastOpenedPanelIndex = -1;
    private int lastOpenedFrame = -1;

    private void Update()
    {
        if (!closeOnOutsideClick || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        int openPanelIndex = GetFirstOpenPanelIndex();
        if (openPanelIndex < 0)
        {
            return;
        }

        if (openPanelIndex == lastOpenedPanelIndex && Time.frameCount == lastOpenedFrame)
        {
            return;
        }

        if (IsPointerInsidePanel(panels[openPanelIndex]))
        {
            return;
        }

        Debug.Log($"🖱 คลิกนอกพื้นที่ panel → ปิด {panels[openPanelIndex].name}");
        panels[openPanelIndex].SetActive(false);
    }

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
        lastOpenedPanelIndex = index;
        lastOpenedFrame = Time.frameCount;
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

    private int GetFirstOpenPanelIndex()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && panels[i].activeSelf)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsPointerInsidePanel(GameObject panel)
    {
        if (panel == null)
        {
            return false;
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            return false;
        }

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        Camera eventCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, eventCamera);
    }
}