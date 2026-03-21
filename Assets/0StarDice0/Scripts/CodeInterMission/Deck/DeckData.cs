using UnityEngine;
public class DeckData : MonoBehaviour
{
    public CardData[] savedDeck = new CardData[20];

    private void Awake()
    {
        if (FindObjectsOfType<DeckData>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // จะอยู่ข้าม Scene
    }
}
