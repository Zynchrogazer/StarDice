using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ใช้กับพื้นที่ "ภายในหน้าต่าง status" เพื่อกิน event click
/// ไม่ให้ click ไปโดนปุ่มพื้นหลังที่ใช้เปิด/ปิด panel
/// </summary>
public class StatusPanelClickBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
}
