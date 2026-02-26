using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleRewardButton : MonoBehaviour
{
    [Header("Money Reward")]
    [SerializeField] private int minBattleMoneyReward = 30;
    [SerializeField] private int maxBattleMoneyReward = 120;

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

                // 3. 💰 สุ่มเงินรางวัลหลังชนะ Battle
                int reward = Random.Range(minBattleMoneyReward, maxBattleMoneyReward + 1);
                player.PlayerMoney += reward;

                // Sync เข้ากับ PlayerData ที่เป็นข้อมูลหลักให้เงินคงอยู่ข้ามด่าน/ข้ามซีน
                if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
                {
                    GameData.Instance.selectedPlayer.AddMoney(reward);
                }

                Debug.Log($"💰 ได้รับเงินจาก Battle +{reward} (รวมตอนนี้ {player.PlayerMoney})");

                found = true;
                break;
            }
        }

        if (!found) Debug.LogWarning("⚠ หาผู้เล่นตัวจริงไม่เจอ! (ค่า WinCount อาจไม่อัปเดต)");

        // 3. 🏠 ย้ายกลับบ้าน (ตามโค้ดเดิมของคุณ)
        SceneManager.LoadScene("TestMain");
    }
}
