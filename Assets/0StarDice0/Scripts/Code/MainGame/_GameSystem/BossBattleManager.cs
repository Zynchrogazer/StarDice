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
        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer == null)
        {
            Debug.LogWarning("[BossBattleManager] CurrentPlayer is null. Cannot start boss battle.");
            return;
        }

        Debug.Log($"--- PREPARING FOR BOSS BATTLE ---");

        // 1. เก็บข้อมูลปัจจุบันของผู้เล่นลงใน "กระเป๋าเดินทาง"
        int currentHp = Mathf.Clamp(currentPlayer.PlayerHealth, 0, Mathf.Max(1, currentPlayer.MaxHealth));
        currentPlayer.PlayerHealth = currentHp;

        if (GameData.Instance?.selectedPlayer != null)
        {
            currentPlayer.PlayerCredit = GameData.Instance.selectedPlayer.Credit;
        }

        Debug.Log($"Data saved: HP={currentHp}, Credit={currentPlayer.PlayerCredit}");

        // 2. สั่งให้โหลด Scene ต่อสู้
        Debug.Log($"Loading scene: {bossBattleSceneName}");
        SceneManager.LoadScene(bossBattleSceneName);
    }
}
