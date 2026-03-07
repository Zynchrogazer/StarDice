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
    private const string RewardMilestoneKeyPrefix = "LEVEL_REWARD_MILESTONE_";

    [Header("References")]
    public PlayerState player;
    public TMP_Text levelText;
    
    // ❌ ลบ public PlayerData playerData; ออกไปแล้ว ไม่ต้องลากใส่เองแล้วครับ!

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
        if (player == null || player.selectedPlayerPreset == null) return;
        string currentPlayerName = GetActiveCharacterName();

        foreach (var setup in rewardSetups)
        {
            bool isCorrectCharacter = string.IsNullOrEmpty(setup.characterName)
                                      || string.Equals(setup.characterName.Trim(), currentPlayerName.Trim(), System.StringComparison.OrdinalIgnoreCase);
            if (!isCorrectCharacter) continue;

            int milestoneReached = player.PlayerLevel / 10;

            while (setup.nextMilestoneIndex < milestoneReached)
            {
                // ปลดล็อคสกิลตาม milestone เสมอ แม้ไม่ได้ตั้ง reward panel ไว้
                AutoUnlockOneSkill();

                if (setup.nextMilestoneIndex < setup.rewardPanels.Count)
                {
                    GameObject panelToShow = setup.rewardPanels[setup.nextMilestoneIndex];
                    if (panelToShow != null)
                    {
                        panelToShow.SetActive(true);
                    }
                }
                
                setup.nextMilestoneIndex++; 
                SaveMilestoneProgress(setup);
            }
        }
    }

    // ✅ ฟังก์ชันสุ่มปลดล็อค (อัปเดตใหม่ ให้ดึง Data จากตัวละครที่เลือก)
    private void AutoUnlockOneSkill()
    {
        // ดึง PlayerData ของตัวละครที่กำลังเล่นอยู่มาใช้
        PlayerData activeData = player.selectedPlayerPreset;

        if (activeData == null) 
        {
            Debug.LogError("ไม่พบข้อมูลตัวละครที่กำลังเล่นอยู่!");
            return;
        }

        List<int> lockedIndexes = new List<int>();
        
        // ค้นหาสกิลที่ล็อคอยู่ของตัวละครนี้
        for (int i = 0; i < activeData.allSkills.Length; i++)
        {
            if (activeData.allSkills[i] != null && activeData.allSkills[i].isLocked)
                lockedIndexes.Add(i);
        }

        if (lockedIndexes.Count == 0)
        {
            Debug.Log($"ตัวละคร {activeData.name} ไม่มีสกิลล็อคเหลือให้สุ่มแล้วจ้า");
            return;
        }

        // สุ่มมา 1 อัน และปลดล็อคใน PlayerData ของตัวละครนั้นเลย
        int randomIndex = lockedIndexes[Random.Range(0, lockedIndexes.Count)];
        activeData.allSkills[randomIndex].isLocked = false;

        Debug.Log($"🎉 เลเวลอัพ! ปลดล็อคสกิล '{activeData.allSkills[randomIndex].skillName}' ให้ตัวละคร '{activeData.name}' สำเร็จ!");
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

            setup.nextMilestoneIndex = isCorrectCharacter ? LoadMilestoneProgress(setup) : 0;
        }
    }

    private string GetMilestoneSaveKey(CharacterRewardSetup setup)
    {
        string activeCharacter = GetActiveCharacterName();
        string setupCharacter = (setup?.characterName ?? string.Empty).Trim();
        string keyCharacter = string.IsNullOrEmpty(setupCharacter) ? activeCharacter : setupCharacter;

        if (string.IsNullOrEmpty(keyCharacter))
        {
            keyCharacter = "UnknownCharacter";
        }

        return $"{RewardMilestoneKeyPrefix}{keyCharacter}";
    }

    private void SaveMilestoneProgress(CharacterRewardSetup setup)
    {
        if (setup == null) return;

        string saveKey = GetMilestoneSaveKey(setup);
        PlayerPrefs.SetInt(saveKey, Mathf.Max(0, setup.nextMilestoneIndex));
        PlayerPrefs.Save();
    }

    private int LoadMilestoneProgress(CharacterRewardSetup setup)
    {
        if (setup == null) return 0;

        string saveKey = GetMilestoneSaveKey(setup);
        return Mathf.Max(0, PlayerPrefs.GetInt(saveKey, 0));
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnStatsUpdated -= HandleStatsUpdated;
        }
    }

}
