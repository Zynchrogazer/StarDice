using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BattleResultFlowService
{
    private const string RuntimeHubSceneName = "RuntimeHub";
    private const string InterMissionSceneName = "InterMission";
    private const int DefaultMinReward = 50;
    private const int DefaultMaxReward = 300;

    private static CoroutineRunner runner;
    private static bool isProcessingTransition;

 public static void HandleRewardAndReturnToBoard(int minReward = DefaultMinReward, int maxReward = DefaultMaxReward, bool isMiniGame = false)
    {
        if (isProcessingTransition) return;

        PlayerState rewardTarget = ResolveHumanPlayerState();
        if (rewardTarget != null)
        {
            SyncHPFromBattleToBoard(rewardTarget);

            // 🟢 2. เอาสวิตช์มาครอบไว้! ถ้าไม่ใช่มินิเกม ถึงจะยอมให้ +1 Battle
            if (!isMiniGame)
            {
                rewardTarget.RecordBattleWin();
                Debug.Log("[BattleResultFlow] ⚔️ ชนะการต่อสู้จริง! เควสต์ Battle +1");
            }
            else
            {
                Debug.Log("[BattleResultFlow] 🎮 จบมินิเกม! ข้ามการบวกแต้ม Battle");
            }

            // แจกรางวัลตามปกติ (โค้ดเดิมของคุณเลยครับ)
            int reward = Random.Range(minReward, maxReward + 1);
            rewardTarget.PlayerCredit += reward;

            Debug.Log($"[BattleResultFlow] Reward claimed +{reward} credit");
        }
        else
        {
            Debug.LogWarning("[BattleResultFlow] Could not find human PlayerState for reward flow.");
        }

        // 🟢 2. เช็คสวิตช์บอส 
        bool isBossBattle = PlayerPrefs.GetInt("IsBossBattle", 0) == 1;

        if (isBossBattle)
        {
            Debug.Log("👑 [BattleResultFlow] ปราบบอสสำเร็จ! จบเกม กลับหน้า InterMission");
            PlayerPrefs.SetInt("IsBossBattle", 0);
            PlayerPrefs.Save();
            StartTransition(ReturnToSceneKeepingRuntimeHub(InterMissionSceneName));
        }
        else
        {
            Debug.Log("⚔️ [BattleResultFlow] ชนะมอนสเตอร์ปกติ กลับไปกระดาน");
            string targetBoardScene = PlayerPrefs.GetString(GameEventManager.LastBoardSceneKey, "TestMain");
            PlayerPrefs.SetInt(GameTurnManager.PendingBattleReturnKey, 1);
            PlayerPrefs.Save();
            StartTransition(ReturnToSceneKeepingRuntimeHub(targetBoardScene));
        }
    }
    public static void HandleRestartToInterMission()
    {
        if (isProcessingTransition) return;
        
        // 🟢 เคลียร์ค่าสวิตช์บอสทิ้งด้วย (เผื่อผู้เล่นสู้บอสแพ้ หรือกดยอมแพ้)
        PlayerPrefs.SetInt("IsBossBattle", 0);
        PlayerPrefs.Save();

        StartTransition(ReturnToSceneKeepingRuntimeHub(InterMissionSceneName));
    }
    private static PlayerState ResolveHumanPlayerState()
    {
        if (GameTurnManager.TryGet(out var gameTurnManager) && gameTurnManager.allPlayers != null)
        {
            foreach (PlayerState player in gameTurnManager.allPlayers)
            {
                if (player != null && !player.isAI)
                    return player;
            }
        }

        PlayerState[] allPlayers = Object.FindObjectsOfType<PlayerState>(true);
        foreach (PlayerState player in allPlayers)
        {
            if (player != null && !player.isAI)
                return player;
        }

        return null;
    }

    private static void StartTransition(IEnumerator routine)
    {
        EnsureRunner();
        if (runner == null) return;
        runner.StartCoroutine(routine);
    }

    private static void EnsureRunner()
    {
        if (runner != null) return;

        GameObject go = new GameObject("[BattleResultFlowService]");
        Object.DontDestroyOnLoad(go);
        runner = go.AddComponent<CoroutineRunner>();
    }

    private static IEnumerator ReturnToSceneKeepingRuntimeHub(string targetSceneName)
    {
        if (isProcessingTransition) yield break;
        isProcessingTransition = true;

        try
        {
            if (string.IsNullOrWhiteSpace(targetSceneName))
                yield break;

            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            bool wasAlreadyLoaded = targetScene.IsValid() && targetScene.isLoaded;
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                while (loadOp != null && !loadOp.isDone)
                {
                    yield return null;
                }

                targetScene = SceneManager.GetSceneByName(targetSceneName);
            }

            bool targetBoardReady = false;
            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                foreach (GameObject root in targetScene.GetRootGameObjects())
                {
                    if (root != null) root.SetActive(true);
                }

                SceneManager.SetActiveScene(targetScene);
                targetBoardReady = true;
            }

            if (targetBoardReady && wasAlreadyLoaded)
            {
                // KISS: ถ้า board ถูกโหลดค้างอยู่แล้ว จะไม่เกิด SceneLoaded event
                // จึงยิงสัญญาณ ready ตรงนี้เพื่อให้ TurnManager กู้ flow ได้แน่นอน
                GameEventManager.NotifyBoardSceneReady(targetSceneName);
            }

            int sceneCount = SceneManager.sceneCount;
            var toUnload = new System.Collections.Generic.List<Scene>();
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                if (string.Equals(scene.name, RuntimeHubSceneName, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(scene.name, targetSceneName, System.StringComparison.OrdinalIgnoreCase)) continue;

                toUnload.Add(scene);
            }

            for (int i = 0; i < toUnload.Count; i++)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(toUnload[i]);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }
        finally
        {
            isProcessingTransition = false;
        }
    }

    // 🟢 ฟังก์ชันใหม่สำหรับสูบเลือดจากฉากต่อสู้มาใส่ตัวละครบนบอร์ด
    private static void SyncHPFromBattleToBoard(PlayerState target)
    {
        if (target == null) return;

        // ค้นหา MonoBehaviour ทั้งหมดที่กำลังทำงานอยู่ (ซึ่งตอนนี้ฉากต่อสู้ยังเปิดอยู่)
        foreach (MonoBehaviour behaviour in Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (behaviour == null) continue;

            // มุดเข้าไปหาตัวแปรที่ชื่อ "playerHP" ในสคริปต์
            var hpField = behaviour.GetType().GetField("playerHP", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            
            if (hpField != null && hpField.FieldType == typeof(int))
            {
                int battleHp = (int)hpField.GetValue(behaviour);
                
                // อัปเดตเลือดกลับไปที่บอร์ด (ป้องกันเลือดติดลบ หรือเกิน Max)
                target.PlayerHealth = Mathf.Clamp(battleHp, 0, Mathf.Max(1, target.MaxHealth));
                
                Debug.Log($"[BattleResultFlow] 💖 ดูดเลือดปัจจุบัน ({battleHp}) กลับไปที่ตัวละครบนบอร์ดสำเร็จ!");
                return; // เจอแล้ว ดึงแล้ว หยุดหาได้เลย
            }
        }
    }

    private class CoroutineRunner : MonoBehaviour { }
}
