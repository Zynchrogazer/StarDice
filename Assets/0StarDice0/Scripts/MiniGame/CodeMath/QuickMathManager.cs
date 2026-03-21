using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickMathManager : MonoBehaviour
{
    [Header("UI Elements")]
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
    private int questionCount = 0;

    [Header("Reward Settings")]
    public Sprite[] itemImages; 
    public Image showImage;

    // ประกาศตัวแปร ItemID ทั้งหมดที่คุณมี (สมมติว่าคุณมี Enum ItemID อยู่แล้ว)
    public ItemID KnightSword = ItemID.KnightSword;
    public ItemID KnightArmor = ItemID.KnightArmor;
    public ItemID KnightShoes = ItemID.KnightShoes;
    public ItemID GodArmor = ItemID.GodArmor;
    public ItemID FireSword = ItemID.FireSword;
    public ItemID HearthNeckless = ItemID.HearthNeckless;
    public ItemID RecoverRing = ItemID.RecoverRing;
    public ItemID WhiteFeather = ItemID.WhiteFeather;
    public ItemID DawnRign = ItemID.DawnRign;
    public ItemID Armor = ItemID.Armor;
    public ItemID Sword = ItemID.Sword;

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (showImage != null) showImage.gameObject.SetActive(false); // ซ่อนรูปไอเทมตอนเริ่มเกม

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

    void GenerateQuestion()
    {
        int a, b, op;

        if (questionCount < 10)
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
                b = Random.Range(1, 10); 
                correctAnswer = a * b;
                questionText.text = $"{a} × {b} = ?";
                break;

            case 3: // หารลงตัว
                b = Random.Range(2, 10); 
                correctAnswer = Random.Range(2, 10 * difficulty);
                a = correctAnswer * b;
                questionText.text = $"{a} ÷ {b} = ?";
                break;
        }

        questionCount++;
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
                timer += 2f;
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

        // เรียกใช้งานการสุ่มแจกไอเทมตอนจบเกม
        GiveRewardBasedOnScore(); 
    }

   int rewardAmount = 0;

    void GiveRewardBasedOnScore()
    {
        
        int roll = Random.Range(1, 101); // สุ่มเลข 1-100 ไว้ใช้ร่วมกันทุกเงื่อนไข

        if (score >= 2000) 
        {
            rewardAmount = 500;
if (GameTurnManager.CurrentPlayer != null)
{
    PlayerState p = GameTurnManager.CurrentPlayer.GetComponent<PlayerState>();
    if (p != null) p.PlayerCredit += rewardAmount;
}


if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
{
    GameData.Instance.selectedPlayer.AddCredit(rewardAmount);
}
            if (roll <= 20) // 1-20 (20%)
            {
                EquipmentManager.Instance.UnlockItem(KnightSword);
                showImage.sprite = itemImages[0]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 40) // 21-40 (20%)
            {
                EquipmentManager.Instance.UnlockItem(KnightArmor);
                showImage.sprite = itemImages[1]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 60) // 41-60 (20%)
            {
                EquipmentManager.Instance.UnlockItem(KnightShoes);
                showImage.sprite = itemImages[2]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll == 90) // โอกาส 1% 
            {
                EquipmentManager.Instance.UnlockItem(GodArmor);
                showImage.sprite = itemImages[3]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll == 100) // โอกาส 1% 
            {
                EquipmentManager.Instance.UnlockItem(FireSword);
                showImage.sprite = itemImages[4]; 
                showImage.gameObject.SetActive(true);
            }
            else 
            {
                // หากสุ่มได้เลข 61-89 หรือ 91-99 จะไม่ได้ไอเทมพิเศษอะไร (เกลือ)
                showImage.gameObject.SetActive(false); 
            }
        }
        else if (score >= 1000) 
        {
            rewardAmount = 300;
if (GameTurnManager.CurrentPlayer != null)
{
    PlayerState p = GameTurnManager.CurrentPlayer.GetComponent<PlayerState>();
    if (p != null) p.PlayerCredit += rewardAmount;
}


if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
{
    GameData.Instance.selectedPlayer.AddCredit(rewardAmount);
}
            if (roll <= 40) // 1-40 (40%)
            {
                EquipmentManager.Instance.UnlockItem(HearthNeckless);
                showImage.sprite = itemImages[5]; 
                showImage.gameObject.SetActive(true);
            }
            else if(roll <= 80 && roll> 41)// 41-100 (60%)
            {
                EquipmentManager.Instance.UnlockItem(RecoverRing);
                showImage.sprite = itemImages[6]; 
                showImage.gameObject.SetActive(true);
            }
              else 
            {
                // หากสุ่มได้เลข 61-89 หรือ 91-99 จะไม่ได้ไอเทมพิเศษอะไร (เกลือ)
                showImage.gameObject.SetActive(false); 
            }
        }
        else if (score >= 500) 
        {
            rewardAmount = 100;
if (GameTurnManager.CurrentPlayer != null)
{
    PlayerState p = GameTurnManager.CurrentPlayer.GetComponent<PlayerState>();
    if (p != null) p.PlayerCredit += rewardAmount;
}


if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
{
    GameData.Instance.selectedPlayer.AddCredit(rewardAmount);
}
            // แบ่งโอกาส 4 ชิ้น ชิ้นละ 25% ให้เท่าๆ กัน
            if (roll <= 20) 
            {
                EquipmentManager.Instance.UnlockItem(WhiteFeather);
                showImage.sprite = itemImages[7]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 40) 
            {
                EquipmentManager.Instance.UnlockItem(DawnRign);
                showImage.sprite = itemImages[8]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 60) 
            {
                EquipmentManager.Instance.UnlockItem(Armor);
                showImage.sprite = itemImages[9]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <=80)// 76-100
            {
                EquipmentManager.Instance.UnlockItem(Sword);
                showImage.sprite = itemImages[10]; 
                showImage.gameObject.SetActive(true);
            }
              else 
            {
                // หากสุ่มได้เลข 61-89 หรือ 91-99 จะไม่ได้ไอเทมพิเศษอะไร (เกลือ)
                showImage.gameObject.SetActive(false); 
            }
        }
        else 
        {
            // คะแนนไม่ถึง 2000 ไม่ได้อะไรเลย
            showImage.gameObject.SetActive(false);
        }
    }
}