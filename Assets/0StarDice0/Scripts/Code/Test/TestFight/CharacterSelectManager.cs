using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    public PlayerData selectedPlayer; // ตัวละครที่เลือกตอนนี้

    void Awake()
    {
        if (FindObjectsOfType<CharacterSelectManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // เก็บข้าม Scene
    }

    // ฟังก์ชันเลือกตัวละคร
    public void SelectCharacter(PlayerData player)
    {
        selectedPlayer = player;
       
        if (GameData.Instance != null)
        {
            GameData.Instance.selectedPlayer = player;
        }
    }
}
