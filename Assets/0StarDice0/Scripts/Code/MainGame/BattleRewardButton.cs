using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleRewardButton : MonoBehaviour
{
    [Header("Credit Reward")]
    [SerializeField] private int minBattleCreditReward = 30;
    [SerializeField] private int maxBattleCreditReward = 120;

    // เรียกฟังก์ชันนี้เมื่อกดปุ่ม "รับรางวัล" (Link ผ่าน OnClick ใน Inspector)
    public void OnClaimRewardClicked()
    {
        Debug.Log("🎁 Claiming Reward...");

        PlayerState rewardTarget = null;

        // KISS: ถ้ามี GTM ใช้ current player ตรง ๆ ก่อน
        if (GameTurnManager.TryGet(out var gameTurnManager) &&
            gameTurnManager.allPlayers != null &&
            gameTurnManager.allPlayers.Count > 0)
        {
            rewardTarget = GameTurnManager.CurrentPlayer;
        }

        // fallback: หา player ที่เป็นคนจาก scene ใดก็ได้ (รวม inactive)
        if (rewardTarget == null)
        {
            PlayerState[] allPlayers = FindObjectsOfType<PlayerState>(true);
            foreach (var player in allPlayers)
            {
                if (player != null && !player.isAI)
                {
                    rewardTarget = player;
                    break;
                }
            }
        }

        if (rewardTarget != null)
        {
            Debug.Log($"✅ พบผู้เล่นตัวจริง: {rewardTarget.name} -> เพิ่ม WinCount และ EXP");

            // 2. 🎯 สั่งบวก WinCount และ Exp เข้าตัวจริงทันที
            rewardTarget.RecordBattleWin();

            // 3. 💰 สุ่มเครดิตรางวัลหลังชนะ Battle
            int reward = Random.Range(minBattleCreditReward, maxBattleCreditReward + 1);
            rewardTarget.PlayerCredit += reward;

            // Sync เข้ากับ PlayerData ที่เป็นข้อมูลหลักให้เครดิตคงอยู่ข้ามด่าน/ข้ามซีน
            if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.AddCredit(reward);
            }

            Debug.Log($"💰 ได้รับเครดิตจาก Battle +{reward} (รวมตอนนี้ {rewardTarget.PlayerCredit})");
        }
        else Debug.LogWarning("⚠ หาผู้เล่นตัวจริงไม่เจอ! (ค่า WinCount อาจไม่อัปเดต)");

        // 3. 🏠 ย้ายกลับบ้าน (กลับฉากบอร์ดล่าสุดที่จำไว้)
        string targetBoardScene = PlayerPrefs.GetString(GameEventManager.LastBoardSceneKey, "TestMain");
        PlayerPrefs.SetInt(GameTurnManager.PendingBattleReturnKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(targetBoardScene);
    }
}
