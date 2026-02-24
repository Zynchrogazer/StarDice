using UnityEngine;

public class PassiveSkillManager : MonoBehaviour
{
    public static PassiveSkillManager Instance { get; private set; }

    [Header("Save Data")]
    public int globalGold = 0;          // เงินสะสมถาวร (สำหรับซื้อของ/อัปสกิล)
    public int starSkillLevel = 0;      // เลเวลสกิล: เก็บดาวเพิ่ม
    public int attackSkillLevel = 0;    // เลเวลสกิล: ตีแรงขึ้น

    [Header("Settings")]
    public int baseUpgradeCost = 100;   // ราคาเริ่มต้น

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ห้ามตายเมื่อเปลี่ยนฉาก
        LoadData(); // โหลดเซฟทันทีที่เริ่มเกม
    }

    // --- 💰 ระบบจัดการเงินและอัปเกรด ---

    public void AddGold(int amount)
    {
        globalGold += amount;
        SaveData();
    }

    public bool TryUpgradeStarSkill()
    {
        int cost = GetUpgradeCost(starSkillLevel);
        if (globalGold >= cost)
        {
            globalGold -= cost;
            starSkillLevel++;
            SaveData();
            return true;
        }
        return false;
    }

    public bool TryUpgradeAttackSkill()
    {
        int cost = GetUpgradeCost(attackSkillLevel);
        if (globalGold >= cost)
        {
            globalGold -= cost;
            attackSkillLevel++;
            SaveData();
            return true;
        }
        return false;
    }

    public int GetUpgradeCost(int currentLevel)
    {
        // สูตรคำนวณราคา: ราคาเพิ่มขึ้นทีละ 50% หรือบวกเพิ่มตามใจชอบ
        return baseUpgradeCost + (currentLevel * 50);
    }

    // --- 💪 ระบบคำนวณโบนัส (เอาไปใช้ในเกม) ---

    public int GetStarBonusAmount()
    {
        // ตัวอย่าง: เวลละ 1 ดวง (หรือจะเป็็น % ก็ได้)
        return starSkillLevel * 1;
    }

    public int GetAttackBonusAmount()
    {
        // ตัวอย่าง: เวลละ 5 damage
        return attackSkillLevel * 5;
    }

    // --- 💾 ระบบ Save/Load (ใช้ PlayerPrefs ง่ายๆ) ---

    private void SaveData()
    {
        PlayerPrefs.SetInt("GlobalGold", globalGold);
        PlayerPrefs.SetInt("StarSkillLv", starSkillLevel);
        PlayerPrefs.SetInt("AtkSkillLv", attackSkillLevel);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        globalGold = PlayerPrefs.GetInt("GlobalGold", 0);
        starSkillLevel = PlayerPrefs.GetInt("StarSkillLv", 0);
        attackSkillLevel = PlayerPrefs.GetInt("AtkSkillLv", 0);
    }

    // คำสั่งล้างเซฟ (เผื่อใช้เทส)
    [ContextMenu("Reset Save")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
    }
}