using UnityEngine;
using System.Collections;

public class SimpleAI : MonoBehaviour
{
    // เรียกฟังก์ชันนี้เมื่อถึงตา AI (GameManager เป็นคนเรียก)
    public void PlayTurn()
    {
        StartCoroutine(AIRoutine());
    }

    IEnumerator AIRoutine()
    {
        Debug.Log("🤖 AI: กำลังคิด...");
        yield return new WaitForSeconds(1.0f); // รอ 1 วิ ให้คนเตรียมใจ

        Debug.Log("🤖 AI: ทอยเต๋าล่ะนะ!");

        // สั่งทอยเต๋าเหมือนที่คนกดปุ่ม (ใช้ระบบเดิมเลย)
        // แต่ต้องแน่ใจว่าตอนนี้เป็นตาของ AI จริงๆ เพื่อไม่ให้ DiceRoller บล็อก
        if (DiceRollerFromPNG.TryGet(out var diceRoller))
            diceRoller.RollDice();
    }
}