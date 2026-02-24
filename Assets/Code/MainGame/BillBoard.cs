using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // ทำให้ตัวละครหันหน้าเข้าหากล้องตลอดเวลา
        transform.forward = mainCamera.transform.forward;
        
        // ถ้าอยากให้หันหาแค่แกน Y (ไม่เงยหน้าขึ้นลงตามกล้อง) ให้ใช้วิธีด้านล่างแทน:
        // Vector3 targetPosition = transform.position + mainCamera.transform.rotation * Vector3.forward;
        // Vector3 targetOrientation = mainCamera.transform.rotation * Vector3.up;
        // transform.LookAt(targetPosition, targetOrientation);
    }
}