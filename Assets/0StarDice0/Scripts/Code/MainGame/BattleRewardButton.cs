using UnityEngine;

public class BattleRewardButton : MonoBehaviour
{
    [Header("Credit Reward")]
    [SerializeField] private int minBattleCreditReward = 30;
    [SerializeField] private int maxBattleCreditReward = 120;

    // เรียกฟังก์ชันนี้เมื่อกดปุ่ม "รับรางวัล" (Link ผ่าน OnClick ใน Inspector)
    public void OnClaimRewardClicked()
    {
        BattleResultFlowService.HandleRewardAndReturnToBoard(minBattleCreditReward, maxBattleCreditReward);
    }

    public void OnRestartToInterMissionClicked()
    {
        BattleResultFlowService.HandleRestartToInterMission();
    }

}
