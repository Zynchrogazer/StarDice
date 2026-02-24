using UnityEngine;

public enum CardRarity { Common, Rare, SR, SSR }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card")]
public class CardData : ScriptableObject
{
  public string cardName;
  [TextArea] public string description;
  public Sprite icon;

  public CardEffectType effectType;
  public int value; // เช่น จำนวนดาเมจ, จำนวน heal หรือ จำนวน turn ของบัฟ
  public int cooldownTurns = 0; // ถ้ามีคูลดาวน์
  public Sprite cardImage;
  public ElementType elementType; // ใช้ร่วมกับระบบธาตุ ถ้ามี

  public bool ignoreElement = false;
        
         public CardRarity rarity;   // ความแรร์
    public bool isUsable = true; // true = ใช้ได้, false = ใช้ไม่ได้
}
