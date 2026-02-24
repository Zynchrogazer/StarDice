using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นต้องใช้เพื่อเช็คการคลิก

public class DeckSlotRightClick : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex; // จะถูกกำหนดโดย DeckManager ว่าช่องนี้คือ index ที่เท่าไหร่

    public void OnPointerClick(PointerEventData eventData)
    {
        // เช็คว่าเป็นคลิกขวา (Right Click) หรือไม่
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // เรียกฟังก์ชัน RemoveCard ใน DeckManager
            if (DeckManager.Instance != null)
            {
                Debug.Log($"🖱️ Right Clicked on Slot {slotIndex}");
                DeckManager.Instance.RemoveCard(slotIndex);
            }
        }
    }
}