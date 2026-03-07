using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillSelectUI : MonoBehaviour
{
    public PlayerData playerData;

    [Header("Panel & Buttons")]
    public GameObject panel;
    public Button[] changeSkillButtons = new Button[10];
    public Image[] skillImages = new Image[10];
    public TMP_Text[] skillNames = new TMP_Text[10];
    public TMP_Text[] skillActionLabels = new TMP_Text[10];

    [Header("Random Unlock")]
    public Button randomUnlockButton;
    public Image randomUnlockImage;
    public TMP_Text randomUnlockName;
    public GameObject randomText; // Text ที่จะ Active/Deactivate

    public Button closePanelButton;

    private bool randomButtonClicked = false;
    private bool wasPanelActive = false;

 
    public Image skillSlot1; // ลากช่องรูปสกิล 1 มาใส่
    public Image skillSlot2; // ลากช่องรูปสกิล 2 มาใส่
    public Image skillSlot3; // ลากช่องรูปสกิล 3 มาใส่
    void Start()
    {
        // sync จาก runtime player ก่อน แล้วค่อยรีเซ็ตล็อกสกิล
        SyncPlayerDataFromRuntime();
        ResetSkillLocksForStageStart();

        // ตั้งปุ่มสกิล
        if (playerData == null)
        {
            Debug.LogError($"[{GetType().Name}] playerData is null after runtime sync. Please check inspector/GameData binding.");
            return;
        }

        if (playerData.allSkills.Length >= 3 && playerData.skills.Length >= 3)
        {
            // บังคับยัดข้อมูลสกิล 1, 2, 3 เข้าช่อง
            playerData.skills[0] = playerData.allSkills[0];
            playerData.skills[1] = playerData.allSkills[1];
            playerData.skills[2] = playerData.allSkills[2];

            // บังคับเปลี่ยนรูปภาพหน้าจอ ให้ตรงกับที่เพิ่งยัดเข้าไปเป๊ะๆ
            if (skillSlot1 != null) skillSlot1.sprite = playerData.skills[0].icon;
            if (skillSlot2 != null) skillSlot2.sprite = playerData.skills[1].icon;
            if (skillSlot3 != null) skillSlot3.sprite = playerData.skills[2].icon;
        }
        
        for (int i = 0; i < changeSkillButtons.Length; i++)
        {
            int index = i;
            if (changeSkillButtons[i] == null) continue;

            changeSkillButtons[i].onClick.AddListener(() =>
            {
                SkillData selectedSkill = playerData.allSkills[index];

                if (selectedSkill.isLocked)
                {
                    Debug.LogWarning($"สกิล {selectedSkill.skillName} ถูกล็อคอยู่");
                    return;
                }

                // ป้องกันซ้ำกับ skill[0–2]
                if (selectedSkill == playerData.skills[0] ||
                    selectedSkill == playerData.skills[1] ||
                    selectedSkill == playerData.skills[2])
                {
                    Debug.LogWarning("สกิลนี้ถูกเลือกไว้แล้วใน skill[0–2]");
                    return;
                }

                playerData.skills[0] = selectedSkill;
                Debug.Log($"✔ เลือกสกิล skill[0]: {selectedSkill.skillName}");

                RefreshSkillButtons();
            });
        }

        if (randomUnlockButton != null)
            randomUnlockButton.onClick.AddListener(RandomUnlockSkill);

        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(ClosePanel);

        RefreshSkillButtons();
        wasPanelActive = panel != null && panel.activeInHierarchy;
    }

    private void Update()
    {
        bool isPanelActive = panel != null && panel.activeInHierarchy;
        if (isPanelActive && !wasPanelActive)
        {
            SyncPlayerDataFromRuntime();
            ResetSkillLocksForStageStart();
            RefreshSkillButtons();
        }

        wasPanelActive = isPanelActive;
    }

private void ResetSkillLocksForStageStart()
{
    if (playerData == null) return;

    string levelSource;
    int currentLevel = ResolveCurrentPlayerLevel(out levelSource);
    playerData.ResetSkillLocksForStageStart(currentLevelOverride: currentLevel);
    Debug.Log($"[{GetType().Name}] Reset skill lock using level {currentLevel} (source: {levelSource}, playerData: {playerData.name})");
}

private void SyncPlayerDataFromRuntime()
{
    PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
    if (currentPlayer != null && !currentPlayer.isAI && currentPlayer.selectedPlayerPreset != null)
    {
        if (playerData != null && playerData != currentPlayer.selectedPlayerPreset)
        {
            Debug.LogWarning($"[{GetType().Name}] playerData binding mismatch ({playerData.name}) -> sync to runtime preset ({currentPlayer.selectedPlayerPreset.name})");
        }

        playerData = currentPlayer.selectedPlayerPreset;
        return;
    }

    if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
    {
        if (playerData != null && playerData != GameData.Instance.selectedPlayer)
        {
            Debug.LogWarning($"[{GetType().Name}] playerData binding mismatch ({playerData.name}) -> sync to GameData selectedPlayer ({GameData.Instance.selectedPlayer.name})");
        }

        playerData = GameData.Instance.selectedPlayer;
    }
}

private int ResolveCurrentPlayerLevel(out string source)
{
    PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
    if (currentPlayer != null && !currentPlayer.isAI)
    {
        source = "GameTurnManager.CurrentPlayer.PlayerLevel";
        return Mathf.Max(1, currentPlayer.PlayerLevel);
    }

    PlayerState[] players = FindObjectsOfType<PlayerState>(true);
    foreach (PlayerState player in players)
    {
        if (player == null || player.isAI) continue;
        source = "FindObjectsOfType<PlayerState>.PlayerLevel";
        return Mathf.Max(1, player.PlayerLevel);
    }

    source = "playerData.level (fallback)";
    return Mathf.Max(1, playerData != null ? playerData.level : 1);
}

    public void RefreshSkillButtons()
    {
        if (playerData == null || playerData.allSkills == null || playerData.skills == null) return;
        for (int i = 0; i < changeSkillButtons.Length; i++)
        {
            Button btn = changeSkillButtons[i];
            if (btn == null || playerData.allSkills[i] == null) continue;

            SkillData thisSkill = playerData.allSkills[i];

            bool isUsed = (thisSkill == playerData.skills[0] ||
                           thisSkill == playerData.skills[1] ||
                           thisSkill == playerData.skills[2]);

            btn.interactable = !thisSkill.isLocked && !isUsed;

            ColorBlock colors = btn.colors;
            colors.normalColor = (!btn.interactable) ? Color.gray : Color.white;
            colors.highlightedColor = (!btn.interactable) ? Color.gray : Color.white;
            colors.pressedColor = (!btn.interactable) ? Color.gray : Color.white;
            colors.selectedColor = (!btn.interactable) ? Color.gray : Color.white;
            btn.colors = colors;

            if (skillImages.Length > i && skillImages[i] != null)
                skillImages[i].sprite = thisSkill.icon;

            if (skillNames.Length > i && skillNames[i] != null)
                skillNames[i].text = thisSkill.skillName;

            TMP_Text actionLabel = null;
            if (skillActionLabels.Length > i)
            {
                actionLabel = skillActionLabels[i];
            }

            if (actionLabel == null && btn != null)
            {
                actionLabel = btn.GetComponentInChildren<TMP_Text>(true);
            }

            if (actionLabel != null)
            {
                actionLabel.text = thisSkill.isLocked ? "Locked" : (isUsed ? "Selected" : "Select");
            }
        }

        if (randomUnlockButton != null)
            randomUnlockButton.interactable = !randomButtonClicked;
    }

    private void RandomUnlockSkill()
    {
        if (randomButtonClicked) return;
        if (playerData == null || playerData.allSkills == null) return;

    List<int> lockedIndexes = new List<int>();
    for (int i = 0; i < playerData.allSkills.Length; i++)
    {
        if (playerData.allSkills[i] != null && playerData.allSkills[i].isLocked)
            lockedIndexes.Add(i);
    }

    if (lockedIndexes.Count == 0)
    {
        Debug.Log("ไม่มีสกิลล็อคเหลือให้สุ่ม");
        return;
    }

    int randomIndex = lockedIndexes[Random.Range(0, lockedIndexes.Count)];
    SkillData unlockedSkill = playerData.allSkills[randomIndex];
    unlockedSkill.isLocked = false;

    // แสดง icon และชื่อ
    if (randomUnlockImage != null) randomUnlockImage.sprite = unlockedSkill.icon;
    if (randomUnlockName != null) 
    {
        randomUnlockName.text = unlockedSkill.skillName;
        randomUnlockName.gameObject.SetActive(true); // ✅ Active ตอนสุ่ม
    }

    if (randomText != null)
        randomText.SetActive(true); // ✅ Active ตอนสุ่ม

    randomButtonClicked = true;
    RefreshSkillButtons();
}

private void ClosePanel()
{
    if (panel != null) panel.SetActive(false);

    // รีเซ็ตปุ่มสุ่ม
    randomButtonClicked = false;
    if (randomUnlockButton != null)
    {
        randomUnlockButton.interactable = true;
        ColorBlock colors = randomUnlockButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        randomUnlockButton.colors = colors;
    }

    // ปิดชื่อและข้อความสุ่ม
    if (randomUnlockName != null) randomUnlockName.gameObject.SetActive(false);
    if (randomText != null) randomText.SetActive(false);

    RefreshSkillButtons();
}

}
