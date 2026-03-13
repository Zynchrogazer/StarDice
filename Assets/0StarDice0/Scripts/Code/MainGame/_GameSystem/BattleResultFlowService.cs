using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BattleResultFlowService
{
    private const string RuntimeHubSceneName = "RuntimeHub";
    private const string InterMissionSceneName = "InterMission";
    private const int DefaultMinReward = 30;
    private const int DefaultMaxReward = 120;

    private static CoroutineRunner runner;
    private static bool isProcessingTransition;

    public static void HandleRewardAndReturnToBoard(int minReward = DefaultMinReward, int maxReward = DefaultMaxReward)
    {
        if (isProcessingTransition) return;

        PlayerState rewardTarget = ResolveHumanPlayerState();
        if (rewardTarget != null)
        {
            rewardTarget.RecordBattleWin();

            int reward = Random.Range(minReward, maxReward + 1);
            rewardTarget.PlayerCredit += reward;

            if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.AddCredit(reward);
            }

            Debug.Log($"[BattleResultFlow] Reward claimed +{reward} credit");
        }
        else
        {
            Debug.LogWarning("[BattleResultFlow] Could not find human PlayerState for reward flow.");
        }

        string targetBoardScene = PlayerPrefs.GetString(GameEventManager.LastBoardSceneKey, "TestMain");
        PlayerPrefs.SetInt(GameTurnManager.PendingBattleReturnKey, 1);
        PlayerPrefs.Save();

        StartTransition(ReturnToSceneKeepingRuntimeHub(targetBoardScene));
    }

    public static void HandleRestartToInterMission()
    {
        if (isProcessingTransition) return;
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
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                while (loadOp != null && !loadOp.isDone)
                {
                    yield return null;
                }

                targetScene = SceneManager.GetSceneByName(targetSceneName);
            }

            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                foreach (GameObject root in targetScene.GetRootGameObjects())
                {
                    if (root != null) root.SetActive(true);
                }

                SceneManager.SetActiveScene(targetScene);
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

    private class CoroutineRunner : MonoBehaviour { }
}
