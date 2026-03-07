using UnityEngine;

[ExecuteInEditMode]
public class CameraLookAtOrigin : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0, 15, -15);
    public float pitch = 35f;   // เอียงลง
    public float yaw = 0f;      // เอียงรอบแกน Y

    void Update()
    {
        transform.position = Quaternion.Euler(0, yaw, 0) * cameraOffset; // หมุนแกน Y ก่อน
        transform.LookAt(Vector3.zero);
        transform.Rotate(pitch, 0, 0, Space.Self); // หมุน Pitch อีกชั้น
    }
}