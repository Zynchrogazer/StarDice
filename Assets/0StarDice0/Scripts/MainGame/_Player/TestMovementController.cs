// In TestMovementController.cs (Updated Version)

using UnityEngine;

public class TestMovementController : MonoBehaviour
{
    [Tooltip("ลาก Player GameObject ที่มี PlayerPathWalker Script มาใส่ที่นี่")]
    // 1. เปลี่ยนจาก PlayerController เป็น PlayerPathWalker
    public PlayerPathWalker playerToTest;

    void Start()
    {
        if (playerToTest == null)
        {
            Debug.LogError("PlayerPathWalker is not assigned in TestMovementController. Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// เมธอดสำหรับรับคำสั่งเดินจากปุ่มทดสอบ
    /// </summary>
    /// <param name="numberOfSteps">จำนวนก้าวที่ต้องการให้เดิน</param>
    public void TriggerTestMove(int numberOfSteps)
    {
        if (playerToTest == null)
        {
            Debug.LogError("PlayerPathWalker is not assigned. Cannot trigger move.");
            return;
        }

        // 2. ตรวจสอบสถานะด้วยเมธอด IsExecutingTurn() ของ PlayerPathWalker
        if (playerToTest.IsExecutingTurn)
        {
            Debug.LogWarning($"Player is currently busy. Cannot start new test move from TestMovementController.");
            return;
        }

        Debug.Log($"TestMovementController: Triggering player to move {numberOfSteps} steps.");
        // 3. เรียกใช้เมธอด ExecuteMove() ของ PlayerPathWalker
        playerToTest.ExecuteMove(numberOfSteps);
    }
}