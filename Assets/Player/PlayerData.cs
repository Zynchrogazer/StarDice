using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Battle/PlayerData")]

public class PlayerData : ScriptableObject
{
    public string playerName;
    public ElementType element;
    public Sprite playerSprite;

    public int maxHP = 100;
    public int attackDamage = 10;
    public int speed = 10;
    public int def = 1;
    public SkillData[] skills = new SkillData[3]; // 3 สกิลพิเศษ
    public SkillData[] allSkills = new SkillData[10]; //สกิลทั้งหมด
    public ElementType elementType;


    [Header("Player Stats")]
    public int maxHealth = 100;
    [SerializeField]
    private int currentHealth;

    [Header("Level System")]
    public int level = 1;      // เลเวลเริ่มต้น
    public int currentExp = 0; // EXP เริ่มต้น
    public int maxExp = 100;   // EXP ที่ต้องใช้ในการอัปเวลครั้งแรก
    // ------------------------------------

    private int money = 50;
    private int star = 55; // ถ้าคุณใช้ star ด้วย ก็ควรเพิ่ม Property ให้มันเหมือน Money

    public event Action<int> OnMoneyChanged;
    public event Action OnDied;

    [Header("Status Effects")]
    public int turnsToSkip = 0;

    public int Money
    {
        get => money;
        set
        {
            if (money == value) return;
            money = value;
            OnMoneyChanged?.Invoke(money);
        }
    }

    private void Die()
    {
        Debug.LogError($"[PlayerData] {playerName} has died!");
        // TODO: Game over logic, show UI, trigger event ฯลฯ
    }

    public int GetMaxHealth()
    {
        return maxHP;
    }

    /// <summary>
    /// เมธอดสำหรับตั้งค่า HP โดยตรง (ใช้ตอนโหลดข้อมูลกลับ)
    /// </summary>
    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        Debug.Log($"[PlayerData] {playerName} health set to {currentHealth}/{maxHealth}");
        // สามารถเพิ่ม Event แจ้ง UI ให้รีเฟรชได้
    }

    /// <summary>
    /// เมธอดสำหรับตั้งค่าเงินโดยตรง และเรียก Event
    /// </summary>
    public void SetMoney(int newAmount)
    {
        this.Money = newAmount; // ใช้ Property เพื่อให้ Event OnMoneyChanged ทำงาน
    }



}

