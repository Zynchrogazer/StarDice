using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleRewardButton : MonoBehaviour
{
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

                found = true;
                break;
            }
        }

        if (!found) Debug.LogWarning("⚠ หาผู้เล่นตัวจริงไม่เจอ! (ค่า WinCount อาจไม่อัปเดต)");

        // 3. 🏠 ย้ายกลับบ้าน (ตามโค้ดเดิมของคุณ)
        SceneManager.LoadScene("TestMain");
    }
}