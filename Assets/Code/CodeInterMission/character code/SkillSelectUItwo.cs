using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillSelectUItwo : MonoBehaviour
{
    public PlayerData playerData;

    [Header("Panel & Buttons")]
    public GameObject panel;
    public Button[] changeSkillButtons = new Button[10];
    public Image[] skillImages = new Image[10];
    public TMP_Text[] skillNames = new TMP_Text[10];

    [Header("Random Unlock")]
    public Button randomUnlockButton;
    public Image randomUnlockImage;
    public TMP_Text randomUnlockName;
    public GameObject randomText; // Text ที่จะ Active/Deactivate

    public Button closePanelButton;

    private bool randomButtonClicked = false;

    void Start()
    {
        // ล็อคสกิล 3–9 เริ่มต้น
        for (int i = 3; i < playerData.allSkills.Length; i++)
        {
            if (playerData.allSkills[i] != null)
                playerData.allSkills[i].isLocked = true;
        }

        // ตั้งปุ่มสกิล
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

                playerData.skills[1] = selectedSkill;
                Debug.Log($"✔ เลือกสกิล skill[1]: {selectedSkill.skillName}");

                RefreshSkillButtons();
            });
        }

        if (randomUnlockButton != null)
            randomUnlockButton.onClick.AddListener(RandomUnlockSkill);

        if (closePanelButton != null)
            closePanelButton.onClick.AddListener(ClosePanel);

        RefreshSkillButtons();
    }

    public void RefreshSkillButtons()
    {
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
        }

        if (randomUnlockButton != null)
            randomUnlockButton.interactable = !randomButtonClicked;
    }

    private void RandomUnlockSkill()
    {
        if (randomButtonClicked) return;

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
        playerData.allSkills[randomIndex].isLocked = false;

        SkillData unlockedSkill = playerData.allSkills[randomIndex];

        if (randomUnlockImage != null)
            randomUnlockImage.sprite = unlockedSkill.icon;

        if (randomUnlockName != null)
            randomUnlockName.text = unlockedSkill.skillName;

        randomButtonClicked = true;

        if (randomUnlockButton != null)
        {
            randomUnlockButton.interactable = false;
            ColorBlock colors = randomUnlockButton.colors;
            colors.normalColor = Color.green;
            colors.highlightedColor = Color.green;
            colors.pressedColor = Color.green;
            colors.selectedColor = Color.green;
            randomUnlockButton.colors = colors;
        }

        if (randomText != null)
            randomText.SetActive(true);

        RefreshSkillButtons();
    }

    private void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

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

        if (randomText != null)
            randomText.SetActive(false);

        RefreshSkillButtons();
    }
}
