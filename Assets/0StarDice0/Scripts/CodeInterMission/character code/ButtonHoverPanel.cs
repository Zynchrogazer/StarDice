using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject panel; // Panel ที่จะแสดง
    private bool isHovering = false;

    void Update()
    {
        if (isHovering && panel.activeSelf)
        {
            // อัปเดตตำแหน่ง panel ให้ตามเมาส์
            Vector2 mousePos = Input.mousePosition;
            panel.transform.position = mousePos + new Vector2(0f, 100f); // ใส่ offset กันทับเมาส์
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panel.SetActive(true);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panel.SetActive(false);
        isHovering = false;
    }
}
