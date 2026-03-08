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
    public TextMeshProUGUI scoreText; // <-- เพิ่ม Text คะแนน

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

    void Start()
    {
        timer = gameDuration;
        UpdateScoreUI();
        StartCoroutine(GameLoop());
        gameOverPanel.SetActive(false);

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

        // รอจน phasePassed เป็น true จากการเล่นสำเร็จเท่านั้น
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
            // กดถูก
            if (buttons[index] != null)
                buttons[index].image.color = correctColor;

                
    score += 20; // ✅ ได้ 20 คะแนนต่อจุด
    UpdateScoreUI();

            if (playerInput.Count == currentPattern.Count)
            {
                // ถูกหมด
                score += 1000;
                UpdateScoreUI();
                isAnswerPhase = false;
                phasePassed = true; // ไป phase ถัดไป
            }
        }
        else
        {
            // กดผิด
            if (buttons[index] != null)
                buttons[index].image.color = wrongColor;

            score -= 500;
            UpdateScoreUI();
            isAnswerPhase = false;

            // รีเริ่ม phase เดิม
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
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        isGameOver = true;
        Debug.Log("🎯 Game Over!");
        phaseText.text = "Game Over";

         gameOverPanel.SetActive(true);
    finalScoreText.text = "Your Score: " + score.ToString();
    MiniGameRewardService.TryGrantCreditReward(score, "MemoryGame");

    // เพิ่ม EventListener ให้ปุ่ม
    nextSceneButton.onClick.RemoveAllListeners();
    nextSceneButton.onClick.AddListener(() =>
    {
        MiniGameRewardService.ReturnToBoardScene();
    });

    Debug.Log("🎯 Game Over!");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}
