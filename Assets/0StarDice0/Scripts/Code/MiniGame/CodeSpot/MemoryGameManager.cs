using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryGameManager : MonoBehaviour
{
    public List<Button> buttons; // ใส่ 30 ปุ่ม (6x5)
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI scoreText; 

    public Color highlightColor = Color.yellow;
    public Color correctColor = Color.cyan;
    public Color wrongColor = Color.red;

    public float showTime = 2f;
    public float gameDuration = 60f;

    private float timer;
    private int currentPhase = 1;
    private List<int> currentPattern = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool isAnswerPhase = false;
    private bool isGameOver = false;
    private bool phasePassed = false;
    private bool waitingForPhase = false;

    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button nextSceneButton;

    private int score = 0;

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ตัวแปรสำหรับจัดการไอเทมและรูปภาพ
    // ---------------------------------------------------------
    [Header("Reward Settings")]
    public Sprite[] itemImages; 
    public Image showImage;

    public ItemID KnightSword = ItemID.KnightSword; 
    public ItemID KnightArmor = ItemID.KnightArmor; 
    public ItemID KnightShoes = ItemID.KnightShoes; 
    public ItemID WindLegendaryEye = ItemID.WindLegendaryEye; 
    public ItemID WaterGodArmor = ItemID.WaterGodArmor; 

    public ItemID HearthNeckless = ItemID.HearthNeckless; 
      public ItemID RecoverRing = ItemID.RecoverRing; 

       public ItemID WhiteFeather = ItemID.WhiteFeather; 
        public ItemID DawnRign = ItemID.DawnRign; 
 public ItemID Armor = ItemID.Armor; 
 public ItemID Sword = ItemID.Sword; 


    // ---------------------------------------------------------

    void Start()
    {
        timer = gameDuration;
        UpdateScoreUI();
        StartCoroutine(GameLoop());
        gameOverPanel.SetActive(false);
        
        // [เพิ่มใหม่] ปิดรูปภาพไอเทมตอนเริ่มเกมไว้ก่อน
        if(showImage != null)
            showImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.CeilToInt(timer).ToString();

            if (timer <= 0f)
            {
                isGameOver = true;
                EndGame();
            }
        }
    }

    IEnumerator GameLoop()
    {
        while (currentPhase <= 10 && !isGameOver)
        {
            phasePassed = false;
            waitingForPhase = true;

            yield return StartCoroutine(StartPhase(currentPhase, false));

            while (!phasePassed && !isGameOver)
            {
                yield return null;
            }

            if (phasePassed)
            {
                currentPhase++;
            }
        }

        EndGame();
    }

    IEnumerator StartPhase(int phase, bool isRetry = false)
    {
        if (!isRetry)
            phaseText.text = "Phase: " + phase;

        ResetButtons();
        currentPattern.Clear();
        playerInput.Clear();
        isAnswerPhase = false;

        int numTargets = Mathf.Min(10 + (phase - 1), buttons.Count);
        HashSet<int> usedIndexes = new HashSet<int>();
        int safety = 1000;
        while (currentPattern.Count < numTargets && safety-- > 0)
        {
            int rand = Random.Range(0, buttons.Count);
            if (!usedIndexes.Contains(rand))
            {
                usedIndexes.Add(rand);
                currentPattern.Add(rand);
            }
        }

        foreach (int i in currentPattern)
        {
            if (buttons[i] != null)
                buttons[i].image.color = highlightColor;
        }

        yield return new WaitForSeconds(showTime);

        ResetButtons();
        isAnswerPhase = true;

        while (isAnswerPhase && !isGameOver)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    public void OnButtonPressed(int index)
    {
        if (!isAnswerPhase || playerInput.Contains(index))
            return;

        playerInput.Add(index);

        if (currentPattern.Contains(index))
        {
            if (buttons[index] != null)
                buttons[index].image.color = correctColor;

            score += 80; 
            UpdateScoreUI();

            if (playerInput.Count == currentPattern.Count)
            {
                score += 1000;
                UpdateScoreUI();
                isAnswerPhase = false;
                phasePassed = true; 
            }
        }
        else
        {
            if (buttons[index] != null)
                buttons[index].image.color = wrongColor;

            score -= 300;
            UpdateScoreUI();
            isAnswerPhase = false;

            StartCoroutine(RetryPhase());
        }
    }

    IEnumerator RetryPhase()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(StartPhase(currentPhase, true));
    }

    void ResetButtons()
    {
        foreach (Button b in buttons)
        {
            if (b != null)
                b.image.color = Color.white;
        }
    }

    void EndGame()
    {
        // ป้องกันไม่ให้ EndGame ทำงานซ้ำซ้อน
        if(gameOverPanel.activeSelf) return; 

        isGameOver = true;
        Debug.Log("🎯 Game Over!");
        phaseText.text = "Game Over";

        gameOverPanel.SetActive(true);
        finalScoreText.text = "Your Score: " + score.ToString();

        // ---------------------------------------------------------
        // [เพิ่มใหม่] เรียกฟังก์ชันคำนวณและแจกไอเทมตอนจบเกม
        GiveRewardBasedOnScore();
        // ---------------------------------------------------------

        nextSceneButton.onClick.RemoveAllListeners();
        nextSceneButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        });
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    // ---------------------------------------------------------
    // [เพิ่มใหม่] ฟังก์ชันสำหรับเช็คคะแนนและแจกไอเทมพร้อมโชว์รูป
    // ---------------------------------------------------------
    void GiveRewardBasedOnScore()
    {
        // ปรับตัวเลขคะแนนตรงนี้ได้ตามต้องการเลยครับ
        if (score >= 8000) 
        {
            int roll = Random.Range(1, 101);
            
            if (roll < 21)
            {
                 EquipmentManager.Instance.UnlockItem(KnightSword);
                  showImage.sprite = itemImages[0]; 
                 showImage.gameObject.SetActive(true);
            }
            else if (roll >21 && roll < 41)
            {
                 EquipmentManager.Instance.UnlockItem(KnightArmor);
                  showImage.sprite = itemImages[1]; 
                 showImage.gameObject.SetActive(true);
            }
            else if (roll > 41 && roll < 61)
            {
                 EquipmentManager.Instance.UnlockItem(KnightShoes);
                  showImage.sprite = itemImages[2]; 
                 showImage.gameObject.SetActive(true);
            }
             else if (roll == 100)
            {
                 EquipmentManager.Instance.UnlockItem(WaterGodArmor);
                  showImage.sprite = itemImages[3]; 
                 showImage.gameObject.SetActive(true);
            }
             else if (roll == 90)
            {
                 EquipmentManager.Instance.UnlockItem(WindLegendaryEye);
                  showImage.sprite = itemImages[4]; 
                 showImage.gameObject.SetActive(true);
            }
              else 
            {
                // หากสุ่มได้เลข 61-89 หรือ 91-99 จะไม่ได้ไอเทมพิเศษอะไร (เกลือ)
                showImage.gameObject.SetActive(false); 
            }
            

          
        }
        else if (score >= 5000) 
        {
            // ได้คะแนน 4000 - 7999 ได้รองเท้า
                    int roll = Random.Range(1, 101);
            
            if (roll < 41)
            {
                 EquipmentManager.Instance.UnlockItem(HearthNeckless);
                  showImage.sprite = itemImages[5]; 
                 showImage.gameObject.SetActive(true);
            }
            else if (roll >21 && roll < 81)
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
            // ได้คะแนนน้อย (0 - 3999) ได้แหวน
           int roll = Random.Range(1, 101);
            
            if (roll < 41)
            {
                 EquipmentManager.Instance.UnlockItem(WhiteFeather);
                  showImage.sprite = itemImages[7]; 
                 showImage.gameObject.SetActive(true);
            }
            else if (roll >21 && roll < 81)
            {
                 EquipmentManager.Instance.UnlockItem(DawnRign);
                  showImage.sprite = itemImages[8]; 
                 showImage.gameObject.SetActive(true);
            }
             else if (roll >21 && roll < 81)
            {
                 EquipmentManager.Instance.UnlockItem(Armor);
                  showImage.sprite = itemImages[9]; 
                 showImage.gameObject.SetActive(true);
            }
             else if (roll >21 && roll < 81)
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
            // กรณีคะแนนติดลบ ไม่ได้อะไรเลย (เปิดปิดรูปทิ้งไว้)
            showImage.gameObject.SetActive(false);
        }
    }
}