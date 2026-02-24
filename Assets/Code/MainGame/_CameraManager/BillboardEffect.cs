using UnityEngine;

public class BillboardEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ถ้าติ๊กถูก = ตัวละครจะยืนตั้งตรง 90 องศากับพื้น (เหมาะกับตัวละครเดิน)\nถ้าไม่ติ๊ก = ตัวละครจะเอนหลังเงยหน้ามองกล้อง (เหมาะกับเอฟเฟกต์/หลอดเลือด)")]
    public bool standUpright = true;

    private Camera mainCamera;

    void Start()
    {
        // หากล้องหลักเตรียมไว้
        mainCamera = Camera.main;
    }

    // ใช้ LateUpdate เพื่อให้หมุนหลังจากที่กล้องขยับเสร็จแล้ว (ภาพจะไม่สั่น)
    void LateUpdate()
    {
        // 1. กัน Error หากล้องไม่เจอ
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 2. คำนวณการหันหน้า
        if (standUpright)
        {
            // ✅ แบบ A: ยืนตั้งตรง (Ragnarok Style)
            // เอาแค่แกน Y ของกล้องมา (หมุนซ้ายขวาตามกล้อง) แต่แกน X,Z ล็อคไว้ที่ 0 (ไม่ให้เอน)
            transform.rotation = Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f);
        }
        else
        {
            // ✅ แบบ B: หันหน้าหากล้องเป๊ะๆ (Paper Mario / UI Style)
            // ก๊อปปี้มุมกล้องมาเลย ตัวละครจะเอนหลังเหมือนนอนมองกล้อง
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}