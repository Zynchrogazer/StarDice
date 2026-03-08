using UnityEngine;

public class BoardgameTestAddCreditButton : MonoBehaviour
{
    [SerializeField] private int bonusCredit = 500;

    // ผูกกับปุ่มใน OnClick()
    public void AddCreditForBoardgameTest()
    {
        PlayerState player = FindMainPlayer();
        if (player == null)
        {
            Debug.LogWarning("[BoardgameTestAddCreditButton] ไม่พบ PlayerState ของผู้เล่นจริง");
            return;
        }

        player.PlayerCredit += Mathf.Max(0, bonusCredit);

        // sync กลับข้อมูลหลัก เพื่อให้ตอนออก Intermission เครดิตยังคงอยู่
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.AddCredit(Mathf.Max(0, bonusCredit));
        }

        Debug.Log($"[BoardgameTestAddCreditButton] เพิ่มเครดิตทดสอบ +{bonusCredit} -> ตอนนี้ {player.PlayerCredit}");
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
