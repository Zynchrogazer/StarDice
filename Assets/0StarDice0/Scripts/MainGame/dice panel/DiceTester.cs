using UnityEngine;

public class DiceTester : MonoBehaviour
{
    private static readonly KeyCode[] ForcedRollKeys =
    {
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6
    };

    // เปิด/ปิด UI สำหรับเทส
    public bool showTestButtons = true;

    // เปิด/ปิดปุ่มลัด NumPad สำหรับบังคับหน้าลูกเต๋า 1-6
    public bool enableNumpadShortcuts = true;

    private void Update()
    {
        if (!enableNumpadShortcuts) return;

        for (int i = 0; i < ForcedRollKeys.Length; i++)
        {
            if (Input.GetKeyDown(ForcedRollKeys[i]))
            {
                RollForced(i + 1);
                return;
            }
        }
    }

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
