using UnityEngine;
using System.Collections.Generic;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

