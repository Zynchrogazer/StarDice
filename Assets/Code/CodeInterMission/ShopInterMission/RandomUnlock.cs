using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // 1. ต้องเพิ่มบรรทัดนี้เพื่อใช้ List

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
    public GameObject fireImage;
    public Button resetButton;

    private void Start()
    {
        resultPanel.SetActive(false);

        closeButton.onClick.AddListener(() =>
        {
            resultPanel.SetActive(false);
        });

        rollButton.onClick.AddListener(() =>
        {
            RollMonsterNoDuplicate(); // เรียกฟังก์ชันใหม่
        });

        resetButton.onClick.AddListener(() =>
        {
            ResetAllMonsters();
            UpdateMonsterUI();
            resultText.text = "Reset Monster All";
            resultPanel.SetActive(true);
        });
    }

    // ฟังก์ชันสุ่มแบบไม่ซ้ำ
    public void RollMonsterNoDuplicate()
    {
        HideAllImages();

        // 1. สร้างรายการเก็บ "เลขของตัวที่ยังไม่ได้ปลดล็อค"
        List<int> availableID = new List<int>();

        // เช็คทีละตัวเลยว่า ตัวไหนยังเป็น 0 (ยังไม่มี) ให้เอาเลขนั้นใส่ตะกร้าไว้
        if (PlayerPrefs.GetInt("MonsterWater", 0) == 0) availableID.Add(1);
        if (PlayerPrefs.GetInt("MonsterEarth", 0) == 0) availableID.Add(2);
        if (PlayerPrefs.GetInt("MonsterWind", 0) == 0) availableID.Add(3);
        if (PlayerPrefs.GetInt("MonsterLight", 0) == 0) availableID.Add(4);
        if (PlayerPrefs.GetInt("MonsterDark", 0) == 0) availableID.Add(5);
        if (PlayerPrefs.GetInt("MonsterFire", 0) == 0) availableID.Add(6);

        // 2. ถ้าในตะกร้าว่างเปล่า แปลว่าได้ครบทุกตัวแล้ว
        if (availableID.Count == 0)
        {
            resultText.text = "You have all monsters!";
            resultPanel.SetActive(true);
            return; // จบการทำงาน ไม่สุ่มต่อ
        }

        // 3. สุ่มหยิบ 1 เลขจากในตะกร้า (List)
        int randomIndex = Random.Range(0, availableID.Count);
        int finalID = availableID[randomIndex]; // นี่คือเลขที่สุ่มได้

        // 4. แสดงผลตามเลขที่ได้ (Logic เดิม)
        string monsterName = "";

        switch (finalID)
        {
            case 1: monsterName = "MonsterWater"; waterImage.SetActive(true); break;
            case 2: monsterName = "MonsterEarth"; earthImage.SetActive(true); break;
            case 3: monsterName = "MonsterWind"; windImage.SetActive(true); break;
            case 4: monsterName = "MonsterLight"; lightImage.SetActive(true); break;
            case 5: monsterName = "MonsterDark"; darkImage.SetActive(true); break;
            case 6: monsterName = "MonsterFire"; fireImage.SetActive(true); break;
        }

        // บันทึก
        PlayerPrefs.SetInt(monsterName, 1);
        PlayerPrefs.Save();

        resultText.text = "You Got " + monsterName + " !";
        resultPanel.SetActive(true);
    }

    private void HideAllImages()
    {
        waterImage.SetActive(false);
        earthImage.SetActive(false);
        windImage.SetActive(false);
        lightImage.SetActive(false);
        darkImage.SetActive(false);
        if(fireImage) fireImage.SetActive(false);
    }

    private void ResetAllMonsters()
    {
        PlayerPrefs.SetInt("MonsterWater", 0);
        PlayerPrefs.SetInt("MonsterEarth", 0);
        PlayerPrefs.SetInt("MonsterWind", 0);
        PlayerPrefs.SetInt("MonsterLight", 0);
        PlayerPrefs.SetInt("MonsterDark", 0);
        PlayerPrefs.SetInt("MonsterFire", 0);
        PlayerPrefs.Save();
    }

    private void UpdateMonsterUI()
    {
        // ส่วนนี้ไว้โชว์สถานะในหน้าสุ่ม (ถ้าจำเป็น)
        if(waterImage) waterImage.SetActive(PlayerPrefs.GetInt("MonsterWater") == 1);
        // ... ใส่เพิ่มให้ครบถ้าต้องการ
    }
}