// In PlayerMovement.cs

using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isMoving = false;

    public bool IsMoving => isMoving; // property สำหรับให้ script อื่นตรวจสอบสถานะ

    // เมธอดสำหรับสั่งให้เริ่มเคลื่อนที่ไปยังเป้าหมาย
    public void MoveTo(Transform targetNode, System.Action onArrival = null)
    {
        if (isMoving || targetNode == null) return;
        StartCoroutine(MoveCoroutine(targetNode, onArrival));
    }

    private IEnumerator MoveCoroutine(Transform targetNode, System.Action onArrival)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetNode.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetNode.position, moveSpeed * Time.deltaTime);
            yield return null; // รอเฟรมถัดไป
        }

        // เมื่อถึงที่หมายแล้ว
        transform.position = targetNode.position; // ทำให้ตำแหน่งตรงเป๊ะ
        isMoving = false;

        // เรียก callback function ถ้ามี
        onArrival?.Invoke();
    }
}