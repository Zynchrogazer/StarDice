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

    [Header("Submit Quest Panel")]
    public GameObject submitPanel; // หน้าต่างถามว่า "ส่งเควสไหม?"
    public Button submitBtn;       // ปุ่ม "ส่งเควส (Submit)"

    [Header("Info Display (On Screen HUD) - ของเดิม")]
    public TextMeshProUGUI currentRankText;
    public TextMeshProUGUI currentGoalText;

    [Header("Detailed Progress HUD (ช่องใหม่ที่เพิ่งเพิ่ม)")]
    public TextMeshProUGUI rankProgressText; // สำหรับโชว์: Rank 1 / 6
    public TextMeshProUGUI starProgressText; // สำหรับโชว์: Stars 10 / 30
    public TextMeshProUGUI winProgressText;  // สำหรับโชว์: Wins 1 / 2

    [Header("Max Rank UI")]
    public TextMeshProUGUI finalPhaseText; // ลาก Text ตัวใหม่ที่จะโชว์ตอนเลเวลตันมาใส่ตรงนี้
    public Image finalPhaseImage;
    public GameObject[] objectsToHideOnBossPhase;

    // ตัวแปรสำหรับคอยเช็คว่าดาว/การชนะ เปลี่ยนแปลงไปหรือยัง (เอาไว้อัปเดต UI แบบ Real-time)
    private int lastStars = -1;
    private int lastWins = -1;

    private void Start()
    {
        // 1. ซ่อน Popup เลือก Norma ไว้ก่อน
        if (selectionPanel != null) selectionPanel.SetActive(false);


        if (submitPanel != null) submitPanel.SetActive(false);
        
        // 🟢 ผูกปุ่มกดส่งเควส
        if (submitBtn != null)
        {
            submitBtn.onClick.RemoveAllListeners();
            submitBtn.onClick.AddListener(OnSubmitClicked);
        }

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



        // 3. ดึงข้อมูลมาโชว์ทันทีที่เกิด
        UpdateInfoUI();
    }

 private void Update()
    {
        // 🟢 เปลี่ยนจาก GameTurnManager.CurrentPlayer เป็น GetHumanPlayer()
        PlayerState human = GetHumanPlayer();
        if (human != null)
        {
            int currentStars = human.PlayerStar;
            int currentWins = human.WinCount;

            // ถ้ามีการได้ดาวเพิ่ม หรือชนะเพิ่ม ให้สั่งอัปเดตหน้าจอทันที!
            if (currentStars != lastStars || currentWins != lastWins)
            {
                lastStars = currentStars;
                lastWins = currentWins;
                UpdateInfoUI();
            }
        }
    }

public void UpdateInfoUI()
    {
        if (!NormaSystem.TryGet(out var normaSystem)) return;

        bool isMaxRank = normaSystem.currentNormaRank >= normaSystem.maxNormaRank;

        if (isMaxRank)
        {
            // 🛑 ซ่อน UI เควสเดิมทั้งหมด
            if (rankProgressText != null) rankProgressText.gameObject.SetActive(false);
            if (starProgressText != null) starProgressText.gameObject.SetActive(false);
            if (winProgressText != null) winProgressText.gameObject.SetActive(false);
            if (currentGoalText != null) currentGoalText.gameObject.SetActive(false);
            if (currentRankText != null) currentRankText.gameObject.SetActive(false);

            // 🛑 ปิดรูปภาพอีก 3 รูป (หรืออะไรก็ตามที่คุณลากมาใส่ใน Inspector)
            if (objectsToHideOnBossPhase != null)
            {
                foreach (var obj in objectsToHideOnBossPhase)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            // 🌟 เปิด Text ตัวใหม่
            if (finalPhaseText != null)
            {
                finalPhaseText.gameObject.SetActive(true);
                finalPhaseText.text = "WARNING: BOSS APPEARED!"; 
            }

            // 🌟 เปิด Image ตัวใหม่
            if (finalPhaseImage != null)
            {
                finalPhaseImage.gameObject.SetActive(true);
            }
            
            return; // จบการทำงาน
        }
        else
        {
            // 🟢 ถ้ายังไม่ตัน: ให้แน่ใจว่า UI เดิมเปิดอยู่
            if (rankProgressText != null) rankProgressText.gameObject.SetActive(true);
            if (starProgressText != null) starProgressText.gameObject.SetActive(true);
            if (winProgressText != null) winProgressText.gameObject.SetActive(true);
            if (currentGoalText != null) currentGoalText.gameObject.SetActive(true);
            if (currentRankText != null) currentRankText.gameObject.SetActive(true);

            // 🟢 เปิดรููปภาพทั้ง 3 รูป กลับมาเป็นปกติ (เผื่อกรณีเริ่มกระดานใหม่)
            if (objectsToHideOnBossPhase != null)
            {
                foreach (var obj in objectsToHideOnBossPhase)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            // 🛑 ซ่อน Text และ Image ตัวใหม่ (เด้งอันใหม่ทิ้ง)
            if (finalPhaseText != null) finalPhaseText.gameObject.SetActive(false);
            if (finalPhaseImage != null) finalPhaseImage.gameObject.SetActive(false);
        }

        int currentStars = 0;
        int currentWins = 0;
        
        // 🟢 เปลี่ยนมาดึงค่าจาก GetHumanPlayer()
        PlayerState human = GetHumanPlayer();
        if (human != null)
        {
            currentStars = human.PlayerStar;
            currentWins = human.WinCount;
        }

        int nextRank = normaSystem.currentNormaRank + 1;
        if (nextRank > normaSystem.maxNormaRank) nextRank = normaSystem.maxNormaRank;
        
        int reqStars = normaSystem.GetRequirement(nextRank, NormaType.Stars);
        int reqWins = normaSystem.GetRequirement(nextRank, NormaType.Wins);
        
        NormaType activeQuest = normaSystem.selectedNorma;

        if (rankProgressText != null)
            rankProgressText.text = $"Rank: {normaSystem.currentNormaRank} / {normaSystem.maxNormaRank}";
        
        if (currentRankText != null) 
            currentRankText.text = $"Rank: {normaSystem.currentNormaRank}";

        if (starProgressText != null)
        {
            if (activeQuest == NormaType.Stars)
                starProgressText.text = $"Stars: {currentStars} / {reqStars}";
            else
                starProgressText.text = $"Stars: {currentStars}";
        }

        if (winProgressText != null)
        {
            if (activeQuest == NormaType.Wins)
                winProgressText.text = $"Wins: {currentWins} / {reqWins}";
            else
                winProgressText.text = $"Wins: {currentWins}";
        }
    }
    public void ShowSelectionPanel(int nextLevel)
    {
        if (selectionPanel == null) return;
        selectionPanel.SetActive(true);
        if (titleText != null) titleText.text = $"Select your quest!";

        if (NormaSystem.TryGet(out var normaSystem))
        {
            string starReqText = normaSystem.GetRequirementText(nextLevel, NormaType.Stars);
            string winReqText = normaSystem.GetRequirementText(nextLevel, NormaType.Wins);
            if (starBtnText != null) starBtnText.text = $"Collect {starReqText} Stars";
            if (winBtnText != null) winBtnText.text = $"Win {winReqText} Battles";
        }
    }

    private void OnChoose(NormaType type)
    {
        if (NormaSystem.TryGet(out var normaSystem)) normaSystem.SelectNorma(type);
        if (selectionPanel != null) selectionPanel.SetActive(false);

        // สั่งอัปเดตหน้าจอทันทีเมื่อเลือกเควสเสร็จ
        UpdateInfoUI(); 
    }

    // สั่งให้เปิดหน้าต่างนี้ขึ้นมา
    public void ShowSubmitPanel()
    {
        if (submitPanel != null) submitPanel.SetActive(true);
    }

    // เมื่อผู้เล่นกดปุ่ม "ส่งเควส"
// เมื่อผู้เล่นกดปุ่ม "ส่งเควส"
    private void OnSubmitClicked()
    {
        // 1. ปิดหน้าต่างนี้ทิ้งไป
        if (submitPanel != null) submitPanel.SetActive(false);

        // 2. ดำเนินการส่งเควส
        if (NormaSystem.TryGet(out var normaSystem))
        {
            // 🟢 เปลี่ยนให้ดึงตัวคนเล่นมาหักทรัพยากร
            PlayerState player = GetHumanPlayer();
            if (player != null)
            {
                // 🟢 หักค่าทรัพยากรตามประเภทเควสที่เลือกไว้
                if (normaSystem.selectedNorma == NormaType.Stars)
                {
                    player.PlayerStar -= normaSystem.targetAmount;
                    
                    // ป้องกันค่าติดลบเผื่อไว้
                    if (player.PlayerStar < 0) player.PlayerStar = 0; 
                    
                    Debug.Log($"[Norma] จ่าย {normaSystem.targetAmount} ดาว เพื่ออัปเลเวล! (เหลือ {player.PlayerStar} ดาว)");
                }
                else if (normaSystem.selectedNorma == NormaType.Wins)
                {
                    player.WinCount -= normaSystem.targetAmount;
                    
                    if (player.WinCount < 0) player.WinCount = 0;
                    
                    Debug.Log($"[Norma] ใช้ยอดชนะ {normaSystem.targetAmount} ครั้ง เพื่ออัปเลเวล! (เหลือ {player.WinCount} ครั้ง)");
                }
            }

            // 3. สั่งอัปเลเวลของจริง! (มันจะไปเด้งหน้าต่างเลือกเควสถัดไปต่อให้อัตโนมัติ)
            normaSystem.NormaLevelUp(); 
        }
    }

    // 🟢 ฟังก์ชันนี้จะควานหา "ผู้เล่นที่เป็นคน" เท่านั้น
    private PlayerState GetHumanPlayer()
    {
        if (GameTurnManager.TryGet(out var turnManager))
        {
            foreach (PlayerState p in turnManager.allPlayers)
            {
                if (!p.isAI) 
                {
                    return p; // เจอคนเล่นแล้ว ส่งข้อมูลกลับไปเลย!
                }
            }
        }
        return null;
    }
}