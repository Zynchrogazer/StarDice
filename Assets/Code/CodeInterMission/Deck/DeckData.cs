using UnityEngine;
public class DeckData : MonoBehaviour
{
    public static DeckData Instance;

    public CardData[] savedDeck = new CardData[20];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // จะอยู่ข้าม Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
