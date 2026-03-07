using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class CharacterRewardSetup
{
    public string characterName;
    public List<Button> startingSkills = new List<Button>();
    public List<GameObject> rewardPanels = new List<GameObject>();
    [HideInInspector] public int nextMilestoneIndex = 0;
}

public class LevelRewardUI : MonoBehaviour
{
    [Header("References")]
    public PlayerState player;
    public TMP_Text levelText;

    [Header("Character & Reward Setup")]
    public List<CharacterRewardSetup> rewardSetups = new List<CharacterRewardSetup>();

    private IEnumerator Start()
    {
        foreach (var setup in rewardSetups)
        {
            foreach (var panel in setup.rewardPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
            foreach (var btn in setup.startingSkills)
            {
                if (btn != null) btn.gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(0.1f);

        if (player != null)
        {
            ResetMilestoneProgress();
            player.OnStatsUpdated += HandleStatsUpdated;
            UpdateLevelText();
            UnlockStartingSkills();
            CheckLevelRewards();
        }
    }

    private void HandleStatsUpdated()
    {
        UpdateLevelText();
        CheckLevelRewards();
    }

    private void UpdateLevelText()
    {
        if (levelText != null && player != null)
        {
            levelText.text = "Lv. " + player.PlayerLevel.ToString();
        }
    }

    private void CheckLevelRewards()
    {
        if (player == null || player.selectedPlayerPreset == null || player.selectedPlayerPreset.allSkills == null) return;

        int totalSkills = player.selectedPlayerPreset.allSkills.Length;
        int targetUnlockedCount = player.GetTargetUnlockedSkillCountByLevel(totalSkills);
        string currentPlayerName = GetActiveCharacterName();

        foreach (var setup in rewardSetups)
        {
            bool isCorrectCharacter = string.IsNullOrEmpty(setup.characterName)
                                      || string.Equals(setup.characterName.Trim(), currentPlayerName.Trim(), System.StringComparison.OrdinalIgnoreCase);
            if (!isCorrectCharacter) continue;

            while (player.GetRuntimeUnlockedSkillCount() < targetUnlockedCount)
            {
                bool unlocked = player.UnlockRandomLockedSkill(totalSkills, out int unlockedIndex);
                if (!unlocked)
                {
                    return;
                }

                if (setup.nextMilestoneIndex < setup.rewardPanels.Count)
                {
                    GameObject panelToShow = setup.rewardPanels[setup.nextMilestoneIndex];
                    if (panelToShow != null)
                    {
                        panelToShow.SetActive(true);
                    }
                }

                SkillData unlockedSkill = player.selectedPlayerPreset.allSkills[unlockedIndex];
                string skillName = unlockedSkill != null ? unlockedSkill.skillName : $"Index {unlockedIndex}";
                Debug.Log($"🎉 เลเวลอัพ! ปลดล็อคสกิล '{skillName}' (Lv.{player.PlayerLevel})");

                setup.nextMilestoneIndex++;
            }
        }
    }

    private void UnlockStartingSkills()
    {
        if (player.selectedPlayerPreset == null) return;

        string currentPlayerName = GetActiveCharacterName();

        foreach (var setup in rewardSetups)
        {
            string setupName = setup.characterName == null ? string.Empty : setup.characterName.Trim();
            if (string.Equals(setupName, currentPlayerName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                foreach (Button btn in setup.startingSkills)
                {
                    if (btn != null)
                    {
                        btn.gameObject.SetActive(true);
                    }
                }
                break;
            }
        }
    }

    private string GetActiveCharacterName()
    {
        if (player?.selectedPlayerPreset == null) return string.Empty;

        if (!string.IsNullOrWhiteSpace(player.selectedPlayerPreset.playerName))
        {
            return player.selectedPlayerPreset.playerName.Trim();
        }

        return player.selectedPlayerPreset.name.Trim();
    }

    private void ResetMilestoneProgress()
    {
        string currentPlayerName = GetActiveCharacterName();

        foreach (var setup in rewardSetups)
        {
            if (setup == null) continue;

            bool isCorrectCharacter = string.IsNullOrEmpty(setup.characterName)
                                      || string.Equals(setup.characterName.Trim(), currentPlayerName.Trim(), System.StringComparison.OrdinalIgnoreCase);

            setup.nextMilestoneIndex = isCorrectCharacter ? 0 : 0;
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnStatsUpdated -= HandleStatsUpdated;
        }
    }
}
