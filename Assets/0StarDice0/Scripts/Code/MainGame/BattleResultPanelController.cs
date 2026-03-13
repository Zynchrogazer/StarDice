using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Optional Result Text")]
    [SerializeField] private TMP_Text resultText;

    [Header("Optional Result Image")]
    [SerializeField] private Image rewardPreviewImage;

    [Header("Win Actions")]
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private Button restartToInterMissionButton;

    [Header("Lose Actions")]
    [SerializeField] private Button loseRestartButton;

    [Header("Reward")]
    [SerializeField] private int minBattleCreditReward = 30;
    [SerializeField] private int maxBattleCreditReward = 120;

    private void Awake()
    {
        BindButtons();
    }

    private void BindButtons()
    {
        BindButton(claimRewardButton, OnClaimRewardClicked);
        BindButton(restartToInterMissionButton, OnRestartToInterMissionClicked);
        BindButton(loseRestartButton, OnRestartToInterMissionClicked);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null || callback == null) return;

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    public void ShowWin(string message = "Victory!")
    {
        SetResult(message);
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void ShowLose(string message = "You Lose!")
    {
        SetResult(message);
        if (losePanel != null) losePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void HideAll()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void OnClaimRewardClicked()
    {
        int minReward = Mathf.Min(minBattleCreditReward, maxBattleCreditReward);
        int maxReward = Mathf.Max(minBattleCreditReward, maxBattleCreditReward);
        BattleResultFlowService.HandleRewardAndReturnToBoard(minReward, maxReward);
    }

    public void OnRestartToInterMissionClicked()
    {
        BattleResultFlowService.HandleRestartToInterMission();
    }

    public void SetRewardImage(Sprite sprite)
    {
        if (rewardPreviewImage == null) return;

        rewardPreviewImage.sprite = sprite;
        rewardPreviewImage.gameObject.SetActive(sprite != null);
    }

    public void HideRewardImage()
    {
        if (rewardPreviewImage == null) return;
        rewardPreviewImage.gameObject.SetActive(false);
    }

    private void SetResult(string message)
    {
        if (resultText == null) return;
        resultText.text = message;
    }
}
