using UnityEngine;

public class BoardgameTestAddLevelButton : MonoBehaviour
{
    // ผูกกับปุ่มใน OnClick()
    public void AddOneLevelForBoardgameTest()
    {
        PlayerState player = FindMainPlayer();
        if (player == null)
        {
            Debug.LogWarning("[BoardgameTestAddLevelButton] ไม่พบ PlayerState ของผู้เล่นจริง");
            return;
        }

        int neededExp = Mathf.Max(1, player.MaxExp - player.CurrentExp);
        player.GainExp(neededExp);

        Debug.Log($"[BoardgameTestAddLevelButton] เพิ่มเลเวลทดสอบ 1 ระดับ -> Lv.{player.PlayerLevel}");
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
