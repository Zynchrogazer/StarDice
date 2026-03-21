// ตัวอย่างการตั้งค่าในโค้ด
using UnityEngine;
using UnityEngine.UI;

public class ScrollSpeedAdjuster : MonoBehaviour
{
    public ScrollRect scrollRect;

    private void Start()
    {
        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = 10f; // ปรับความเร็ว
        }
    }
}
