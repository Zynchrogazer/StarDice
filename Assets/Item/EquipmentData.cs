using UnityEngine;
public enum ItemID
{
    None,

    // Normal
    Sword,  // สำหรับดาบ (+Damage 5)
    Armor,   // สำหรับเกราะ (+Def 10)
    DawnRign,  // เพิ่มพลังชีวิต 20 หน่วย
    WhiteFeather, //เพิ่มความเร็ว 20 หน่วย
    RecoverRing, // เพิ่มพลังชีวิต 2 หน่วยทุกเทิร์น
    HearthNeckless, // เพิ่มเลือดตอนเริ่มเกม 20%
    KnightSword, // ดาเมจ 5%
    KnightArmor, // Def 5%
    KnightShoes, // SPD 10%

    // Light

    // Normal
    LightArmor, //กันดาเมจธาตุมืด 10%
    LightNeckless, //เลือกเพิ่มขึ้น 50 หน้วย
    LightRing, //ดาเมจผู้เล่นแรงขึ้น 5%
    //SPB
    LightSpear, //ดาเมจใส่ธาตุมืดแรงขึ้น 10%
    //B
    GodArmor, //เพิ่มพลังป้องกัน 15%

    //Fire

    //N
    FireDagger, //ทำดาเมจเพิ่มขึ้น 7%
    FireAxe, // มีโอกาสติดไฟ 10%
    FireArmor, // กันดาเมจธาตุลม 10%
    //SPB
    FireLegendarySword, //ตีธาตุลมแรงขึ้น 10%
    //B
    FireSword, //มีโอกาสติดไฟ 20%

    //Water
    //N
    WaterArmor, // Def+10%
    RegenRing, //เพิ่มเลือด 5 หน่วยทุกเทิร์น
    WaterNeckless, // เลือดเพิ่ม 70 หน้วย
    //SPB
    WaterLegendaryArmor, // กันดาเมจธาตุไฟเพิ่มขึ้น 15%
    //B
    WaterGodArmor, //กันดาเมจ 15% และเพิ่มเลือด 5 หน่วยทุกเทิร์น

    //Wind
    //N
    WindShoes, // SPD เพิ้มขึ้น 20%
    WindEye, //หลบการโจมตี 10%
    WindSword, //โจมตีแรงขึ้น 7%
    //SPB
    WindSpear, //โจมตีแรงขึ้น 10%

    //B
    WindLegendaryEye, //หลบการโจมตี 30%

    //Earth
    EarthArmor, //Def +10%
    EarthHammer, //โจมตีแล้วมีโอกาสสตั๊น 10%
    EarthRing, //มีโอกาส 7% ที่ไม่รับดาเมจ

    //SPB
    EarthLegendaryArmor, //กันธาตุน้ำ 15%
    //B
    EarthLegendaryHammer, //ตีมีโอกาสติดสตั๊น 20%

    //Dark
    DarkDagger, //ตีธาตุแสงแรงขึ้น 10%
    DarkShoes, //หลบการโจมตี 15%
    DarkRing, //ตีและดูดเลือด 10% ของดาเมจ

    //SPB
    DarkLegendaryRing, //ตีและดูดเลือด 30% ของดาเมจ

    //B
    DarkLegendaryDagger //ดาเมจ 20%




    // Light


    //Fire

    //Water

    //Wind

    //Earth

    //Dark
}
// แก้ชื่อ Class ตรงนี้ให้ตรงกับชื่อไฟล์
[CreateAssetMenu(fileName = "NewEquipment", menuName = "Inventory/Equipment")]
public class EquipmentData : ScriptableObject 
{
    public string itemName;
    public Sprite icon;
    public ItemID itemID;
    
    [Header("Stats")]
    public int attackBonus;
    public int defenseBonus;
    public int speedBonus;

    [Header("Status")]
    public bool isOwned = false;
}