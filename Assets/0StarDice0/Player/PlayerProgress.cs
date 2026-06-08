using System;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    // Credit is shared across all monsters so changing the selected monster does not swap wallets.
    // The legacy per-player key is still written as a fallback for older saves/tools.
    private const string CreditKeyPrefix = "PLAYER_PROGRESS_CREDIT_";
    private const string SharedCreditKey = "PLAYER_PROGRESS_SHARED_CREDIT";
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

        credit = LoadSharedCreditWithLegacyFallback();
        level = Mathf.Max(1, PlayerPrefs.GetInt(GetLevelKey(playerId), level));
        currentExp = Mathf.Max(0, PlayerPrefs.GetInt(GetCurrentExpKey(playerId), currentExp));
        maxExp = Mathf.Max(1, PlayerPrefs.GetInt(GetMaxExpKey(playerId), maxExp));
    }

    private int LoadSharedCreditWithLegacyFallback()
    {
        if (PlayerPrefs.HasKey(SharedCreditKey))
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(SharedCreditKey, credit));
        }

        int migratedCredit = Mathf.Max(0, PlayerPrefs.GetInt(GetCreditKey(playerId), credit));
        PlayerPrefs.SetInt(SharedCreditKey, migratedCredit);
        PlayerPrefs.Save();
        return migratedCredit;
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(playerId)) return;

        SaveCreditFields(false);
        SaveProgressFields(false);
        PlayerPrefs.Save();
    }

    private void SaveCreditFields(bool flush = true)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        PlayerPrefs.SetInt(SharedCreditKey, credit);
        PlayerPrefs.SetInt(GetCreditKey(playerId), credit);

        if (flush)
        {
            PlayerPrefs.Save();
        }
    }

    private void SaveProgressFields(bool flush = true)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        PlayerPrefs.SetInt(GetLevelKey(playerId), level);
        PlayerPrefs.SetInt(GetCurrentExpKey(playerId), currentExp);
        PlayerPrefs.SetInt(GetMaxExpKey(playerId), maxExp);

        if (flush)
        {
            PlayerPrefs.Save();
        }
    }

    public void SetCredit(int amount)
    {
        int normalized = Mathf.Max(0, amount);
        bool sharedCreditIsCurrent = PlayerPrefs.HasKey(SharedCreditKey) && PlayerPrefs.GetInt(SharedCreditKey, normalized) == normalized;
        if (credit == normalized && sharedCreditIsCurrent) return;

        credit = normalized;
        SaveCreditFields();
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
        SaveProgressFields();
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

    public static void ResetSharedCredit(int creditAmount = 0)
    {
        PlayerPrefs.SetInt(SharedCreditKey, Mathf.Max(0, creditAmount));
        PlayerPrefs.Save();
    }

    private static string GetCreditKey(string id) => CreditKeyPrefix + id;
    private static string GetLevelKey(string id) => LevelKeyPrefix + id;
    private static string GetCurrentExpKey(string id) => CurrentExpKeyPrefix + id;
    private static string GetMaxExpKey(string id) => MaxExpKeyPrefix + id;
}
