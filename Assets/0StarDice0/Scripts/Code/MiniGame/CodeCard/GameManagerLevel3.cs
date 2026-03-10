using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerLevel3 : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    

    public GameObject cardPrefab;
    public Transform cardParent;
    public TMP_Text mistakeText;

    public Color[] cardColors; // <- ใส่สีที่ไม่ซ้ำกัน 8 สี

    private Card firstCard, secondCard;
    private int matchedPairs = 0;
    private int mistakes = 0;
    private int totalPairs = 15;

    public bool IsBusy = false;

    public TMP_Text timerText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button nextButton;
    private float timeRemaining = 60f;
    private bool gameEnded = false;

    public int score = 0;

    public TMP_Text scoreText; // อย่าลืม using TMPro;


    void Start()
    {
        ResolveScoreManager();
        CreateCards();
        mistakeText.text = "Mistakes: 0";

        resultPanel.SetActive(false);
        nextButton.onClick.AddListener(GoToNextScene);

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

            card.ConfigureSelection(OnCardSelected, () => !IsBusy);

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
               ResolveScoreManager()?.AddScore(100);
            firstCard.isMatched = true;
            secondCard.isMatched = true;
            matchedPairs++;
        }
        else
        {
             SubtractScore(10);
             ResolveScoreManager()?.SubtractScore(10);

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

    private ScoreManager ResolveScoreManager()
    {
        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        return scoreManager;
    }

public void AddScore(int amount)
{
    score += amount;
    Debug.Log("Score: " + score); // สำหรับเทสต์
    UpdateScoreUI(); // ถ้ามี UI
}


    void UpdateScoreUI()
    {
        if (scoreText != null)
       
             scoreText.text = "Total Score: " + (ResolveScoreManager() != null ? ResolveScoreManager().totalScore : score);
    }

public void SubtractScore(int amount)
{
    score -= amount;

    // ไม่ให้ติดลบ (ถ้าอยากจำกัด)
    if (score < 0) score = 0;

    UpdateScoreUI();
}



void EndGame(bool won)
{
    gameEnded = true;
    resultPanel.SetActive(true);

    MiniGameRewardService.TryGrantCreditReward(ResolveScoreManager() != null ? ResolveScoreManager().totalScore : score, "CardMemory");

    if (won)
        resultText.text = "You matched all cards!\nMistakes: " + mistakes;
    else
        resultText.text = "Time's up!\nMistakes: " + mistakes;
}

void GoToNextScene()
{
    if (ResolveScoreManager() != null)
    {
        ResolveScoreManager().ResetScore();
    }
    MiniGameRewardService.ReturnToBoardScene();
}


}
