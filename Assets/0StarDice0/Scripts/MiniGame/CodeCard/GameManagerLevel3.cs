using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerLevel3 : MonoBehaviour
{
    public static GameManagerLevel3 Instance;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform cardParent;
    public TMP_Text mistakeText;
    public Color[] cardColors; // <- ใส่สีที่ไม่ซ้ำกัน 8 สี (หรือมากกว่าถ้าต้องการ 15 คู่)

    private Card firstCard, secondCard;
    private int matchedPairs = 0;
    private int mistakes = 0;
    private int totalPairs = 15; // 15 คู่ = จับคู่ถูกหมดได้ 1500 คะแนน

    public bool IsBusy = false;

    [Header("UI & Timing")]
    public TMP_Text timerText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button nextButton;
    private float timeRemaining = 60f;
    private bool gameEnded = false;

    [Header("Scoring")]
    public int score = 0;
    public TMP_Text scoreText;

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ตัวแปรสำหรับจัดการไอเทมและรูปภาพ
    // ---------------------------------------------------------
    [Header("Reward Settings")]
    public Sprite[] itemImages; 
    public Image showImage;

    public ItemID KnightSword = ItemID.KnightSword;
    public ItemID KnightArmor = ItemID.KnightArmor;
    public ItemID KnightShoes = ItemID.KnightShoes;
    public ItemID WaterGodArmor = ItemID.WaterGodArmor;
    public ItemID EarthLegendaryArmor = ItemID.EarthLegendaryArmor;
    public ItemID HearthNeckless = ItemID.HearthNeckless;
    public ItemID RecoverRing = ItemID.RecoverRing;
    public ItemID WhiteFeather = ItemID.WhiteFeather;
    public ItemID DawnRign = ItemID.DawnRign;
    public ItemID Armor = ItemID.Armor;
    public ItemID Sword = ItemID.Sword;
    // ---------------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateCards();
        mistakeText.text = "Mistakes: 0";

        resultPanel.SetActive(false);
        nextButton.onClick.AddListener(GoToNextScene);

        // [เพิ่มใหม่] ซ่อนรูปไอเทมตอนเริ่มเกม
        if (showImage != null)
            showImage.gameObject.SetActive(false);
    }

    void CreateCards()
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        Shuffle(ids);

        foreach (int id in ids)
        {
            GameObject obj = Instantiate(cardPrefab, cardParent);
            Card card = obj.GetComponent<Card>();
            card.Setup(id);

            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() => card.OnClick());
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            int tmp = list[i];
            list[i] = list[rnd];
            list[rnd] = tmp;
        }
    }

    public void OnCardSelected(Card card)
    {
        if (firstCard == null)
        {
            firstCard = card;
        }
        else if (secondCard == null && card != firstCard)
        {
            secondCard = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        IsBusy = true;
        yield return new WaitForSeconds(1f);

        if (firstCard.cardId == secondCard.cardId)
        {
            AddScore(100); // ได้ 100 คะแนน
            ScoreManager.Instance.AddScore(100);
            
            firstCard.isMatched = true;
            secondCard.isMatched = true;
            matchedPairs++;
        }
        else
        {
            SubtractScore(10); // หัก 10 คะแนน
            ScoreManager.Instance.SubtractScore(10);

            mistakes++;
            mistakeText.text = "Mistakes: " + mistakes;
            firstCard.Hide();
            secondCard.Hide();
        }

        firstCard = null;
        secondCard = null;
        IsBusy = false;

        if (matchedPairs == totalPairs)
        {
            EndGame(true);
        }
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining).ToString();

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame(false);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI(); 
    }

    public void SubtractScore(int amount)
    {
        score -= amount;
        if (score < 0) score = 0; // ไม่ให้ติดลบ
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null && ScoreManager.Instance != null)
            scoreText.text = "Total Score: " + ScoreManager.Instance.totalScore;
    }

    void EndGame(bool won)
    {
        // ป้องกัน EndGame ทำงานซ้ำ
        if (gameEnded) return;

        gameEnded = true;
        resultPanel.SetActive(true);

        if (won)
            resultText.text = "You matched all cards!\nMistakes: " + mistakes + "\nScore: " + score;
        else
            resultText.text = "Time's up!\nMistakes: " + mistakes + "\nScore: " + score;

        // ---------------------------------------------------------
        // [เพิ่มใหม่] สุ่มแจกไอเทมตอนจบเกม
        GiveRewardBasedOnScore();
        // ---------------------------------------------------------
    }

    void GoToNextScene()
    {
        ScoreManager.Instance.ResetScore();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1); 
    }

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ฟังก์ชันสำหรับแจกไอเทมตามคะแนน (ปรับเกณฑ์ให้เข้ากับคะแนนเต็ม 1500)
    // ---------------------------------------------------------

   int rewardAmount = 0;


    void GiveRewardBasedOnScore()
    {
        int roll = Random.Range(1, 101); // สุ่มเลข 1-100

        if (score >= 1500) // เกณฑ์ระดับสูง
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
                EquipmentManager.Instance.UnlockItem(EarthLegendaryArmor);
                showImage.sprite = itemImages[3]; 
                showImage.gameObject.SetActive(true);
            }
            else if (roll == 100) 
            {
                EquipmentManager.Instance.UnlockItem(WaterGodArmor);
                showImage.sprite = itemImages[4]; 
                showImage.gameObject.SetActive(true);
            }
            else 
            {
                showImage.gameObject.SetActive(false); 
            }
        }
        else if (score >= 1000) // เกณฑ์ระดับกลาง
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
        else if( score >=500) // เกณฑ์ระดับเริ่มต้น (น้อยกว่า 500 หรือจบแบบ Game Over)
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