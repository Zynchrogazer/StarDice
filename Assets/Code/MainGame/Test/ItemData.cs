using UnityEngine;

// This attribute allows you to create instances of this class from the Unity Editor's menu
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    //public ItemEffectData itemEffect;
    public enum ItemType { Weapon, Armor, Accessory }
    public enum ItemRarity { N, R, SR,SSR }
    public ItemType itemType;
    public ItemRarity itemRarity;
    public string itemName = "New Item";
    public string description = "Item Description";
    public Sprite icon = null;
    public int atk = 0; 
    public int def = 0;
    public int spd = 0;

    // You can add any other stats like damage, healing amount, etc.
}