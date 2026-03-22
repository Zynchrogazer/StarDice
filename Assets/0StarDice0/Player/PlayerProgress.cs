using System;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    private const string CreditKeyPrefix = "PLAYER_PROGRESS_CREDIT_";
    private const string LevelKeyPrefix = "PLAYER_PROGRESS_LEVEL_";
    private const string CurrentExpKeyPrefix = "PLAYER_PROGRESS_CURRENT_EXP_";
    private const string MaxExpKeyPrefix = "PLAYER_PROGRESS_MAX_EXP_";

    [SerializeField] private string playerId;
    [SerializeField] private int credit;
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp = 100;

    public event Action<int> OnCreditChanged;
    public event Action OnProgressChanged;

    public string PlayerId => playerId;
    public int Credit => credit;
    public int Level => level;
    public int CurrentExp => currentExp;
    public int MaxExp => maxExp;

    public static PlayerProgress Create(PlayerData playerData)
    {
        PlayerProgress progress = new PlayerProgress();
        progress.Initialize(playerData);
        progress.Load();
        return progress;
    }

    public void Initialize(PlayerData playerData)
    {
        playerId = ResolvePlayerId(playerData);
        credit = Mathf.Max(0, playerData != null ? playerData.startingCredit : 0);
        level = Mathf.Max(1, playerData != null ? playerData.startingLevel : 1);
        currentExp = Mathf.Max(0, playerData != null ? playerData.startingCurrentExp : 0);
        maxExp = Mathf.Max(1, playerData != null ? playerData.startingMaxExp : 100);
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(playerId)) return;

        credit = Mathf.Max(0, PlayerPrefs.GetInt(GetCreditKey(playerId), credit));
        level = Mathf.Max(1, PlayerPrefs.GetInt(GetLevelKey(playerId), level));
        currentExp = Mathf.Max(0, PlayerPrefs.GetInt(GetCurrentExpKey(playerId), currentExp));
        maxExp = Mathf.Max(1, PlayerPrefs.GetInt(GetMaxExpKey(playerId), maxExp));
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(playerId)) return;

        PlayerPrefs.SetInt(GetCreditKey(playerId), credit);
        PlayerPrefs.SetInt(GetLevelKey(playerId), level);
        PlayerPrefs.SetInt(GetCurrentExpKey(playerId), currentExp);
        PlayerPrefs.SetInt(GetMaxExpKey(playerId), maxExp);
        PlayerPrefs.Save();
    }

    public void SetCredit(int amount)
    {
        int normalized = Mathf.Max(0, amount);
        if (credit == normalized) return;
        credit = normalized;
        Save();
        OnCreditChanged?.Invoke(credit);
        OnProgressChanged?.Invoke();
    }

    public void AddCredit(int amount)
    {
        if (amount <= 0) return;
        SetCredit(credit + amount);
    }

    public bool TrySpendCredit(int amount)
    {
        if (amount <= 0) return true;
        if (credit < amount) return false;

        SetCredit(credit - amount);
        return true;
    }

    public void SetLevelProgress(int newLevel, int newCurrentExp, int newMaxExp)
    {
        level = Mathf.Max(1, newLevel);
        currentExp = Mathf.Max(0, newCurrentExp);
        maxExp = Mathf.Max(1, newMaxExp);
        Save();
        OnProgressChanged?.Invoke();
    }

    public void ResetToDefaults(PlayerData playerData)
    {
        Initialize(playerData);
        Save();
        OnCreditChanged?.Invoke(credit);
        OnProgressChanged?.Invoke();
    }

    public static string ResolvePlayerId(PlayerData playerData)
    {
        if (playerData == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(playerData.playerId)) return playerData.playerId;
        if (!string.IsNullOrWhiteSpace(playerData.playerName)) return playerData.playerName;
        return playerData.name;
    }

    public static void ResetStoredProgress(PlayerData playerData)
    {
        string id = ResolvePlayerId(playerData);
        if (string.IsNullOrEmpty(id)) return;

        PlayerPrefs.DeleteKey(GetCreditKey(id));
        PlayerPrefs.DeleteKey(GetLevelKey(id));
        PlayerPrefs.DeleteKey(GetCurrentExpKey(id));
        PlayerPrefs.DeleteKey(GetMaxExpKey(id));
    }

    private static string GetCreditKey(string id) => CreditKeyPrefix + id;
    private static string GetLevelKey(string id) => LevelKeyPrefix + id;
    private static string GetCurrentExpKey(string id) => CurrentExpKeyPrefix + id;
    private static string GetMaxExpKey(string id) => MaxExpKeyPrefix + id;
}
