using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public PlayerData[] allCharacters;   // ตัวละครทั้งหมด
    public Button[] characterButtons;    // ปุ่มที่วางใน Scene
    [SerializeField] private CharacterSelectManager characterSelectManager;

    void Start()
    {
        if (characterSelectManager == null)
        {
            characterSelectManager = FindObjectOfType<CharacterSelectManager>();
        }

        if (characterSelectManager == null)
        {
            Debug.LogError("[CharacterSelectUI] ไม่พบ CharacterSelectManager ใน scene");
            return;
        }

        SetupButtons();
    }

    void SetupButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;

            // ลบ Listener เก่า แล้วผูกใหม่
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnButtonClick(index));

            // ตั้งสีปุ่มตาม Data
            if (characterSelectManager.selectedPlayer == allCharacters[i])
                characterButtons[i].image.color = Color.gray; // ปุ่มถูกเลือก
            else
                characterButtons[i].image.color = Color.white; // ปุ่มไม่ถูกเลือก
        }
    }

    void OnButtonClick(int index)
    {
        // อัปเดต Data
        if (characterSelectManager == null)
        {
            Debug.LogError("[CharacterSelectUI] CharacterSelectManager หายไประหว่างทำงาน");
            return;
        }

        characterSelectManager.SelectCharacter(allCharacters[index]);

        // Refresh UI สีปุ่ม
        SetupButtons();
    }
}
