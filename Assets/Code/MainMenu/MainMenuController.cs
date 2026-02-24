using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการเปลี่ยน Scene
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "InterMission"; // ⭐️ ใส่ชื่อ Scene เกมหลักของคุณที่นี่

    [Header("DATABASE (ต้องลากใส่!)")]
    // ต้องเอา Database มาใส่เพื่อให้รู้ว่าจะลบสถานะการ์ดใบไหนบ้าง
    public List<CardData> allCardsDatabase; 

    // รายชื่อ Key ที่ต้องลบ (ก๊อปมาจาก BackupSaveManager)
    private string[] keysToClear = new string[] 
    { 
        "MonsterWater", "MonsterEarth", "MonsterWind", "MonsterLight", "MonsterDark",
        "CurrentDeckData" // เพิ่ม Deck เข้าไปด้วยเลย
    };

    // ฟังก์ชันนี้เอาไปผูกกับปุ่ม New Game (หรือปุ่ม Yes ใน Confirm Panel)
    public void OnClickNewGame()
    {
        Debug.Log("🗑️ Deleting Active Save Data...");

        // 1. ลบข้อมูลทั่วไป (Monster, Deck)
        foreach (string key in keysToClear)
        {
            PlayerPrefs.DeleteKey(key);
        }

        // 2. ลบสถานะการ์ด (Card States) - Logic เดียวกับ ResetActiveGame
        if (allCardsDatabase != null)
        {
            foreach (var card in allCardsDatabase)
            {
                if (card != null)
                {
                    string cardKey = "CardState_" + card.cardName;
                    PlayerPrefs.DeleteKey(cardKey);
                    
                    // รีเซ็ตค่าใน Memory ด้วย
                    card.isUsable = false; 
                }
            }
        }

        // 3. บันทึกการลบ
        PlayerPrefs.Save();

        // 4. เปลี่ยนไปฉากเกม
        Debug.Log("🚀 Loading Game Scene: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }
}