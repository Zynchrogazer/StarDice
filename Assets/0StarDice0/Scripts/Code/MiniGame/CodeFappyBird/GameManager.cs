using UnityEngine;
using UnityEngine.UI; // <-- [เพิ่มใหม่] ต้องมีอันนี้เพื่อใช้ Image
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // ใช้เพื่อเข้าถึงจากที่อื่น

    public int score = 0;  // เก็บคะแนน
    public TextMeshProUGUI scoreText;  // ตัวแปรอ้างอิง TextMeshProUGUI
    public GameObject gameOverPanel;  // เพิ่มตัวแปรสำหรับ Game Over Panel
    public TextMeshProUGUI gameOverScoreText; // ใช้ตอนเกมจบ

    private bool isGameOver = false;  // เช็คว่าเกมจบหรือยัง

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ตัวแปรสำหรับจัดการไอเทมและรูปภาพ
    // ---------------------------------------------------------
    [Header("Reward Settings")]
    public Sprite[] itemImages; 
    public Image showImage;

    public ItemID KnightSword = ItemID.KnightSword;
    public ItemID KnightArmor = ItemID.KnightArmor;
    public ItemID KnightShoes = ItemID.KnightShoes;
    public ItemID DarkLegendaryDagger = ItemID.DarkLegendaryDagger;
    public ItemID WindLegendaryEye = ItemID.WindLegendaryEye;
    public ItemID HearthNeckless = ItemID.HearthNeckless;
    public ItemID RecoverRing = ItemID.RecoverRing;
    public ItemID WhiteFeather = ItemID.WhiteFeather;
    public ItemID DawnRign = ItemID.DawnRign;
    public ItemID Armor = ItemID.Armor;
    public ItemID Sword = ItemID.Sword;
    // ---------------------------------------------------------

    void Awake()
    {
        // เช็คว่า instance ตั้งค่าไว้หรือยัง ถ้ายังไม่ตั้งค่า จะตั้งค่าเป็นตัวนี้
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);  // ถ้ามี instance แล้ว จะทำลาย object นี้
    }

    void Start()
    {
        // [เพิ่มใหม่] ปิดรูปไอเทมตอนเริ่มเกม
        if (showImage != null)
            showImage.gameObject.SetActive(false);
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
        if (isGameOver) return;  

        score += amount;  
        UpdateScoreText();  
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();  
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + score.ToString();
        }
    }

    // ฟังก์ชัน Game Over
    public void GameOver()
    {
        if (isGameOver) return;  

        isGameOver = true;  
        gameOverPanel.SetActive(true);  
        UpdateScoreText();  
        GiveRewardBasedOnScore();
        Time.timeScale = 0f;  
    }

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ฟังก์ชันเช็คคะแนนและแจกของ
    // ---------------------------------------------------------

   int rewardAmount = 0;

    void GiveRewardBasedOnScore()
    {
        int roll = Random.Range(1, 101); // สุ่มเลข 1-100

        // ปรับเป็น 6000 เพื่อให้สอดคล้องกับเงื่อนไขจบเกมใน Update()
        if (score >= 6000) 
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
            if (roll <= 20) 
            {
                EquipmentManager.Instance.UnlockItem(KnightSword);
                showImage.sprite = itemImages[0]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 40) 
            {
                EquipmentManager.Instance.UnlockItem(KnightArmor);
                showImage.sprite = itemImages[1]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <= 60) 
            {
                EquipmentManager.Instance.UnlockItem(KnightShoes);
                showImage.sprite = itemImages[2]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll == 90) 
            {
                EquipmentManager.Instance.UnlockItem(WindLegendaryEye);
                showImage.sprite = itemImages[3]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll == 100) 
            {
                EquipmentManager.Instance.UnlockItem(DarkLegendaryDagger);
                showImage.sprite = itemImages[4]; 
                showImage.gameObject.SetActive(true);
            }
            else 
            {
                showImage.gameObject.SetActive(false); 
            }
        }
        else if (score >= 4000) 
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
            if (roll <= 40) 
            {
                EquipmentManager.Instance.UnlockItem(HearthNeckless);
                showImage.sprite = itemImages[5]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll <=80)
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
        else if (score >= 2000) 
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
            else if (roll <=80)
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
            showImage.gameObject.SetActive(false);
        }
    }
}