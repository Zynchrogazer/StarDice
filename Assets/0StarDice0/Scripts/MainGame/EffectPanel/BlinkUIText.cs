using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkTMPText : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float blinkInterval = 0.5f;

    // 🟢 เปลี่ยนจาก Start เป็น OnEnable (ทำงานทุกครั้งที่ถูกเปิดขึ้นมาใหม่)
    void OnEnable()
    {
        // 1. บังคับให้เปิดตา (โชว์ข้อความ) ก่อนเสมอ ป้องกันอาการค้างตอนดับจากรอบที่แล้ว
        if (tmpText != null)
        {
            tmpText.enabled = true;
        }
        
        // 2. สั่งเริ่มกระพริบใหม่
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval); // รอแป๊บนึงก่อนค่อยสลับสถานะ
            tmpText.enabled = !tmpText.enabled;
        }
    }
}