// สร้างไฟล์ใหม่ชื่อ BattleState.cs
using UnityEngine;
using System.Collections.Generic;

// ไม่ต้องสืบทอดจาก MonoBehaviour
public class BattleState
{
    #region Player Status
    public int PlayerHP { get; set; }
    public int PlayerMaxHP { get; set; }
    public int PlayerDef { get; set; }
    public int PlayerSpeed { get; set; }
    public int PlayerAttackDamage { get; set; }
    public ElementType PlayerElement { get; set; }
    #endregion

    #region Enemy Status
    public int EnemyHP { get; set; }
    public int EnemyMaxHP { get; set; }
    public int EnemyDef { get; set; }
    public int EnemySpeed { get; set; }
    public ElementType EnemyElement { get; set; }
    #endregion

    #region Turn & Game Status
    public bool IsPlayerTurn { get; set; }
    public bool HasUsedCardThisTurn { get; set; }
    #endregion

    #region Cooldowns
    // <SkillData, int> คือ <สกิล, จำนวนเทิร์นที่เหลือ>
    public Dictionary<SkillData, int> PlayerSkillCooldowns { get; set; }
    public int EnemySkill1Cooldown { get; set; }
    public int EnemySkill2Cooldown { get; set; }
    public int EnemySkill3Cooldown { get; set; }
    #endregion

    #region Player Buffs / States
    // (นี่เป็นแค่ตัวอย่างบางส่วน คุณต้องเพิ่มตัวแปรสถานะ *ทั้งหมด* ของผู้เล่นที่นี่)
    public bool IsDamageBoosted { get; set; }
    public int PlayerStunTurns { get; set; }
    public bool IsShieldActive { get; set; }
    public int ShieldTurnsLeft { get; set; }
    public int EvadeTurnsLeft { get; set; }
    public int LightBuffTurnsLeft { get; set; }
    public int LightShieldTurnsLeft { get; set; }
    public int DoubleAttackTurnsLeft { get; set; }
    public bool ReflectNextAttackWind { get; set; }
    public bool SuperSmashFire { get; set; }
    // ... (เพิ่มตัวแปรบัฟผู้เล่นอื่นๆ ทั้งหมด เช่น WaterBuffx2, EarthBootDef, ฯลฯ) ...
    #endregion

    #region Enemy Buffs / Debuffs
    // (นี่เป็นแค่ตัวอย่างบางส่วน คุณต้องเพิ่มตัวแปรสถานะ *ทั้งหมด* ของศัตรูที่นี่)
    public bool IsEnemyStunned { get; set; }
    public int EnemyStunTurnsLeft { get; set; }
    public int BurnTurnsLeft { get; set; }
    public int SuperburnTurnsLeft { get; set; }
    public int SilenceEnemyTurns { get; set; }
    public bool IsEnemyDamageReduced { get; set; }
    public int EnemyDamageReductionTurns { get; set; }
    public int EnemyDarkWalk { get; set; }
    // ... (เพิ่มตัวแปรดีบัฟศัตรูอื่นๆ ทั้งหมด) ...
    #endregion

    #region Card States
    public HashSet<CardData> UsedCards { get; set; }
    // (เพิ่มตัวแปรสถานะที่เกิดจากการ์ดทั้งหมดที่นี่)
    public bool IsIgnoreElementCardActive { get; set; }
    public bool IsEnemyAttackReducedPermanently { get; set; }
    public bool IsHalfDamageAll { get; set; }
    public bool HasPreventDeathEffect { get; set; }
    // ... (เพิ่มตัวแปรจากการ์ดอื่นๆ ทั้งหมด) ...
    #endregion


    // --- Constructor ---

    /// <summary>
    /// (1) Constructor เริ่มต้น: ใช้สร้างสถานะ "จริง" ของเกมในตอนเริ่ม
    /// </summary>
    public BattleState(PlayerData playerData, int enemyHP, int enemyDef, int enemySpeed, ElementType enemyElement)
    {
        // --- Player ---
        PlayerHP = playerData.maxHP;
        PlayerMaxHP = playerData.maxHP;
        PlayerDef = playerData.def;
        PlayerSpeed = playerData.speed;
        PlayerAttackDamage = playerData.attackDamage;
        PlayerElement = playerData.element; // (คุณมี elementType 2 ตัวใน PlayerData ผมเลือกตัวเล็ก)

        // --- Enemy ---
        EnemyHP = enemyHP;
        EnemyMaxHP = enemyHP; // สมมติว่า HP เริ่มต้นคือ MaxHP
        EnemyDef = enemyDef;
        EnemySpeed = enemySpeed;
        EnemyElement = enemyElement;

        // --- Game State ---
        IsPlayerTurn = (PlayerSpeed >= EnemySpeed);
        HasUsedCardThisTurn = !IsPlayerTurn; // ถ้าศัตรูเริ่มก่อน ก็ถือว่า "ใช้การ์ด" ไปแล้ว

        // --- Collections ---
        // (สำคัญ: ต้อง new ขึ้นมาใหม่)
        PlayerSkillCooldowns = new Dictionary<SkillData, int>();
        UsedCards = new HashSet<CardData>();

        // --- Buffs/Debuffs (ตั้งค่าเริ่มต้น) ---
        // (คุณต้องเพิ่มตัวแปรทั้งหมดของคุณที่นี่ โดยตั้งค่าเป็น false หรือ 0)
        IsDamageBoosted = false;
        PlayerStunTurns = 0;
        IsShieldActive = false;
        ShieldTurnsLeft = 0;
        EvadeTurnsLeft = 0;
        
        IsEnemyStunned = false;
        EnemyStunTurnsLeft = 0;
        BurnTurnsLeft = 0;
        SuperburnTurnsLeft = 0;
        
        EnemySkill1Cooldown = 0;
        EnemySkill2Cooldown = 0;
        EnemySkill3Cooldown = 0;
    }


    /// <summary>
    /// (2) Copy Constructor: นี่คือหัวใจของ Minimax!!!
    /// ใช้สำหรับ "คัดลอก" สถานะเพื่อนำไป "จำลอง" (Simulate)
    /// </summary>
    public BattleState(BattleState original)
    {
        // --- Player ---
        PlayerHP = original.PlayerHP;
        PlayerMaxHP = original.PlayerMaxHP;
        PlayerDef = original.PlayerDef;
        PlayerSpeed = original.PlayerSpeed;
        PlayerAttackDamage = original.PlayerAttackDamage;
        PlayerElement = original.PlayerElement;

        // --- Enemy ---
        EnemyHP = original.EnemyHP;
        EnemyMaxHP = original.EnemyMaxHP;
        EnemyDef = original.EnemyDef;
        EnemySpeed = original.EnemySpeed;
        EnemyElement = original.EnemyElement;

        // --- Game State ---
        IsPlayerTurn = original.IsPlayerTurn;
        HasUsedCardThisTurn = original.HasUsedCardThisTurn;

        // --- Collections (สำคัญมาก: ต้อง "Deep Copy") ---
        PlayerSkillCooldowns = new Dictionary<SkillData, int>(original.PlayerSkillCooldowns);
        UsedCards = new HashSet<CardData>(original.UsedCards);

        // --- Cooldowns ---
        EnemySkill1Cooldown = original.EnemySkill1Cooldown;
        EnemySkill2Cooldown = original.EnemySkill2Cooldown;
        EnemySkill3Cooldown = original.EnemySkill3Cooldown;

        // --- Buffs/Debuffs (คัดลอกค่าปัจจุบัน) ---
        // (คุณต้องคัดลอก *ทุก* ตัวแปรที่คุณเพิ่มไว้)
        IsDamageBoosted = original.IsDamageBoosted;
        PlayerStunTurns = original.PlayerStunTurns;
        IsShieldActive = original.IsShieldActive;
        ShieldTurnsLeft = original.ShieldTurnsLeft;
        EvadeTurnsLeft = original.EvadeTurnsLeft;
        
        IsEnemyStunned = original.IsEnemyStunned;
        EnemyStunTurnsLeft = original.EnemyStunTurnsLeft;
        BurnTurnsLeft = original.BurnTurnsLeft;
        SuperburnTurnsLeft = original.SuperburnTurnsLeft;
    }
}