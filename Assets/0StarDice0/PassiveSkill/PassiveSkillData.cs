using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Passive Skill", menuName = "Skill Tree/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    // ใช้ชื่อไฟล์เป็น ID (ห้ามตั้งชื่อซ้ำกันนะ)
    public string skillID => this.name;

    public string skillName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;

    [Header("Requirements")]
    // ต้องปลดล็อคสกิลพวกนี้ก่อน ถึงจะอัปอันนี้ได้
    public List<PassiveSkillData> requiredSkills;

    // (แถม) ราคาพอยต์ที่ต้องใช้
    public int costPoint = 1;

    [Header("Passive Bonus")]
    public int bonusAttack = 0;
    public int bonusMaxHP = 0;
    public int bonusStar = 0;

    public bool HasAnyBonus()
    {
        return bonusAttack != 0 || bonusMaxHP != 0 || bonusStar != 0;
    }
}
