using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    // รายชื่อ ID ของสกิลที่ปลดล็อคไปแล้ว
    public HashSet<string> unlockedSkillIDs = new HashSet<string>();

    public int playerSkillPoints = 5; // สมมติว่ามีแต้ม 5 แต้ม

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // (Test) สกิลเริ่มต้นอาจจะให้ฟรี
        // UnlockSkill("Skill_Basic_01"); 
    }

    // เช็คว่าสกิลนี้ "ปลดล็อคไปแล้วหรือยัง"
    public bool IsUnlocked(PassiveSkillData skill)
    {
        return unlockedSkillIDs.Contains(skill.skillID);
    }

    // เช็คว่าสกิลนี้ "สามารถอัปได้ไหม" (เงื่อนไขครบไหม)
    public bool CanUnlock(PassiveSkillData skill)
    {
        // 1. ถ้าปลดไปแล้ว ก็ไม่ต้องปลดอีก
        if (IsUnlocked(skill)) return false;

        // 2. เช็คแต้มพอไหม
        if (playerSkillPoints < skill.costPoint) return false;

        // 3. (หัวใจสำคัญ) เช็คว่าสกิลก่อนหน้า (Prerequisites) ปลดครบทุกอันหรือยัง
        foreach (var req in skill.requiredSkills)
        {
            if (!IsUnlocked(req))
            {
                return false; // มีอันนึงยังไม่ปลด -> อัปไม่ได้
            }
        }

        return true; // ผ่านทุกเงื่อนไข
    }

    // สั่งปลดล็อค
    public bool TryUnlockSkill(PassiveSkillData skill)
    {
        if (CanUnlock(skill))
        {
            playerSkillPoints -= skill.costPoint;
            unlockedSkillIDs.Add(skill.skillID);

            // แจ้งเตือน UI ให้อัปเดตใหม่ทั้งหน้า
            OnSkillTreeUpdated?.Invoke();
            return true;
        }
        return false;
    }

    // Event เอาไว้บอกปุ่มต่างๆ ให้รีเฟรชสี
    public System.Action OnSkillTreeUpdated;
}