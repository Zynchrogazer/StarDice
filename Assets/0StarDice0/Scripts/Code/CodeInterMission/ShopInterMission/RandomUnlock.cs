using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RandomUnlock : MonoBehaviour
{
    [Header("Cost")]
    [Tooltip("ค่าใช้จ่ายต่อการสุ่ม 1 ครั้ง")]
    public int price = 10000;

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

        RefreshRollButtonState();

    }

    private void OnEnable()
    {
        RegisterCreditListener();
        RefreshRollButtonState();
    }

    private void OnDisable()
    {
        UnregisterCreditListener();
    }

    public void RollMonster()
    {
        if (!TrySpendIntermissionCredit(price, out int remainingCredit))
        {
            resultText.text = $"Credit ไม่พอ (ต้องใช้ {price}, มี {GetCurrentCredit()})";
            resultPanel.SetActive(true);
            RefreshRollButtonState();
            return;
        }

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
        resultText.text = "You Got " + monsterName + $" !\n(จ่าย {price}, เหลือ {remainingCredit})";

        // เปิด Panel
        resultPanel.SetActive(true);

        RefreshRollButtonState();
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

    private void RegisterCreditListener()
    {
        if (GameData.Instance == null || GameData.Instance.selectedPlayer == null) return;
        GameData.Instance.selectedPlayer.OnCreditChanged += HandleCreditChanged;
    }

    private void UnregisterCreditListener()
    {
        if (GameData.Instance == null || GameData.Instance.selectedPlayer == null) return;
        GameData.Instance.selectedPlayer.OnCreditChanged -= HandleCreditChanged;
    }

    private void HandleCreditChanged(int _)
    {
        RefreshRollButtonState();
    }

    private void RefreshRollButtonState()
    {
        if (rollButton == null) return;
        rollButton.interactable = GetCurrentCredit() >= Mathf.Max(1, price);
    }

    private int GetCurrentCredit()
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
        {
            return Mathf.Max(0, GameData.Instance.selectedPlayer.Credit);
        }

        return 0;
    }

    private bool TrySpendIntermissionCredit(int amount, out int remainingCredit)
    {
        remainingCredit = GetCurrentCredit();
        if (amount <= 0)
        {
            Debug.LogWarning($"[RandomUnlock] ราคา roll ไม่ถูกต้อง ({amount}) จึงไม่อนุญาตให้สุ่ม");
            return false;
        }

        if (GameData.Instance == null || GameData.Instance.selectedPlayer == null)
        {
            return false;
        }

        if (!GameData.Instance.selectedPlayer.TrySpendCredit(amount))
        {
            return false;
        }

        remainingCredit = GameData.Instance.selectedPlayer.Credit;
        return true;
    }

}