using UnityEngine;
using UnityEngine.SceneManagement; // ต้องมีสำหรับจัดการ Scene

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance { get; private set; }

    [Header("Scene Configuration")]
    [Tooltip("ใส่ชื่อของ Scene ที่ใช้ต่อสู้กับบอส (ต้องตรงกับชื่อไฟล์ Scene)")]
    public string bossBattleSceneName = "BossBattleScene";

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void StartBossBattle(GameObject player)
    {
        PlayerData playerData = player.GetComponent<PlayerData>();
        if (playerData == null) return;

        Debug.Log($"--- PREPARING FOR BOSS BATTLE ---");

        // 1. เก็บข้อมูลปัจจุบันของผู้เล่นลงใน "กระเป๋าเดินทาง"
        GameTurnManager.CurrentPlayer.PlayerHealth = playerData.GetMaxHealth();
        GameTurnManager.CurrentPlayer.PlayerMoney = playerData.Money;
        Debug.Log($"Data saved: HP={playerData.GetMaxHealth()}, Money={playerData.Money}");

        // 2. สั่งให้โหลด Scene ต่อสู้
        Debug.Log($"Loading scene: {bossBattleSceneName}");
        SceneManager.LoadScene(bossBattleSceneName);
    }
}