using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;

    public PlayerData selectedPlayer; // ตัวละครที่เลือกตอนนี้

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
