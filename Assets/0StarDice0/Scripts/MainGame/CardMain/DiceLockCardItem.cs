using UnityEngine;
using System.Collections.Generic;
// ประกาศประเภทการ์ดไว้ข้างบนนี้ (จะได้ไม่ต้องสร้างไฟล์แยก)
public enum DiceCardType
{
    LockNumber, // ล็อคเลข
    Multiplier, // คูณ (x2, x3, x4)
    Warp,        // วาร์ป
    CleanseBurn, // ล้างสถานะเผาไหม้
    Heal,
    CleanseIce,
    FreezeAllEnemies,
    DoubleStar,
    WarpToEnemy
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Dice Lock Card")]
public class DiceLockCardItem : ScriptableObject
{
    [Header("Card Info")]
    public string cardName; 
    [TextArea] public string description; 
    public Sprite cardImage; 

    [Header("Settings")]
    [Header("Shop Settings")]
    public int price = 100; // ตั้งราคาเริ่มต้นไว้สักหน่อย
     
     // เลือกประเภทการ์ดตรงนี้
    public DiceCardType cardType;

    [Tooltip("ถ้าเป็น Lock ให้ใส่เลข 1-6 \nถ้าเป็น Multiplier ให้ใส่ตัวคูณ (เช่น 2, 3)")]
    [Range(1, 10)] 
    public int lockNumber = 1; // ใช้ตัวแปรเดิม จะได้ไม่ต้องแก้เยอะ

    [Tooltip("จำนวนเลือดที่ต้องการฟื้นฟู (เฉพาะการ์ดประเภท Heal)")]
    public int healAmount = 30;

    public void Use()
    {
        Debug.Log($"<color=yellow>[CardItem] กดใช้การ์ด: {cardName} (Type: {cardType})</color>");

        if (!DiceRollerFromPNG.TryGet(out var diceRoller))
        {
            Debug.LogError("[CardItem] ❌ ไม่เจอ DiceRollerFromPNG ในฉาก!");
            return;
        }

        // ตรวจสอบประเภทการ์ด แล้วสั่งงานให้ถูกต้อง
        switch (cardType)
        {
            case DiceCardType.LockNumber:
                // แบบเดิม: ล็อคเลข
                Debug.Log($"[CardItem] สั่งล็อคเลขเป็น: {lockNumber}");
                diceRoller.RollDiceWithResult(lockNumber);
                break;

            case DiceCardType.Multiplier:
                // แบบใหม่: ไม่ทอยทันที แต่ส่งค่าไปรอไว้
                Debug.Log($"[CardItem] เปิดใช้งานสถานะคูณ: x{lockNumber}");
                
                // เรียกฟังก์ชันใหม่ที่เราเพิ่งสร้าง
                diceRoller.SetPendingMultiplier(lockNumber);
                break;

            case DiceCardType.Warp:
                Debug.Log("ใช้การ์ด Warp: กรุณาเลือกช่องบนหน้าจอ...");
                
                // เรียกใช้ฟังก์ชันที่เราเพิ่งเขียนใน RouteManager
                if (RouteManager.TryGet(out var routeManager))
                {
                    routeManager.StartWarpSelection();
                }
                break;

            case DiceCardType.CleanseBurn:
                Debug.Log("[CardItem] ใช้การ์ด Cleanse Burn กำลังพยายามล้างสถานะ...");
                
                // ค้นหา PlayerState ในฉากเพื่อสั่งล้างสถานะ
                // หมายเหตุ: ถ้าเกมคุณมีหลายตัวละคร (Multiplayer/AI) 
                // คุณอาจจะต้องเปลี่ยน FindFirstObjectByType เป็นการดึงผ่าน TurnManager หรือ GameData.Instance แทนครับ
                PlayerState currentPlayer = FindFirstObjectByType<PlayerState>();
                
                if (currentPlayer != null)
                {
                    currentPlayer.CleanseBurn();
                }
                else
                {
                    Debug.LogError("[CardItem] ❌ ไม่พบ PlayerState ในฉาก!");
                }
                break;

                case DiceCardType.Heal:
                Debug.Log($"[CardItem] 💖 ใช้การ์ด Heal ฟื้นฟูเลือด {healAmount} หน่วย...");
                
                PlayerState playerToHeal = FindFirstObjectByType<PlayerState>();

                if (playerToHeal != null)
                {
                    // เพิ่มเลือด
                    playerToHeal.PlayerHealth += healAmount;
                    
                    // 🛡️ ป้องกันเลือดทะลุหลอด Max HP
                    if (playerToHeal.PlayerHealth > playerToHeal.MaxHealth)
                    {
                        playerToHeal.PlayerHealth = playerToHeal.MaxHealth;
                    }

                    Debug.Log($"[CardItem] 💖 ฮีลสำเร็จ! เลือดปัจจุบัน: {playerToHeal.PlayerHealth} / {playerToHeal.MaxHealth}");
                }
                else
                {
                    Debug.LogError("[CardItem] ❌ ไม่พบ PlayerState ในฉาก!");
                }
                break;

                case DiceCardType.CleanseIce:
                Debug.Log("[CardItem] ❄️ ใช้การ์ด Cleanse Ice กำลังพยายามล้างสถานะแช่แข็ง...");
                
                // ค้นหาผู้เล่นเพื่อสั่งล้างสถานะ
                PlayerState playerToCleanseIce = FindFirstObjectByType<PlayerState>();
                
                if (playerToCleanseIce != null)
                {
                    // เรียกใช้ฟังก์ชันที่เราเพิ่งสร้าง
                    playerToCleanseIce.CleanseIce();
                }
                else
                {
                    Debug.LogError("[CardItem] ❌ ไม่พบ PlayerState ในฉาก!");
                }
                break;

                case DiceCardType.FreezeAllEnemies:
                Debug.Log("[CardItem] ❄️ ใช้การ์ดแช่แข็ง! ศัตรูทั้งหมดจะหยุดเดิน 3 เทิร์น");

                // ค้นหาตัวละครทั้งหมดในฉาก
                PlayerState[] allPlayers = FindObjectsOfType<PlayerState>();
                
                foreach (PlayerState p in allPlayers)
                {
                    // ถ้าเป็น AI (ศัตรู) ให้ใส่สถานะแช่แข็ง
                    if (p.isAI)
                    {
                        p.StunTurnsRemaining = 3; // กำหนดให้หยุด 3 ตา
                        p.hasIceEffect = true;    // เปิดโชว์ไอคอนน้ำแข็ง
                        p.NotifyStatsUpdated();   // อัปเดต UI
                        Debug.Log($"❄️ {p.name} ถูกแช่แข็ง!");
                    }
                }
                break;

                case DiceCardType.DoubleStar:
                Debug.Log("[CardItem] 🌟 ใช้การ์ด Double Star! ตกช่องดาวครั้งต่อไปจะได้รับดาว x2");
                
                // ค้นหาผู้เล่นเพื่อมอบบัฟ
                PlayerState playerToBuff = FindFirstObjectByType<PlayerState>();
                
                if (playerToBuff != null)
                {
                    playerToBuff.hasDoubleStarBuff = true; // เปิดการใช้งานบัฟ
                    // playerToBuff.NotifyStatsUpdated(); // อัปเดต UI (ถ้ามีไอคอนโชว์บัฟ)
                }
                break;

                case DiceCardType.WarpToEnemy:
                Debug.Log("[CardItem] 🗡️ ใช้การ์ด Warp To Enemy! กำลังค้นหาเป้าหมาย...");

                PlayerPathWalker playerWalker = null;
                List<PlayerPathWalker> enemies = new List<PlayerPathWalker>();

                // ค้นหาคนเดินทั้งหมดบนกระดาน
                PlayerPathWalker[] allWalkers = FindObjectsOfType<PlayerPathWalker>();

                foreach (var walker in allWalkers)
                {
                    PlayerState state = walker.GetComponent<PlayerState>();
                    if (state != null)
                    {
                        if (!state.isAI) playerWalker = walker; // คนเล่น
                        else enemies.Add(walker);               // ศัตรู (บอท)
                    }
                }

                // สุ่มเลือกศัตรูเป้าหมาย แล้วสั่งวาร์ป
                if (playerWalker != null && enemies.Count > 0)
                {
                    PlayerPathWalker targetEnemy = enemies[Random.Range(0, enemies.Count)];
                    Transform targetNode = targetEnemy.CurrentNodeTransform;

                    if (targetNode != null)
                    {
                        Debug.Log($"🎯 วาร์ปผู้เล่นไปหา {targetEnemy.name} ที่ช่อง {targetNode.name}");
                        playerWalker.WarpByCard(targetNode);
                    }
                }
                else
                {
                    Debug.LogWarning("❌ หาเป้าหมายไม่เจอ หรือไม่มีศัตรูในฉาก!");
                }
                break;
        

        }
    }
}