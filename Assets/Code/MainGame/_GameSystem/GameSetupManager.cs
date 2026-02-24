using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("ใส่ไว้กันเหนียว เผื่อกด Play ฉากนี้โดยไม่มี GameData (เช่นตอนเทส)")]
    public PlayerData fallbackPlayerData;

    // ตัวแปรภายในสำหรับเก็บ Data ที่จะใช้จริง (ไม่ต้องโชว์ใน Inspector)
    private PlayerData currentData;

    private void OnEnable()
    {
        // 1. พยายามดึงจาก GameData ก่อน (Priority สูงสุด)
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            currentData = GameData.Instance.selectedPlayer;
            Debug.Log($"[GameSetupManager] ✅ Found GameData! Syncing with selected player: {currentData.playerName}");
        }
        // 2. ถ้าไม่มี GameData (เช่น กด Play จากซีนนี้เลย) ให้ใช้ตัว Fallback ใน Inspector
        else if (fallbackPlayerData != null)
        {
            currentData = fallbackPlayerData;
            Debug.LogWarning("[GameSetupManager] ⚠️ GameData not found. Using Fallback Data from Inspector.");
        }
        else
        {
            Debug.LogError("[GameSetupManager] ❌ Critical: No PlayerData found anywhere!");
            return;
        }

        // 3. เริ่มกระบวนการ Sync ข้อมูล
        if (PlayerState.Instance != null)
        {
            // ⛔ ย้ำอีกครั้ง: บรรทัดนี้ปิดไว้เพื่อป้องกัน "เลือดเด้งเต็ม" ตอนกลับมาจากมินิเกม
            // PlayerState.Instance.LoadFromPlayerData(currentData); 

            Debug.Log("[GameSetupManager] Saving current game state to Data...");
            UpdatePlayerData();
        }
    }

    private void UpdatePlayerData()
    {
        if (currentData == null) return;

        // ดึงค่าปัจจุบันจาก PlayerState
        int currentHealth = PlayerState.Instance.PlayerHealth;

        // ถ้าคุณแยก Inventory ไปแล้ว อาจต้องดึงเงินจาก PlayerInventory หรือ GameData แทน
        // แต่ถ้ายังอยู่ที่เดิม ก็ใช้บรรทัดนี้:
        int currentMoney = PlayerState.Instance.PlayerMoney;

        Debug.Log($"<color=lightblue>[GameSetupManager]</color> Updating {currentData.playerName}: HP={currentHealth}");

        // บันทึกลง ScriptableObject ที่เราดึงมา (GameData.selectedPlayer)
        currentData.SetHealth(currentHealth);
        currentData.SetMoney(currentMoney);
    }
}