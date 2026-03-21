using UnityEngine;
using System;

public class ChoiceArrow : MonoBehaviour
{
    // ตัวแปรสาธารณะสำหรับให้ UIManager บอกว่าลูกศรนี้ชี้ไปที่โหนดไหน
    public Transform TargetNode { get; set; }

    // Callback ที่จะถูกเรียกเมื่อลูกศรนี้ถูกคลิก
    public Action<Transform> OnArrowClicked { get; set; }

    // OnMouseDown() เป็นเมธอดพิเศษของ Unity ที่จะทำงานเมื่อผู้ใช้คลิกเมาส์ลงบน Collider ของ Object นี้
    private void OnMouseDown()
    {
        Debug.Log($"Arrow clicked! Choosing path to {TargetNode.name}");

        // เมื่อถูกคลิก ให้เรียก Callback กลับไปหา UIManager (ซึ่งจะต่อไปยัง PlayerPathWalker)
        // พร้อมกับส่ง "เป้าหมาย" ของตัวเองกลับไปด้วย
        OnArrowClicked?.Invoke(TargetNode);
    }
}