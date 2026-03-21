using UnityEngine;
using UnityEngine.SceneManagement;

public static class MiniGameRewardService
{
    public static int CalculateCreditReward(int score)
    {
        if (score <= 0) return 0;
        return Mathf.Clamp(score / 100, 10, 250);
    }

    public static bool TryGrantCreditReward(int score, string sourceTag)
    {
        int reward = CalculateCreditReward(score);
        if (reward <= 0) return false;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in taggedObjects)
        {
            if (obj.scene.buildIndex != -1) continue; // ต้องเป็นผู้เล่นตัวจริงที่ DontDestroy

            PlayerState player = obj.GetComponent<PlayerState>();
            if (player == null || player.isAI) continue;

            player.PlayerCredit += reward;

            if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.AddCredit(reward);
            }

            Debug.Log($"[MiniGameReward] {sourceTag}: Reward +{reward} credit (score={score})");
            return true;
        }

        Debug.LogWarning($"[MiniGameReward] {sourceTag}: not found real player for reward");
        return false;
    }

    public static void ReturnToBoardScene()
    {
        string boardScene = PlayerPrefs.GetString(GameEventManager.LastBoardSceneKey, "TestMain");
        SceneManager.LoadScene(boardScene);
    }
}
