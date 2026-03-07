using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int power;
    public int cooldown; // ← เพิ่มคูลดาวน์
      public int cooldownTurns;
          public ElementType elementType; 
  public Sprite icon;

    [HideInInspector]
    public bool isLocked = false; // true = ล็อค, false = ปลดล็อค
    public float attackMultiplier = 1f;
    public float dodgeChance = 0f;
    public float damageReductionPlayer = 0f;
// ใน SkillData

    public enum SkillEffectType
    {

        /// //////////Fire///////
        DamageFire,
        Burn,
        BuffFire,
        SuperFire,
        PaintoHeal,
        RandomDamgeFire,
        SuperBurn,
        SuperSmashFire,
        SuperSuperFire,
        ReduceWater100,

        ///////////////Water////////////
        DamageWater,
        HealWater,
        WaterReducted,
        SuperWater,
        SuperHealWater,
        WaterMaxHP,
        SuperWaterReduce100,
        WaterRegen,
        WaterBuffx2,
        NerfEarth,

        /// /Wind//////////
        DamageWind,
        DoubleWindDamage,
        ReflectWind,
        SuperWind,
        SuperSuperWind,
        SuperSuperSuperWind,
        DoubleReflectWind,
        WindBootSPD,
        WindWalk,
        ReduceFire100,

        /// Earth////
        DamageEarth,
        ShieldEarth,
        StunEarth,
        SuperEath,
        EarthNerfDamage,
        EarthBootDef,
        EarthNerfSPD,
        SuperSmashEarth,
        EarthSmashEarth,
        ReduceWind100,


        /// Dark ///
        DamageDark,
        DamageRandomDark,
        EyeDark,
        SuperDark,
        SuperRandomDark,
        LuckyBadLuckDark,
        SuperEyeDark,
        BootDarkSpeed,
        BootDamageDark,
        ReduceLight50,

        /// Light///
        DamageLight,
        Buffx2Light,
        HealandShield,
        SuperLight,
        HealLight,
        SuperHealLight,
        bootDamageLight,
        SuperShieldLight,
        NerfEnemyDamge,
        NerfEnemyDark,

        Custom
    }

    public SkillEffectType effectType;

     // 🧠 ใช้ใน Minimax สำหรับจำลองผลลัพธ์
    public void SimulateSkill(ref int userHP, ref int targetHP,ref float attackMultiplier)
    {
        switch (effectType)
        {

            // 🔥 Fire skills
            case SkillEffectType.DamageFire:
            case SkillEffectType.SuperFire:
               case SkillEffectType.SuperSuperFire:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.BuffFire:
                userHP -= 10;
                attackMultiplier *= 2;
                break;
            case SkillEffectType.Burn:
                targetHP -= (int)(power * 3 * attackMultiplier);
                attackMultiplier = 1;
                break;
             case SkillEffectType.SuperBurn:
                targetHP -= (int)(power * 3 * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.PaintoHeal:
                userHP += 20;
                break;
            case SkillEffectType.RandomDamgeFire:
             int randomfire = Random.Range(20, 50);
                targetHP -= (int)(randomfire * attackMultiplier);
                 attackMultiplier = 1;
                break;
            case SkillEffectType.SuperSmashFire:
                attackMultiplier *= 5;
                break;
            case SkillEffectType.ReduceWater100:
                userHP += 40;
                break;
       
            // 💧 Water
            case SkillEffectType.DamageWater:
            case SkillEffectType.SuperWater:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.HealWater:
             userHP += 20;
                if (userHP > 170) userHP = 170;
                break;
            case SkillEffectType.WaterReducted:
            case SkillEffectType.NerfEarth:
                userHP += 30;
                break;
            case SkillEffectType.SuperHealWater:
                userHP += 40;
                if (userHP > 170) userHP = 170;
                break;
             case SkillEffectType.WaterRegen:
                userHP += 50;
                  if (userHP > 170) userHP = 170;
                break;
            case SkillEffectType.SuperWaterReduce100:
                userHP += 60;
                break;
            case SkillEffectType.WaterBuffx2:
                attackMultiplier *= 2;
                break;

            // 🌪 Wind
            case SkillEffectType.DamageWind:
            case SkillEffectType.DoubleWindDamage:
            case SkillEffectType.SuperWind:
            case SkillEffectType.SuperSuperWind:
            case SkillEffectType.SuperSuperSuperWind:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.ReflectWind:
                targetHP -= 20;
                userHP += 20;
                break;
            case SkillEffectType.DoubleReflectWind:
             targetHP -= 20;
                userHP += 20;      
                break;
            case SkillEffectType.ReduceFire100:
                userHP += 40;
                break;

            // 🪨 Earth
            case SkillEffectType.DamageEarth:
            case SkillEffectType.SuperEath:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.ShieldEarth:
                userHP += 60;
                if (userHP > 150) userHP = 150;
                break;
            case SkillEffectType.StunEarth:
                targetHP -= 60;
                break;
            case SkillEffectType.EarthNerfDamage:
            case SkillEffectType.EarthBootDef:
                userHP += 30;
                break;
            case SkillEffectType.SuperSmashEarth:
                attackMultiplier *= 2;
                break;
            case SkillEffectType.EarthSmashEarth:
                attackMultiplier *= 5;
                break;
            case SkillEffectType.ReduceWind100:
                userHP += 60;
                if (userHP > 150) userHP = 150;
                break;

            // 🌑 Dark
            case SkillEffectType.DamageDark:
            case SkillEffectType.SuperDark:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.DamageRandomDark:
             int randomdark = Random.Range(20, 60);
                targetHP -= (int)(randomdark * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.SuperRandomDark:
             int randomdarkdark = Random.Range(30, 80);
                targetHP -= (int)(randomdarkdark * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.LuckyBadLuckDark:
             int randombadluck = Random.Range(10, 100);
                targetHP -= (int)(randombadluck * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.EyeDark:
                int randomFactor = Random.Range(20, 60);
                userHP += randomFactor;
                break;
            case SkillEffectType.SuperEyeDark:
                int randomsupereye = Random.Range(40, 60);
                userHP += randomsupereye;
                break;
            case SkillEffectType.BootDamageDark:
                attackMultiplier *= 2;
                break;
            case SkillEffectType.ReduceLight50:
                userHP += 30;
                break;


            // 🌕 Light
            case SkillEffectType.DamageLight:
            case SkillEffectType.SuperLight:
                targetHP -= (int)(power * attackMultiplier);
                attackMultiplier = 1;
                break;
            case SkillEffectType.Buffx2Light:
                attackMultiplier *= 2;
                break;
            case SkillEffectType.HealandShield:
                userHP += 90;
                if (userHP > 150) userHP = 150;
                break;
            case SkillEffectType.HealLight:
                userHP += 85;
                if (userHP > 150) userHP = 150;
                break;
             case SkillEffectType.SuperHealLight:
                userHP += 150;
                if (userHP > 150) userHP = 150;
                break;
            case SkillEffectType.bootDamageLight:
                attackMultiplier *= 3;
                break;
            case SkillEffectType.SuperShieldLight:
                userHP += 120;
                if (userHP > 150) userHP = 150;
                break;
            case SkillEffectType.NerfEnemyDamge:
                userHP += 60;
                if (userHP > 150) userHP = 150;
                break;

                default:
                    // กรณีไม่รู้จักสกิล ให้ทำอะไรเบา ๆ เช่นโจมตีปกติ
                    targetHP -= 10;
                    break;
                }
    }
}
