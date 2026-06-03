using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

public class UIBlurManager : MonoBehaviour
{
   [Header("ใส่ Post-Processing Volume ของฉากนี้")]
    public Volume globalVolume;
    
    private DepthOfField dofComponent;

    void Awake()
    {
        // ค้นหาเอฟเฟกต์ Depth of Field เตรียมไว้ตั้งแต่เริ่มเกม
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out dofComponent);
        }
    }

    // ฟังก์ชันนี้จะทำงาน "อัตโนมัติ" ทันทีที่ Panel นี้ถูกเปิด (SetActive เป็น true)
    void OnEnable()
    {
        if (dofComponent != null) 
        {
            dofComponent.active = true; // เปิดเบลอ
        }
    }

    // ฟังก์ชันนี้จะทำงาน "อัตโนมัติ" ทันทีที่ Panel นี้ถูกปิด (SetActive เป็น false)
    void OnDisable()
    {
        if (dofComponent != null) 
        {
            dofComponent.active = false; // ปิดเบลอ
        }
    }
}