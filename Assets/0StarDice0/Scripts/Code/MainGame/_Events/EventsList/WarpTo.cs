//using UnityEngine;

//private void WarpTo(string destinationTileIDText)
//{
//    if (int.TryParse(destinationTileIDText, out int destinationID))
//    {
//        Transform targetNode = FindNodeByTileID(destinationID);
//        if (targetNode != null)
//        {
//            Debug.Log($"🌀 วาร์ปไปยัง tileID: {destinationID}");
//            player.MoveToInstant(targetNode); // หรือใช้ระบบ move ของคุณเอง
//        }
//        else
//        {
//            Debug.LogWarning($"❌ ไม่พบ tileID {destinationID} สำหรับวาร์ป");
//        }
//    }
//    else
//    {
//        Debug.LogWarning($"❌ eventName '{destinationTileIDText}' ไม่ใช่ตัวเลข tileID ที่ถูกต้อง");
//    }
//}
