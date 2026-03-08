using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickMathManager : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private float timer = 60f;
    private int score = 0;
    private int difficulty = 1;
    private int correctAnswer;
    private bool isGameActive = true;

    void Start()
    {
        gameOverPanel.SetActive(false);
        answerInput.onSubmit.AddListener(CheckAnswer);
        GenerateQuestion();
        UpdateUI();
    }

    void Update()
    {
        if (!isGameActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            EndGame();
        }

        UpdateUI();
    }

    private int questionCount = 0; // เพิ่มตัวแปรนี้ไว้ใน class

void GenerateQuestion()
{
    int a, b, op;

    // เฉพาะ 20 ข้อแรกให้มีแค่บวกกับลบ
    if (questionCount < 20)
    {
        op = Random.Range(0, 2); // 0 = +, 1 = -
    }
    else
    {
        op = Random.Range(0, 4); // 0 = +, 1 = -, 2 = *, 3 = ÷
    }

    switch (op)
    {
        case 0: // บวก
            a = Random.Range(1, 10 * difficulty);
            b = Random.Range(1, 10 * difficulty);
            correctAnswer = a + b;
            questionText.text = $"{a} + {b} = ?";
            break;

        case 1: // ลบ
            a = Random.Range(1, 10 * difficulty);
            b = Random.Range(1, a + 1);
            correctAnswer = a - b;
            questionText.text = $"{a} - {b} = ?";
            break;

        case 2: // คูณ
            a = Random.Range(2, 10 * difficulty);
            b = Random.Range(1, 10); // หลักเดียว
            correctAnswer = a * b;
            questionText.text = $"{a} × {b} = ?";
            break;

        case 3: // หารลงตัว
            b = Random.Range(2, 10); // ตัวหารหลักเดียว
            correctAnswer = Random.Range(2, 10 * difficulty);
            a = correctAnswer * b;
            questionText.text = $"{a} ÷ {b} = ?";
            break;
    }

    questionCount++; // นับจำนวนคำถามที่สร้าง
    answerInput.text = "";
    answerInput.ActivateInputField();
}

    void CheckAnswer(string input)
    {
        if (!isGameActive) return;

        if (int.TryParse(input, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                score += 100;
                timer += 5f;
                difficulty++;
            }
            else
            {
                score -= 50;
            }

            GenerateQuestion();
            UpdateUI();
        }
        else
        {
            // ไม่สามารถพิมพ์เลขได้เลย
            score -= 50;
            GenerateQuestion();
        }
    }

    void UpdateUI()
    {
        timerText.text = "Time: " + Mathf.CeilToInt(timer);
        scoreText.text = "Score: " + score;
    }

    void EndGame()
    {
        isGameActive = false;
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Your Score: " + score;
        MiniGameRewardService.TryGrantCreditReward(score, "QuickMath");
    }

    public void ReturnToBoardAfterGame()
    {
        MiniGameRewardService.ReturnToBoardScene();
    }
}
