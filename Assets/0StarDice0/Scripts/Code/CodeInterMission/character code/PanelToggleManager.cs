using UnityEngine;
using UnityEngine.EventSystems;

public class PanelToggleManager : MonoBehaviour
{
    public enum PanelOpenPolicy
    {
        SingleOpenBlock = 0,
        SingleOpenSwitch = 1,
        MultiOpen = 2
    }

    public GameObject[] panels; // ใส่ Panel ทั้งหมด

    [Header("Toggle Behavior")]
    [SerializeField] private bool allowToggleOffWhenTargetAlreadyOpen = false;
    [SerializeField] private PanelOpenPolicy openPolicy = PanelOpenPolicy.SingleOpenBlock;

    [Header("Outside Click Close")]
    [SerializeField] private bool closeOnOutsideClick = true;
    [SerializeField] private bool ignoreOutsideCloseWhenPointerOverAnyUI = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // กันการปิดทันทีจากคลิกเดียวกับที่ใช้เปิด panel
    private int lastOpenedPanelIndex = -1;
    private int lastOpenedFrame = -1;
    private int currentOpenPanelIndex = -1;

    private void OnEnable()
    {
        currentOpenPanelIndex = GetFirstOpenPanelIndex();
    }

    private void Update()
    {
        if (!closeOnOutsideClick || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (ignoreOutsideCloseWhenPointerOverAnyUI && IsPointerOverAnyUI())
        {
            return;
        }

        int openPanelIndex = GetCurrentOpenPanelIndex();
        if (openPanelIndex < 0)
        {
            return;
        }

        if (openPanelIndex == lastOpenedPanelIndex && Time.frameCount == lastOpenedFrame)
        {
            return;
        }

        GameObject openPanel = panels[openPanelIndex];
        if (IsPointerInsidePanel(openPanel))
        {
            return;
        }

        Log($"🖱 คลิกนอกพื้นที่ panel → ปิด {openPanel.name}");
        SetPanelActive(openPanelIndex, false);
    }

    public void TogglePanel(int index)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning("❌ Index ผิดพลาด: " + index);
            return;
        }

        GameObject targetPanel = panels[index];
        if (targetPanel == null)
        {
            Debug.LogWarning($"❌ panels[{index}] เป็น null");
            return;
        }

        Log($"👉 TogglePanel({index}) เรียกกับ {targetPanel.name}, active = {targetPanel.activeSelf}");

        // ถ้า Panel ตัวเองเปิดอยู่
        if (targetPanel.activeSelf)
        {
            if (!allowToggleOffWhenTargetAlreadyOpen)
            {
                Log($"ℹ {targetPanel.name} เปิดอยู่แล้ว → ข้ามการปิด (ป้องกันปิดจากการกด UI ภายใน)");
                return;
            }

            Log($"🔴 ปิด {targetPanel.name}");
            SetPanelActive(index, false);
            return;
        }

        int openPanelIndex = GetCurrentOpenPanelIndex();

        if (openPolicy == PanelOpenPolicy.SingleOpenBlock)
        {
            if (openPanelIndex >= 0 && openPanelIndex != index)
            {
                GameObject openPanel = panels[openPanelIndex];
                if (openPanel != null && openPanel.activeSelf)
                {
                    Log($"⚠ {openPanel.name} กำลังเปิดอยู่ → ไม่เปิด {targetPanel.name}");
                    return;
                }
            }
        }
        else if (openPolicy == PanelOpenPolicy.SingleOpenSwitch)
        {
            if (openPanelIndex >= 0 && openPanelIndex != index)
            {
                SetPanelActive(openPanelIndex, false);
            }
        }

        // MultiOpen / SingleOpenSwitch / SingleOpenBlock(ไม่มีตัวอื่นเปิด) => เปิดได้
        Log($"🟢 เปิด {targetPanel.name}");
        SetPanelActive(index, true);
        lastOpenedPanelIndex = index;
        lastOpenedFrame = Time.frameCount;
    }

    public void ClosePanel(int index)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning("❌ Index ผิดพลาด: " + index);
            return;
        }

        GameObject targetPanel = panels[index];
        if (targetPanel == null || !targetPanel.activeSelf)
        {
            return;
        }

        Log($"🔴 ClosePanel({index}) ปิด {targetPanel.name}");
        SetPanelActive(index, false);
    }

    public void CloseAllPanels()
    {
        if (panels == null)
        {
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            SetPanelActive(i, false);
        }

        Log("🔴 CloseAllPanels() ปิดทุก panel แล้ว");
    }

    private bool IsPointerOverAnyUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }

        return false;
    }

    private int GetCurrentOpenPanelIndex()
    {
        if (IsValidIndex(currentOpenPanelIndex))
        {
            GameObject panel = panels[currentOpenPanelIndex];
            if (panel != null && panel.activeSelf)
            {
                return currentOpenPanelIndex;
            }
        }

        currentOpenPanelIndex = GetFirstOpenPanelIndex();
        return currentOpenPanelIndex;
    }

    private int GetFirstOpenPanelIndex()
    {
        if (panels == null)
        {
            return -1;
        }

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

    private bool IsValidIndex(int index)
    {
        return panels != null && index >= 0 && index < panels.Length;
    }

    private void SetPanelActive(int index, bool isActive)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        GameObject panel = panels[index];
        if (panel == null || panel.activeSelf == isActive)
        {
            return;
        }

        panel.SetActive(isActive);

        if (isActive)
        {
            currentOpenPanelIndex = index;
            return;
        }

        if (currentOpenPanelIndex == index)
        {
            currentOpenPanelIndex = -1;
        }
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}
