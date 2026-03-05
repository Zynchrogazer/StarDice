using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // ใช้เพื่อเข้าถึงจากที่อื่น

    public int score = 0;  // เก็บคะแนน
    public TextMeshProUGUI scoreText;  // ตัวแปรอ้างอิง TextMeshProUGUI
    public GameObject gameOverPanel;  // เพิ่มตัวแปรสำหรับ Game Over Panel
    public TextMeshProUGUI gameOverScoreText; // ใช้ตอนเกมจบ


    private bool isGameOver = false;  // เช็คว่าเกมจบหรือยัง

    void Awake()
    {
        // เช็คว่า instance ตั้งค่าไว้หรือยัง ถ้ายังไม่ตั้งค่า จะตั้งค่าเป็นตัวนี้
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);  // ถ้ามี instance แล้ว จะทำลาย object นี้
    }

    void Update()
    {
        // ถ้าเกมจบแล้ว จะไม่ให้เพิ่มคะแนนหรือทำอะไรเพิ่มเติม
        if (isGameOver)
            return;

        // เช็คว่าถึง 6000 คะแนนแล้วหรือยัง
        if (score >= 6000)
        {
            GameOver();
        }
    }

    // ฟังก์ชันเพิ่มคะแนน
    public void AddScore(int amount)
    {
        if (isGameOver) return;  // ถ้าเกมจบแล้ว จะไม่สามารถเพิ่มคะแนนได้

        score += amount;  // เพิ่มคะแนน
        UpdateScoreText();  // อัปเดตคะแนนที่แสดงบน UI
    }

    // ฟังก์ชันอัปเดตข้อความคะแนนบน UI
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();  // แสดงคะแนนบน UI
        }

         if (gameOverScoreText != null)
         {
        gameOverScoreText.text = "Score: " + score.ToString();
         }
    }

    // ฟังก์ชัน Game Over
    public void GameOver()
    {
        if (isGameOver) return;  // ถ้าเกมจบแล้วไม่ทำอะไร

        isGameOver = true;  // ตั้งค่าเกมให้จบ
        gameOverPanel.SetActive(true);  // แสดง Game Over Panel
        UpdateScoreText();  // อัปเดตคะแนนใน Game Over
        MiniGameRewardService.TryGrantMoneyReward(score, "FlappyBird");
        Time.timeScale = 0f;  // หยุดเวลา (เกมหยุด)
        
    }
}
