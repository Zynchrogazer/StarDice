using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int totalScore = 0;

    void Awake()
    {
        ScoreManager[] managers = FindObjectsByType<ScoreManager>(FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // เก็บข้าม scene
    }

    public void AddScore(int amount)
    {
        totalScore += amount;
        Debug.Log("Total Score: " + totalScore);
    }

    public void SubtractScore(int amount)
    {
        totalScore -= amount;

        // ป้องกันคะแนนติดลบ ถ้าไม่ต้องการให้ติดลบ
        if (totalScore < 0)
            totalScore = 0;

        Debug.Log("Total Score: " + totalScore);
    }

    public void ResetScore()
    {
        totalScore = 0;
    }
}
