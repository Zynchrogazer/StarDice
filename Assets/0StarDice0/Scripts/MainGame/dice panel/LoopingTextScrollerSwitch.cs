using UnityEngine;
using UnityEngine.UI;

public class LoopingTextScrollerSwitch : MonoBehaviour
{
    public float scrollSpeed = 50f;
    public RectTransform[] textRects;    // Text หลายอัน
    public float resetPositionX = -600f;  // จุดเริ่มต้น (ซ้ายสุด)
    public float endPositionX = 600;   // จุดที่จะรีเซ็ตกลับ (ขวาสุด)

    void Update()
    {
        foreach (RectTransform textRect in textRects)
        {
            // เคลื่อนตำแหน่งไปทางขวา
            textRect.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;

            // ถ้าหลุดขอบขวา ให้กลับไปซ้าย
            if (textRect.anchoredPosition.x >= resetPositionX)
            {
                textRect.anchoredPosition = new Vector2(endPositionX, textRect.anchoredPosition.y);
            }
        }
    }
}
