using UnityEngine;

public class BoardGameCamera : MonoBehaviour
{
    [Header("Target & Settings")]
    public Transform target; // ตัวละครที่ต้องการให้กล้องมอง (เปลี่ยนตาม Turn ผู้เล่น)
    public float smoothSpeed = 5f; // ความเร็วในการเลื่อนกล้อง (ค่าน้อย = นุ่มนวล)

    [Header("Position Offset")]
    public Vector3 offset = new Vector3(0, 4, -4); // ระยะห่างกล้องจากตัวละคร (สูง 10, ถอยหลัง 10)
    
    [Header("Rotation")]
    public float lookAngleX = 45f; // มุมก้มเงยของกล้อง

    void LateUpdate() // ใช้ LateUpdate เพื่อให้ขยับหลังจากตัวละครเดินเสร็จแล้ว ภาพจะไม่สั่น
    {
        if (target == null) return;

        // 1. คำนวณจุดที่กล้องควรจะอยู่ (ตำแหน่งตัวละคร + ระยะห่าง)
        Vector3 desiredPosition = target.position + offset;

        // 2. ใช้ Lerp เพื่อเลื่อนกล้องไปหาจุดนั้นแบบนุ่มนวล
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 3. บังคับให้กล้องหันหน้ามองตัวละครเสมอ (หรือจะล็อคมุมตายตัวก็ได้)
        // transform.LookAt(target); 
        
        // แนะนำ: ล็อคมุมกล้องให้คงที่ เพื่อไม่ให้เวียนหัวเวลาเดิน
        transform.rotation = Quaternion.Euler(lookAngleX, 0, 0);
    }
}