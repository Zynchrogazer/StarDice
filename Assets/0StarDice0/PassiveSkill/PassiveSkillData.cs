﻿using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Passive Skill", menuName = "Skill Tree/Passive Skill Data")]
public class PassiveSkillData : ScriptableObject
{
    public enum PassiveStatType
    {
        Mixed = 0,
        Attack = 1,
        MaxHP = 2,
        Star = 3,
        Speed = 4,
        Defense = 5
    }

    // ใช้ชื่อไฟล์เป็น ID (ห้ามตั้งชื่อซ้ำกันนะ)
    public string skillID => this.name;

    public string skillName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;

    [Header("Requirements")]
    public bool useRequiredSkills = false;
    // ต้องปลดล็อคสกิลพวกนี้ก่อน ถึงจะอัปอันนี้ได้
    public List<PassiveSkillData> requiredSkills;

    // (แถม) ราคาพอยต์ที่ต้องใช้
    public int costPoint = 1;

    [Header("Passive Bonus")]
    [Tooltip("ใช้เพื่อจัดหมวดหมู่สกิลใน Inspector/UI; ไม่ได้จำกัดการคำนวณโบนัส.")]
    public PassiveStatType passiveType = PassiveStatType.Mixed;
    public int bonusAttack = 0;
    public int bonusMaxHP = 0;
    public int bonusStar = 0;
    public int bonusSpeed = 0;
    public int bonusDefense = 0;

    public bool HasAnyBonus()
    {
        return bonusAttack != 0 || bonusMaxHP != 0 || bonusStar != 0 || bonusSpeed != 0 || bonusDefense != 0;
    }
}
