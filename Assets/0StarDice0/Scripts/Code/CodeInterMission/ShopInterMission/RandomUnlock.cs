using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RandomUnlock : MonoBehaviour
{
    [Header("Economy Setup")]
    public int rollPrice = 100;
    public TMP_Text creditText;

    [Header("UI Setup")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button rollButton;
    public Button closeButton;
    public Button resetButton;

    [Header("Monster Images")]
    public GameObject waterImage;
    public GameObject earthImage;
    public GameObject windImage;
    public GameObject lightImage;
    public GameObject darkImage;
    public GameObject fireImage;

    private void OnEnable()
    {
        UpdateCreditText();
    }

    private void Start()
    {
        resultPanel.SetActive(false);
        CheckRollButtonState();
        UpdateCreditText();

        closeButton.onClick.AddListener(() =>
        {
            resultPanel.SetActive(false);
        });

        rollButton.onClick.AddListener(() =>
        {
            RollMonster();
        });

        resetButton.onClick.AddListener(() =>
        {
            ResetAllMonsters();
            UpdateMonsterUI(); 
            resultText.text = "Reset Monster";
            resultPanel.SetActive(true);
            CheckRollButtonState(); 
        });
    }

    public void RollMonster()
    {
        // 🟢 1. ดึงเงินปัจจุบัน (ฉลาดขึ้น: รองรับทั้งฉาก Board และฉาก Intermission)
        int currentCredit = 0;
        PlayerState boardPlayer = GameTurnManager.CurrentPlayer;

        if (boardPlayer != null)
        {
            currentCredit = boardPlayer.PlayerCredit; // ดึงจากกระดาน
        }
        else if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            currentCredit = GameData.Instance.selectedPlayer.Credit; // ดึงจากเซฟหลัก
        }
        else
        {
            currentCredit = PlayerPrefs.GetInt("PlayerCredit", 0); // ระบบสำรองเผื่อเทสต์เดี่ยวๆ
        }

        // 🟢 2. เช็คว่าเงินพอไหม?
        if (currentCredit < rollPrice)
        {
            resultText.text = "Not Enough Credit!";
            resultPanel.SetActive(true);
            return; 
        }

        // เตรียมมอนสเตอร์ที่เหลือ
        List<int> availableMonsters = new List<int>();

        if (PlayerPrefs.GetInt("MonsterFire", 0) == 0) availableMonsters.Add(1);
        if (PlayerPrefs.GetInt("MonsterWater", 0) == 0) availableMonsters.Add(2);
        if (PlayerPrefs.GetInt("MonsterEarth", 0) == 0) availableMonsters.Add(3);
        if (PlayerPrefs.GetInt("MonsterWind", 0) == 0) availableMonsters.Add(4);
        if (PlayerPrefs.GetInt("MonsterLight", 0) == 0) availableMonsters.Add(5);
        if (PlayerPrefs.GetInt("MonsterDark", 0) == 0) availableMonsters.Add(6);

        if (availableMonsters.Count == 0)
        {
            resultText.text = "You Got All Monsters!";
            resultPanel.SetActive(true);
            rollButton.interactable = false; 
            return; 
        }

        // 🟢 3. หักเงินและเซฟข้อมูลกลับไปที่เดิม
        currentCredit -= rollPrice;

        if (boardPlayer != null)
        {
            boardPlayer.PlayerCredit = currentCredit; // ให้ PlayerState จัดการเซฟให้
        }
        else if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetCredit(currentCredit); // เซฟลง GameData โดยตรง
        }
        else
        {
            PlayerPrefs.SetInt("PlayerCredit", currentCredit); // เซฟสำรอง
        }

        UpdateCreditText(); // อัปเดตตัวหนังสือ

        // 🟢 4. สุ่มมอนสเตอร์
        int randomIndex = Random.Range(0, availableMonsters.Count);
        int finalMonsterId = availableMonsters[randomIndex]; 

        string monsterName = "";
        HideAllImages();

        switch (finalMonsterId)
        {
            case 1: monsterName = "MonsterFire"; fireImage.SetActive(true); break;
            case 2: monsterName = "MonsterWater"; waterImage.SetActive(true); break;
            case 3: monsterName = "MonsterEarth"; earthImage.SetActive(true); break;
            case 4: monsterName = "MonsterWind"; windImage.SetActive(true); break;
            case 5: monsterName = "MonsterLight"; lightImage.SetActive(true); break;
            case 6: monsterName = "MonsterDark"; darkImage.SetActive(true); break;
        }

        PlayerPrefs.SetInt(monsterName, 1);
        PlayerPrefs.Save();

        resultText.text = "You Got " + monsterName + " !";
        resultPanel.SetActive(true);

        CheckRollButtonState();
        UpdateMonsterUI();
    }

    // 🟢 อัปเดตฟังก์ชันดึงเงินมาแสดงบน Text
    private void UpdateCreditText()
    {
        if (creditText == null) return;

        int currentCredit = 0;

        if (GameTurnManager.CurrentPlayer != null)
        {
            currentCredit = GameTurnManager.CurrentPlayer.PlayerCredit;
        }
        else if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            currentCredit = GameData.Instance.selectedPlayer.Credit;
        }
        else
        {
            currentCredit = PlayerPrefs.GetInt("PlayerCredit", 0);
        }

        creditText.text = "Credit: " + currentCredit.ToString();
    }

    private void CheckRollButtonState()
    {
        bool hasFire = PlayerPrefs.GetInt("MonsterFire", 0) == 1;
        bool hasWater = PlayerPrefs.GetInt("MonsterWater", 0) == 1;
        bool hasEarth = PlayerPrefs.GetInt("MonsterEarth", 0) == 1;
        bool hasWind = PlayerPrefs.GetInt("MonsterWind", 0) == 1;
        bool hasLight = PlayerPrefs.GetInt("MonsterLight", 0) == 1;
        bool hasDark = PlayerPrefs.GetInt("MonsterDark", 0) == 1;

        if (hasFire && hasWater && hasEarth && hasWind && hasLight && hasDark)
        {
            rollButton.interactable = false; 
        }
        else
        {
            rollButton.interactable = true; 
        }
    }

    private void HideAllImages()
    {
        darkImage.SetActive(false);
        waterImage.SetActive(false);
        earthImage.SetActive(false);
        windImage.SetActive(false);
        lightImage.SetActive(false);
        fireImage.SetActive(false);
    }

    private void ResetAllMonsters()
    {
        PlayerPrefs.SetInt("MonsterFire", 0);
        PlayerPrefs.SetInt("MonsterWater", 0);
        PlayerPrefs.SetInt("MonsterEarth", 0);
        PlayerPrefs.SetInt("MonsterWind", 0);
        PlayerPrefs.SetInt("MonsterLight", 0);
        PlayerPrefs.SetInt("MonsterDark", 0);
        PlayerPrefs.Save();
    }

    private void UpdateMonsterUI()
    {
        fireImage.SetActive(PlayerPrefs.GetInt("MonsterFire", 0) == 1);
        waterImage.SetActive(PlayerPrefs.GetInt("MonsterWater", 0) == 1);
        earthImage.SetActive(PlayerPrefs.GetInt("MonsterEarth", 0) == 1);
        windImage.SetActive(PlayerPrefs.GetInt("MonsterWind", 0) == 1);
        lightImage.SetActive(PlayerPrefs.GetInt("MonsterLight", 0) == 1);
        darkImage.SetActive(PlayerPrefs.GetInt("MonsterDark", 0) == 1);
    }
}