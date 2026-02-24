using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MemoryGameManager))]
public class ButtonSetup : MonoBehaviour
{
    public MemoryGameManager gameManager;

    void Awake()
    {
        if (gameManager == null)
            gameManager = GetComponent<MemoryGameManager>();

        for (int i = 0; i < gameManager.buttons.Count; i++)
        {
            int index = i; // สำคัญ: ต้องเก็บค่าชั่วคราวเพื่อไม่ให้ closure bug
            gameManager.buttons[i].onClick.RemoveAllListeners(); // ล้าง Listener เดิม
            gameManager.buttons[i].onClick.AddListener(() => gameManager.OnButtonPressed(index));
        }
    }
}
