using UnityEngine;
using UnityEngine.UI;

public class BearTrapController : MonoBehaviour
{
    public Image openImage;    // ภาพตอนยังไม่งับ
    public Image closedImage;  // ภาพตอนงับแล้ว
    public Image bloodImage;  // ภาพเลือด
    public float delay = 0.5f; // เวลารอก่อนงับ

    void Start()
    {
        // เริ่มด้วยเปิดภาพ openImage
        openImage.gameObject.SetActive(true);
        closedImage.gameObject.SetActive(false);
        bloodImage.gameObject.SetActive(false);

        // ตั้งเวลาให้สลับ
        Invoke(nameof(SnapTrap), delay);
    }

    void SnapTrap()
    {
        openImage.gameObject.SetActive(false);   // ซ่อนกับดักเปิด
        closedImage.gameObject.SetActive(true);  // โชว์กับดักปิด
         bloodImage.gameObject.SetActive(true);
    }
}
