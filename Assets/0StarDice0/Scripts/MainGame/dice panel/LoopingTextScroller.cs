using UnityEngine;

public class LoopingTextScrollerMulti : MonoBehaviour
{
    public RectTransform[] texts;    // Array ของ RectTransform ที่จะเลื่อน
    public float scrollSpeed = 50f;
    public float resetX = 600f;      // จุดที่รีเซ็ตกลับ (ด้านขวา)
    public float endX = -600f;       // จุดที่ข้อความหาย (ด้านซ้าย)

    void Update()
    {
        foreach (RectTransform t in texts)
        {
            // เลื่อนซ้าย
            t.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // รีเซ็ตถ้าออกนอกจอ
            if (t.anchoredPosition.x <= endX)
            {
                t.anchoredPosition = new Vector2(resetX, t.anchoredPosition.y);
            }
        }
    }
}
