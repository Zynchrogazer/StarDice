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

        if (GUILayout.Button("Roll 1")) RollForced(1);
        if (GUILayout.Button("Roll 2")) RollForced(2);
        if (GUILayout.Button("Roll 3")) RollForced(3);
        if (GUILayout.Button("Roll 4")) RollForced(4);
        if (GUILayout.Button("Roll 5")) RollForced(5);
        if (GUILayout.Button("Roll 6")) RollForced(6);

        GUILayout.EndArea();
    }

    private void RollForced(int value)
    {
        if (DiceRollerFromPNG.TryGet(out var diceRoller))
            diceRoller.RollDiceWithResult(value);
    }
}
