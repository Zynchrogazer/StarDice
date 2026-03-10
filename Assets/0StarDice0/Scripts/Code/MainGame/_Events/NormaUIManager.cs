using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NormaUIManager : MonoBehaviour
{
    [Header("Selection Panel (Popup)")]
    public GameObject selectionPanel;
    public TextMeshProUGUI titleText;
    public Button starBtn;
    public Button winBtn;
    public TextMeshProUGUI starBtnText;
    public TextMeshProUGUI winBtnText;

    [Header("Info Display (On Screen HUD)")]
    public TextMeshProUGUI currentRankText;   // ลาก Text ที่มุมจอมาใส่ตรงนี้
    public TextMeshProUGUI currentGoalText;   // ลาก Text ที่มุมจอมาใส่ตรงนี้

    private void Start()
    {
        // 1. ซ่อน Popup เลือก Norma ไว้ก่อน
        if (selectionPanel != null) selectionPanel.SetActive(false);

        // 2. Setup ปุ่มเลือก
        if (starBtn != null)
        {
            starBtn.onClick.RemoveAllListeners();
            starBtn.onClick.AddListener(() => OnChoose(NormaType.Stars));
        }

        if (winBtn != null)
        {
            winBtn.onClick.RemoveAllListeners();
            winBtn.onClick.AddListener(() => OnChoose(NormaType.Wins));
        }

        // 3. 🔥 หัวใจสำคัญ: ดึงข้อมูลจาก System ที่เป็นอมตะ มาโชว์ทันทีที่เกิด
        UpdateInfoUI();
    }

    public void UpdateInfoUI()
    {
        // ถ้า System ยังไม่พร้อม (เผื่อไว้) ก็ไม่ต้องทำ
        if (NormaSystem.Instance == null) return;

        // อัปเดต Rank
        if (currentRankText != null)
            currentRankText.text = $"Rank: {NormaSystem.Instance.currentNormaRank}";

        // อัปเดต Goal
        if (currentGoalText != null)
        {
            string typeStr = (NormaSystem.Instance.selectedNorma == NormaType.Stars) ? "Stars" : "Wins";
            currentGoalText.text = $"Goal: {NormaSystem.Instance.targetAmount} {typeStr}";
        }
    }

    // ... (ส่วน ShowSelectionPanel และ OnChoose เหมือนเดิม) ...
    public void ShowSelectionPanel(int nextLevel)
    {
        if (selectionPanel == null) return;
        selectionPanel.SetActive(true);
        if (titleText != null) titleText.text = $"Select your quest!";

        if (NormaSystem.Instance != null)
        {
            string starReqText = NormaSystem.Instance.GetRequirementText(nextLevel, NormaType.Stars);
            string winReqText = NormaSystem.Instance.GetRequirementText(nextLevel, NormaType.Wins);
            if (starBtnText != null) starBtnText.text = $"Collect {starReqText} Stars";
            if (winBtnText != null) winBtnText.text = $"Win {winReqText} Battles";
        }
    }

    private void OnChoose(NormaType type)
    {
        if (NormaSystem.Instance != null) NormaSystem.Instance.SelectNorma(type);
        if (selectionPanel != null) selectionPanel.SetActive(false);
    }
}
