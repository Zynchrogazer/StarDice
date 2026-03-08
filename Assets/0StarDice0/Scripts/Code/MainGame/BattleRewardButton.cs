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

        // 1. 🔍 ตามหา "ร่างจริง" ของผู้เล่น (ที่เป็นอมตะและซ่อนอยู่)
        // ต้องหาแบบ Include Inactive (true) เพราะ BoardGameGroup อาจซ่อนมันไว้
        PlayerState[] allPlayers = FindObjectsOfType<PlayerState>(true);

        bool found = false;
        foreach (var player in allPlayers)
        {
            // เงื่อนไข: เป็นคน (!isAI) และ เป็นตัวอมตะ (อยู่ scene -1 หรือ DontDestroyOnLoad)
            if (!player.isAI && player.gameObject.scene.buildIndex == -1)
            {
                Debug.Log($"✅ พบผู้เล่นตัวจริง: {player.name} -> เพิ่ม WinCount และ EXP");

                // 2. 🎯 สั่งบวก WinCount และ Exp เข้าตัวจริงทันที
                player.RecordBattleWin();

                // 3. 💰 สุ่มเครดิตรางวัลหลังชนะ Battle
                int reward = Random.Range(minBattleCreditReward, maxBattleCreditReward + 1);
                player.PlayerCredit += reward;

                // Sync เข้ากับ PlayerData ที่เป็นข้อมูลหลักให้เครดิตคงอยู่ข้ามด่าน/ข้ามซีน
                if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
                {
                    GameData.Instance.selectedPlayer.AddCredit(reward);
                }

                Debug.Log($"💰 ได้รับเครดิตจาก Battle +{reward} (รวมตอนนี้ {player.PlayerCredit})");

                found = true;
                break;
            }
        }

        if (!found) Debug.LogWarning("⚠ หาผู้เล่นตัวจริงไม่เจอ! (ค่า WinCount อาจไม่อัปเดต)");

        // 3. 🏠 ย้ายกลับบ้าน (กลับฉากบอร์ดล่าสุดที่จำไว้)
        string targetBoardScene = PlayerPrefs.GetString(GameEventManager.LastBoardSceneKey, "TestMain");
        SceneManager.LoadScene(targetBoardScene);
    }
}
