using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีเพื่อสั่งเปลี่ยน Scene
using System;
using System.Collections.Generic;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "FirstMonsterSelect"; // ใส่ชื่อ Scene เกมของคุณตรงนี้ (เช่น "MainGame" หรือ "DeckBuilding")
    [SerializeField] private string bootstrapSceneName = "Bootstrap";
    [SerializeField] private GameObject confirmExitPanel;

    private bool isRequestingFlowScene;

    // รายชื่อ Key ที่ต้องการรีเซ็ตเมื่อกด New Game (ต้องตรงกับใน BackupSaveManager)
    private string[] keysToReset = new string[] 
    { 
        "MonsterWater", "MonsterEarth", "MonsterWind", "MonsterLight", "MonsterDark", "MonsterFire",
        "SelectedMonster", "HasChosenMainCharacter",
        "CurrentDeckData" 
        // ถ้ามี Level หรือ Credit ก็ใส่เพิ่มตรงนี้
    };


    private void Awake()
    {
        SanitizeDefaultFlowTarget();

        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }


    private void SanitizeDefaultFlowTarget()
    {
        if (string.Equals(gameSceneName, "GameScene", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(gameSceneName)
            || !Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded("FirstMonsterSelect"))
            {
                gameSceneName = "FirstMonsterSelect";
            }
        }
    }

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

        // 2.1 เคลียร์ runtime run state (ถ้ามี bootstrap ค้างอยู่)
        if (RunSessionStore.TryGet(out var runSessionStore))
        {
            runSessionStore.ClearRunState();
        }

        // 3. เปลี่ยนฉากไปเริ่มเกม
        StartCoroutine(RequestFlowScene(gameSceneName));
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
        StartCoroutine(RequestFlowScene(gameSceneName));
    }

    private IEnumerator RequestFlowScene(string targetSceneName)
    {
        if (isRequestingFlowScene)
        {
            yield break;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            isRequestingFlowScene = false;
            yield break;
        }

        isRequestingFlowScene = true;

        if (SceneFlowController.TryRequestScene(targetSceneName))
        {
            isRequestingFlowScene = false;
            yield break;
        }

        // กรณีเริ่มจาก Menu โดยยังไม่มี controller: โหลด Bootstrap แบบ additive ก่อน
        if (!string.IsNullOrEmpty(bootstrapSceneName) &&
            Application.CanStreamedLevelBeLoaded(bootstrapSceneName) &&
            !SceneManager.GetSceneByName(bootstrapSceneName).isLoaded)
        {
            AsyncOperation bootstrapLoad = SceneManager.LoadSceneAsync(bootstrapSceneName, LoadSceneMode.Additive);
            while (bootstrapLoad != null && !bootstrapLoad.isDone)
            {
                yield return null;
            }
        }

        float timeout = 2f;
        while (timeout > 0f && !SceneFlowController.TryGet(out _))
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!SceneFlowController.TryRequestScene(targetSceneName))
        {
            // fallback สุดท้ายเพื่อไม่ให้ flow ตาย
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError($"[MainMenu] Cannot load scene '{targetSceneName}'. Check Build Profiles.");
            }
        }

        isRequestingFlowScene = false;
    }
    
    // (แถม) ปุ่ม Exit
    public void OnExitClicked()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(true);
            return;
        }

        ConfirmExitYes();
    }

    public void ConfirmExitYes()
    {
        Application.Quit();
        Debug.Log("Quit Game");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ConfirmExitNo()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }
}
