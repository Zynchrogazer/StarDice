using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    private bool hasScored = false;  // เช็คว่าได้คะแนนแล้วหรือยัง

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่า player ผ่านจริง ๆ และยังไม่เคยได้คะแนนจาก Zone นี้
        if (!hasScored && other.CompareTag("Player"))
        {
            hasScored = true;  // ป้องกันไม่ให้เพิ่มคะแนนซ้ำ
            GameManager.instance.AddScore(50);  // เพิ่มคะแนน 50
        }
    }

    // รีเซ็ตเมื่อ ScoreZone ถูกเปิดใหม่
    private void OnEnable()
    {
        hasScored = false;  // กลับไปยังสถานะไม่เคยได้คะแนน
    }
}
