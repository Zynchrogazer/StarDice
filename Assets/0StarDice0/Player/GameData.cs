using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public PlayerData selectedPlayer;
    public CardData[] savedDeck;
    public List<CardData> selectedDeck = new List<CardData>();
    public List<CardData> selectedCards = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        selectedPlayer = player;
    }

    public void SetSelectedCards(List<CardData> cards)
    {
        selectedCards = cards ?? new List<CardData>();
    }
}
