using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public PlayerData[] allCharacters;
    public Button[] characterButtons;
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
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnButtonClick(index));

            if (characterSelectManager.SelectedPlayer == allCharacters[i])
                characterButtons[i].image.color = Color.gray;
            else
                characterButtons[i].image.color = Color.white;
        }
    }

    void OnButtonClick(int index)
    {
        if (characterSelectManager == null)
        {
            Debug.LogError("[CharacterSelectUI] CharacterSelectManager หายไประหว่างทำงาน");
            return;
        }

        characterSelectManager.SelectCharacter(allCharacters[index]);
        SetupButtons();
    }
}
