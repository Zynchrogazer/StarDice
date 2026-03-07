using UnityEngine;

public class BoardgameTestAddMoneyButton : MonoBehaviour
{
    [SerializeField] private int bonusMoney = 500;

    // ผูกกับปุ่มใน OnClick()
    public void AddMoneyForBoardgameTest()
    {
        PlayerState player = FindMainPlayer();
        if (player == null)
        {
            Debug.LogWarning("[BoardgameTestAddMoneyButton] ไม่พบ PlayerState ของผู้เล่นจริง");
            return;
        }

        player.PlayerMoney += Mathf.Max(0, bonusMoney);

        // sync กลับข้อมูลหลัก เพื่อให้ตอนออก Intermission เงินยังคงอยู่
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.AddMoney(Mathf.Max(0, bonusMoney));
        }

        Debug.Log($"[BoardgameTestAddMoneyButton] เพิ่มเงินทดสอบ +{bonusMoney} -> ตอนนี้ {player.PlayerMoney}");
    }

    private PlayerState FindMainPlayer()
    {
        PlayerState[] players = FindObjectsOfType<PlayerState>(true);

        foreach (PlayerState player in players)
        {
            if (!player.isAI && player.gameObject.scene.buildIndex == -1)
            {
                return player;
            }
        }

        return null;
    }
}
