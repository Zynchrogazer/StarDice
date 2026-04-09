using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    [FormerlySerializedAs("selectedPlayer")]
    [SerializeField] private PlayerData _selectedPlayer;
    [SerializeField] private PlayerProgress selectedPlayerProgress;
    public CardData[] savedDeck;
    public List<CardData> selectedDeck = new List<CardData>();
    public List<CardData> selectedCards = new List<CardData>();

    public PlayerData SelectedPlayer => _selectedPlayer;
    public PlayerProgress SelectedPlayerProgress => selectedPlayerProgress;

    // Compatibility shim for existing scene references/scripts.
    public PlayerData selectedPlayer
    {
        get => _selectedPlayer;
        set => SetSelectedPlayer(value);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSelectedPlayerProgressLoaded();
            return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

   public void SetSelectedPlayer(PlayerData player)
    {
        _selectedPlayer = player;
        selectedPlayerProgress = PlayerProgressService.LoadForPlayer(player);

        // 🟢 เพิ่มโค้ดส่วนนี้: บังคับปลดล็อคตัวละครทันทีที่ถูกเลือกใช้งาน
        if (player != null)
        {
            UnlockPlayerInPrefs(player.name);
        }
    }

    // 🟢 เพิ่มฟังก์ชันนี้เข้าไปใน GameData: เอาไว้เช็คชื่อและเซฟการปลดล็อค
    private void UnlockPlayerInPrefs(string playerName)
    {
        // แปลงชื่อเป็นตัวพิมพ์เล็กทั้งหมด จะได้หาคำง่ายๆ ไม่ต้องสนตัวพิมพ์เล็ก-ใหญ่
        string nameLower = playerName.ToLower(); 

        // เช็คว่าในชื่อไฟล์ PlayerData มีคำว่าธาตุนั้นๆ ไหม? ถ้ามีก็เซฟปลดล็อคเลย!
        if (nameLower.Contains("water")) PlayerPrefs.SetInt("MonsterWater", 1);
        else if (nameLower.Contains("earth")) PlayerPrefs.SetInt("MonsterEarth", 1);
        else if (nameLower.Contains("wind")) PlayerPrefs.SetInt("MonsterWind", 1);
        else if (nameLower.Contains("light")) PlayerPrefs.SetInt("MonsterLight", 1);
        else if (nameLower.Contains("dark")) PlayerPrefs.SetInt("MonsterDark", 1);
        else if (nameLower.Contains("fire")) PlayerPrefs.SetInt("MonsterFire", 1);

        PlayerPrefs.Save(); // บันทึกข้อมูล
        Debug.Log($"🔓 [GameData] ปลดล็อคตัวละครอัตโนมัติจากชื่อ: {playerName}");
    }
    internal void SetSelectedPlayerProgressInternal(PlayerProgress progress)
    {
        selectedPlayerProgress = progress;
    }

    public void SetSelectedCards(List<CardData> cards)
    {
        selectedCards = cards ?? new List<CardData>();
    }

    public void EnsureSelectedPlayerProgressLoaded()
    {
        selectedPlayerProgress = PlayerProgressService.EnsureSelectedPlayerProgress(this);
    }

    public int GetSelectedPlayerCredit(int fallback = 0)
    {
        return PlayerProgressService.GetSelectedPlayerCredit(this, fallback);
    }

    public void SetSelectedPlayerCredit(int amount)
    {
        PlayerProgressService.SetSelectedPlayerCredit(this, amount);
    }

    public void AddSelectedPlayerCredit(int amount)
    {
        PlayerProgressService.AddSelectedPlayerCredit(this, amount);
    }

    public bool TrySpendSelectedPlayerCredit(int amount)
    {
        return PlayerProgressService.TrySpendSelectedPlayerCredit(this, amount);
    }

    public void SetSelectedPlayerLevelProgress(int level, int currentExp, int maxExp)
    {
        PlayerProgressService.SetSelectedPlayerLevelProgress(this, level, currentExp, maxExp);
    }
}
