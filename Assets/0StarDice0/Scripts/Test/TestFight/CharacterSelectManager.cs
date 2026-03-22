using UnityEngine;
using UnityEngine.Serialization;


public class CharacterSelectManager : MonoBehaviour
{
    [FormerlySerializedAs("selectedPlayer")]
    [SerializeField] private PlayerData defaultSelectedPlayer;

    public PlayerData SelectedPlayer
    {
        get => GameData.Instance != null ? GameData.Instance.SelectedPlayer : defaultSelectedPlayer;
        private set
        {
            if (GameData.Instance != null)
            {
                GameData.Instance.SetSelectedPlayer(value);
            }
            else
            {
                defaultSelectedPlayer = value;
            }
        }
    }

    private void Awake()
    {
        if (FindObjectsByType<CharacterSelectManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        if (GameData.Instance != null && GameData.Instance.SelectedPlayer == null && defaultSelectedPlayer != null)
        {
            GameData.Instance.SetSelectedPlayer(defaultSelectedPlayer);
        }
    }

    public void SelectCharacter(PlayerData player)
    {
        SelectedPlayer = player;
    }
}
