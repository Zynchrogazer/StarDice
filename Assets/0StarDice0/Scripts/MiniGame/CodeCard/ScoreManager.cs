using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // เพิ่มบรรทัดนี้: ประกาศตัวแปร static เพื่อให้สคริปต์อื่นเรียกใช้ผ่าน ScoreManager.Instance ได้
    public static ScoreManager Instance { get; private set; }

    public int totalScore = 0;

    void Awake()
    {
        // ปรับปรุงการเช็ค Singleton ให้กระชับและประสิทธิภาพดีขึ้น
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this; // กำหนดค่าให้ Instance เป็นตัวนี้
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