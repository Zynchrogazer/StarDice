using UnityEngine;

public class DiceTester : MonoBehaviour
{
    // เปิด/ปิด UI สำหรับเทส
    public bool showTestButtons = true;

    private void OnGUI()
    {
        if (!showTestButtons) return;

        // วาด UI ปุ่มง่ายๆ บนหน้าจอ
        GUILayout.BeginArea(new Rect(10, 10, 150, 300));
        GUILayout.Label("--- Dice Test ---");

        if (GUILayout.Button("Roll 1"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(1);
        }
        if (GUILayout.Button("Roll 2"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(2);
        }
        if (GUILayout.Button("Roll 3"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(3);
        }
        if (GUILayout.Button("Roll 4"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(4);
        }
        if (GUILayout.Button("Roll 5"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(5);
        }
        if (GUILayout.Button("Roll 6"))
        {
            DiceRollerFromPNG.Instance.RollDiceWithResult(6);
        }

        GUILayout.EndArea();
    }
}