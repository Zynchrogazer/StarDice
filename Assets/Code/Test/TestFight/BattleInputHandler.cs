using UnityEngine;
using UnityEngine.UI;

public class BattleInputHandler : MonoBehaviour
{
    // ลากปุ่มทั้ง 3 ใบ มาใส่ในช่องนี้ (Size = 3)
    public Button[] cardButtons; 

    // ฟังก์ชันนี้เอาไปผูกกับปุ่ม (On Click)
    public void OnCardClicked(int buttonIndex)
    {
        // 1. สั่งปิดเฉพาะปุ่มที่ถูกส่งเลขมา
        cardButtons[buttonIndex].interactable = false;

        // (แถม) เปลี่ยนสีให้ดูมืดลงด้วย เพื่อความชัดเจน
        // cardButtons[buttonIndex].GetComponent<Image>().color = Color.gray;

        Debug.Log("ปิดการใช้งานปุ่มหมายเลข: " + buttonIndex);
    }
    
    // อย่าลืม! ต้องมีฟังก์ชันเปิดปุ่มคืนตอนเริ่มเทิร์นใหม่
    public void ResetButtons()
    {
        foreach (Button btn in cardButtons)
        {
            btn.interactable = true;
            // btn.GetComponent<Image>().color = Color.white;
        }
    }
}