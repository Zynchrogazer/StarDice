using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีเพื่อสั่งเปลี่ยน Scene
using System;
using System.Collections.Generic;

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

        // เคลียร์ state runtime ที่ติดมาจากรอบก่อน (DontDestroyOnLoad / Singleton)
        ResetRuntimeState();

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

    private void ResetRuntimeState()
    {
        HashSet<Type> persistentTypes = new HashSet<Type>
        {
            typeof(GameData),
            typeof(DeckData),
            typeof(DeckManager),
            typeof(GameTurnManager),
            typeof(GameEventManager),
            typeof(DiceRollerFromPNG),
            typeof(CameraController),
            typeof(SceneController),
            typeof(NormaSystem),
            typeof(BoardGameGroup),
            typeof(PlayerInventory),
            typeof(PlayerDataManager),
            typeof(EquipmentManager),
            typeof(PassiveSkillManager),
            typeof(ScoreManager),
            typeof(CharacterSelectManager)
        };

        MonoBehaviour[] allBehaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour == null) continue;
            if (!persistentTypes.Contains(behaviour.GetType())) continue;
            if (behaviour.gameObject.scene.buildIndex != -1) continue; // เฉพาะ DontDestroyOnLoad

            Destroy(behaviour.gameObject);
        }

        PlayerStartSpawner.LastKnownPositions.Clear();
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
