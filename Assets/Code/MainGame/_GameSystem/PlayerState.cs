using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]


public class PlayerState : MonoBehaviour
{
    //public static PlayerState Instance { get; private set; }
    [Header("AI Settings")]
    public bool isAI = false;
    [Header("Player Stats")]
    public int PlayerHealth;   // เปลี่ยนจาก Property เป็น Field เพื่อให้เห็นใน Inspector
    public int MaxHealth;      // ✅ เพิ่ม: เพื่อใช้คุมเพดานเลือด
    public int PlayerMoney = 0;
    public int PlayerStar = 0;
    public int CurrentAttack;
    public bool DebuffBurn = false;
    public ElementType PlayerElement { get; set; }

    [Header("Battle Stats")]
    public int WinCount = 0;
    

    [Header("Level System")]   // ✅ เพิ่ม: ระบบเลเวล
    public int PlayerLevel = 1;
    public int CurrentExp = 0;
    public int MaxExp = 100;

    [Header("Data & Inventory")]
    public PlayerData selectedPlayerPreset { get; private set; }
    public List<CardData> selectedCards { get; private set; } = new List<CardData>();

    public event System.Action OnDied;
    public event Action OnStatsUpdated;
    private void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
    }

    private void Start()
    {
        if (!isAI && GameData.Instance != null)
        {
            LoadFromPlayerData(GameData.Instance.selectedPlayer);
            SetSelectedCards(GameData.Instance.selectedCards);
        }
        else if (isAI)
        {
            // (Optional) ถ้าอยากให้บอทเริ่มมาเลือดเต็มตาม MaxHealth ที่ตั้งใน Inspector
            PlayerHealth = MaxHealth;
        }
    }

    public void LoadFromPlayerData(PlayerData data)
    {
        if (data == null) return;
        selectedPlayerPreset = data;

        // 1. โหลดค่าพลังชีวิต
        MaxHealth = data.maxHP;
        PlayerHealth = data.maxHP; // เริ่มเกมเลือดเต็ม
        CurrentAttack = data.attackDamage;
        // 2. โหลดเงิน (ถ้าต้องการใช้ค่าเริ่มต้นจาก Data)
        // PlayerMoney = data.Money; 

        // 3. ✅ โหลดข้อมูล Level จาก PlayerData
        PlayerLevel = data.level;
        CurrentExp = data.currentExp;
        MaxExp = data.maxExp;

        // กันเหนียว: ถ้า MaxExp เป็น 0 ให้ตั้งค่าเริ่มต้น
        if (MaxExp <= 0) MaxExp = 100;

        Debug.Log($"[PlayerState] Loaded: Level {PlayerLevel}, HP {PlayerHealth}/{MaxHealth}");
    }

    public void SetSelectedCards(List<CardData> cards)
    {
        selectedCards = new List<CardData>(cards);
    }

    // --- Combat Logic ---

    public void TakeDamage(int dmg)
    {
        PlayerHealth -= dmg;
        Debug.Log($"Took {dmg} damage. HP: {PlayerHealth}/{MaxHealth}");

        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            OnDied?.Invoke();
        }
    }

    public void Heal(int heal)
    {
        PlayerHealth += heal;

        // ✅ เพิ่ม Logic: ห้ามเกิน MaxHealth
        if (PlayerHealth > MaxHealth)
        {
            PlayerHealth = MaxHealth;
        }

        Debug.Log($"Healed {heal}. HP: {PlayerHealth}/{MaxHealth}");

        // (เช็ค <= 0 เผื่อไว้กรณี heal เป็นลบ แต่ปกติไม่ควรเกิด)
        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            OnDied?.Invoke();
        }
    }

    // --- Level & EXP Logic (New) ---

    public void GainExp(int amount)
    {
        CurrentExp += amount;
        if (CurrentExp >= MaxExp)
        {
            LevelUpRPG();
        }
        OnStatsUpdated?.Invoke();
    }

    private void LevelUpRPG()
    {
        CurrentExp -= MaxExp;
        PlayerLevel++;
        MaxExp = Mathf.CeilToInt(MaxExp * 1.2f); // เวลต่อไปยากขึ้น 20%

        // Bonus เมื่อเวลอัป (สไตล์ RPG)
        MaxHealth += 20;
        PlayerHealth = MaxHealth; // เลือดเด้งเต็ม
        CurrentAttack += 2;          // ตีแรงขึ้น

        Debug.Log($"💪 RPG LEVEL UP! Lv.{PlayerLevel} (HP: {MaxHealth}, ATK: {CurrentAttack})");

        // ถ้า EXP ยังเหลือเฟือ ก็ให้เช็คเวลอัปซ้ำ
        if (CurrentExp >= MaxExp) LevelUpRPG();
    }

    public void RecordBattleWin()
    {
        WinCount++;
        
        GainExp(50);
    }
    // --- Other Methods ---

    public void DropCard()
    {
        // ใส่ Logic ทิ้งการ์ดที่นี่
    }

    public void DropStar()
    {
        // ใส่ Logic ทิ้งดาวที่นี่
    }
}