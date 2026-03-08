using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีเพื่อสั่งเปลี่ยน Scene

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "GameScene"; // ใส่ชื่อ Scene เกมของคุณตรงนี้ (เช่น "MainGame" หรือ "DeckBuilding")

    // รายชื่อ Key ที่ต้องการรีเซ็ตเมื่อกด New Game (ต้องตรงกับใน BackupSaveManager)
    private string[] keysToReset = new string[] 
    { 
        "MonsterWater", "MonsterEarth", "MonsterWind", "MonsterLight", "MonsterDark",
        "CurrentDeckData" 
        // ถ้ามี Level หรือ Credit ก็ใส่เพิ่มตรงนี้
    };

    public void OnNewGameClicked()
    {
        Debug.Log("🗑️ Clearing Active Data for New Game...");

        // 1. วนลูปเพื่อลบข้อมูลการเล่นปัจจุบันทิ้ง (Reset)
        foreach (string key in keysToReset)
        {
            PlayerPrefs.DeleteKey(key);
        }

        // 2. บันทึกการลบ
        PlayerPrefs.Save();

        // 3. เปลี่ยนฉากไปเริ่มเกม
        SceneManager.LoadScene(gameSceneName);
    }

    // (แถม) ปุ่ม Continue: โหลดฉากเกมเลยโดยไม่ลบค่า (เล่นต่อจากล่าสุด)
    public void OnContinueClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    // (แถม) ปุ่ม Exit
    public void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}