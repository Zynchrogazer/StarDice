using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BossSceneController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("ชื่อของ Scene บอร์ดเกมที่จะกลับไป")]
    public string mainBoardSceneName = "BoardGameScene"; // << ตั้งชื่อ Scene หลักของคุณ

    [Header("Player Battle Data (Simulated)")]
    // นี่คือค่าจำลองสำหรับใช้ใน Scene นี้
    private int currentPlayerHealth;
    private int currentPlayerMoney;

    [Header("UI References")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerMoneyText;
    public Button returnButton; // << ปุ่มสำหรับจำลองการจบการต่อสู้

    void Start()
    {
        if (GameTurnManager.CurrentPlayer != null)
        {
            // 1. แกะข้อมูลออกจากกระเป๋าตอนเริ่ม
            currentPlayerHealth = GameTurnManager.CurrentPlayer.PlayerHealth;
            currentPlayerMoney = GameTurnManager.CurrentPlayer.PlayerMoney;

            UpdateUI();
        }
        else
        {
            Debug.LogError("PlayerDataPersistence instance not found!");
        }

        // ตั้งค่าให้ปุ่ม "Return" ทำงาน
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(EndBattleAndReturn);
        }
    }

    // เมธอดจำลองการต่อสู้ (เช่น ผู้เล่นโดนดาเมจ)
    public void SimulatePlayerTakesDamage(int damage)
    {
        currentPlayerHealth -= damage;
        if (currentPlayerHealth < 0) currentPlayerHealth = 0;
        UpdateUI();
    }

    /// <summary>
    /// เมธอดหลักที่จะทำงานเมื่อการต่อสู้จบลง
    /// </summary>
    public void EndBattleAndReturn()
    {
        Debug.Log("--- BOSS BATTLE END ---");

        // ตรวจสอบว่ามี Persistence Instance อยู่หรือไม่
        if (GameTurnManager.CurrentPlayer != null)
        {
            // 1. บันทึกข้อมูลล่าสุด (HP, Money) กลับลงใน "กระเป๋าเดินทาง"
            GameTurnManager.CurrentPlayer.PlayerHealth = currentPlayerHealth;
            GameTurnManager.CurrentPlayer.PlayerMoney = currentPlayerMoney;
            Debug.Log($"Data saved for return: HP={currentPlayerHealth}, Money={currentPlayerMoney}");
        }

        // 2. สั่งให้โหลด Scene บอร์ดเกมกลับไป
        Debug.Log($"Returning to scene: {mainBoardSceneName}");
        SceneManager.LoadScene(mainBoardSceneName);
    }

    private void UpdateUI()
    {
        if (playerHealthText != null)
            playerHealthText.text = $"HP: {currentPlayerHealth}";

        if (playerMoneyText != null)
            playerMoneyText.text = $"Money: {currentPlayerMoney}";
    }
}