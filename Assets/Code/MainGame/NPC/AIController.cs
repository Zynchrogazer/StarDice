using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    private PlayerState myState;
    private PlayerPathWalker myWalker;

    private void Start()
    {
        myState = GetComponent<PlayerState>();
        myWalker = GetComponent<PlayerPathWalker>();
    }

    // --- 1. สั่งให้ AI เริ่มเทิร์น (เรียกจาก GameManager) ---
    public void StartAITurn()
    {
        if (!myState.isAI) return;

        Debug.Log($"🤖 AI {name} is thinking...");
        StartCoroutine(ThinkAndAct());
    }

    private IEnumerator ThinkAndAct()
    {
        // แกล้งคิดแป๊บนึง (ให้คนดูทัน)
        yield return new WaitForSeconds(1.5f);

        // 1. เช็คว่าต้องเลือก Norma ไหม? (ถ้าเพิ่งเริ่มเกม หรือเวลอัป)
        // (ปกติระบบ Norma จะเด้ง UI แต่เราจะเขียนดักไว้ใน NormaSystem ว่าถ้าเป็น AI ให้ข้าม UI)

        // 2. สั่งทอยลูกเต๋า
        Debug.Log("🤖 AI is rolling dice!");
        DiceRollerFromPNG.Instance.RollDice();
    }

    // --- 2. ฟังก์ชันตัดสินใจเลือกทางแยก (ถูกเรียกจาก PlayerPathWalker) ---
    public Transform ChoosePath(System.Collections.Generic.List<Transform> choices)
    {
        // Logic ง่ายๆ: สุ่มไปก่อน (อนาคตค่อยเขียนให้เดินไปหาดาว)
        int randomIndex = Random.Range(0, choices.Count);
        Debug.Log($"🤖 AI chose path index: {randomIndex}");
        return choices[randomIndex];
    }

    // --- 3. ฟังก์ชันตัดสินใจเลือก Norma (ถูกเรียกจาก NormaSystem) ---
    public NormaType ChooseNorma(int rank)
    {
        // Logic: ถ้าพลังโจมตีเยอะ เลือก Wins / ถ้าเลือดเยอะ เลือก Stars
        if (myState.CurrentAttack > 12) return NormaType.Wins;
        return NormaType.Stars;
    }
}