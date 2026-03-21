using UnityEngine;

public class CardComparer
{
    public static int Compare(CardData a, CardData b)
    {
        // 1. เรียงตาม usable ก่อน (true มาก่อน false)
        if (a.isUsable != b.isUsable)
            return b.isUsable.CompareTo(a.isUsable);

        // 2. ถ้า usable เท่ากัน เรียงตาม rarity
        int rarityCompare = a.rarity.CompareTo(b.rarity);
        if (rarityCompare != 0)
            return rarityCompare;

        // 3. ถ้า rarity เท่ากัน เรียงตามชื่อ
        return string.Compare(a.cardName, b.cardName, System.StringComparison.OrdinalIgnoreCase);
    }
}
