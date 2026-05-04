using UnityEngine;
using UnityEngine.SceneManagement;

public class BillboardEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ถ้าติ๊กถูก = ตัวละครจะยืนตั้งตรง 90 องศากับพื้น (เหมาะกับตัวละครเดิน)\nถ้าไม่ติ๊ก = ตัวละครจะเอนหลังเงยหน้ามองกล้อง (เหมาะกับเอฟเฟกต์/หลอดเลือด)")]
    public bool standUpright = true;

    [Tooltip("เพิ่ม/ลดมุมหันรอบแกน Y หาก sprite กลับหลังให้ใส่ 180")]
    public float yawOffset = 0f;

    private Camera mainCamera;

    private void LateUpdate()
    {
        if (!TryResolveCamera())
        {
            return;
        }

        if (standUpright)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 flatForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            transform.rotation = targetRotation * Quaternion.Euler(0f, yawOffset, 0f);
        }
        else
        {
            transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(0f, yawOffset, 0f);
        }
    }

    private bool TryResolveCamera()
    {
        if (IsUsableCamera(mainCamera))
        {
            return true;
        }

        Scene objectScene = gameObject.scene;

        Camera[] cameras = Camera.allCameras;

        // 1) เลือกกล้องที่อยู่ scene เดียวกันก่อน (กันกรณี RuntimeHub ค้างแบบ additive)
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (IsUsableCamera(candidate) && candidate.gameObject.scene == objectScene)
            {
                mainCamera = candidate;
                return true;
            }
        }

        // 2) ถ้าไม่มีใน scene เดียวกัน ค่อย fallback ไปที่ Camera.main
        Camera taggedMain = Camera.main;
        if (IsUsableCamera(taggedMain))
        {
            mainCamera = taggedMain;
            return true;
        }

        // 3) fallback สุดท้าย: กล้องที่ active ตัวแรก
        for (int i = 0; i < cameras.Length; i++)
        {
            if (IsUsableCamera(cameras[i]))
            {
                mainCamera = cameras[i];
                return true;
            }
        }

        return false;
    }

    private bool IsUsableCamera(Camera camera)
    {
        return camera != null && camera.isActiveAndEnabled;
    }
}
