using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RandomUnlock : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button rollButton;
    public Button closeButton;

    [Header("Monster Images")]
    public GameObject waterImage;
    public GameObject earthImage;
    public GameObject windImage;
    public GameObject lightImage;
    public GameObject darkImage;
    public Button resetButton;
    private void Start()
    {
        resultPanel.SetActive(false);

        // ปุ่ม Close → ปิด Panel
        closeButton.onClick.AddListener(() =>
        {
            resultPanel.SetActive(false);
        });

        // ปุ่ม Roll → สุ่ม Monster
        rollButton.onClick.AddListener(() =>
        {
            RollMonster();
        });

       resetButton.onClick.AddListener(() =>
{
    ResetAllMonsters();
    UpdateMonsterUI(); // รีเฟรช UI
    resultText.text = "Reset Monster";
    resultPanel.SetActive(true);
});

    }

    public void RollMonster()
    {
        int randomNum = Random.Range(1, 6); // สุ่ม 1-5
        string monsterName = "";

        // ซ่อนรูปทั้งหมดก่อน
        HideAllImages();

        switch (randomNum)
        {
            case 1: monsterName = "MonsterWater"; waterImage.SetActive(true); break;
            case 2: monsterName = "MonsterEarth"; earthImage.SetActive(true); break;
            case 3: monsterName = "MonsterWind"; windImage.SetActive(true); break;
            case 4: monsterName = "MonsterLight"; lightImage.SetActive(true); break;
            case 5: monsterName = "MonsterDark"; darkImage.SetActive(true); break;
        }

        // บันทึกว่าได้ตัวนี้แล้ว
        PlayerPrefs.SetInt(monsterName, 1);
        PlayerPrefs.Save();

        // อัพเดตข้อความ
        resultText.text = "You Got" + monsterName + " !";

        // เปิด Panel
        resultPanel.SetActive(true);
    }

    private void HideAllImages()
    {
        waterImage.SetActive(false);
        earthImage.SetActive(false);
        windImage.SetActive(false);
        lightImage.SetActive(false);
        darkImage.SetActive(false);
    }

    private void ResetAllMonsters()
    {
        PlayerPrefs.SetInt("MonsterWater", 0);
        PlayerPrefs.SetInt("MonsterEarth", 0);
        PlayerPrefs.SetInt("MonsterWind", 0);
        PlayerPrefs.SetInt("MonsterLight", 0);
        PlayerPrefs.SetInt("MonsterDark", 0);
        PlayerPrefs.Save();
    }
    private void UpdateMonsterUI()
{
    waterImage.SetActive(PlayerPrefs.GetInt("MonsterWater") == 1);
    earthImage.SetActive(PlayerPrefs.GetInt("MonsterEarth") == 1);
    windImage.SetActive(PlayerPrefs.GetInt("MonsterWind") == 1);
    lightImage.SetActive(PlayerPrefs.GetInt("MonsterLight") == 1);
    darkImage.SetActive(PlayerPrefs.GetInt("MonsterDark") == 1);
}

}
