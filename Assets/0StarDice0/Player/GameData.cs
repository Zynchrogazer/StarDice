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
