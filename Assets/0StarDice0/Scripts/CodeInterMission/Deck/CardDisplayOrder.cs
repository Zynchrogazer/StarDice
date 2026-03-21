using System.Collections.Generic;
using UnityEngine;

// ⭐️ เพิ่มบรรทัดนี้: ทำให้สร้างไฟล์ "พิมพ์เขียว" นี้ได้จากเมนู Create
[CreateAssetMenu(fileName = "NewCardDisplayOrder", menuName = "Card Game/Card Display Order")]
public class CardDisplayOrder : ScriptableObject
{
    // ⭐️ นี่คือ List ที่คุณจะใช้ลากการ์ดใส่ใน Inspector
    public List<CardData> displayOrder;
}