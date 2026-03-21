using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    public PlayerData selectedPlayer;

    private void Awake()
    {
        if (FindObjectsByType<CharacterSelectManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        SyncWithGameData();
    }

    public void SelectCharacter(PlayerData player)
    {
        selectedPlayer = player;

        if (GameData.Instance != null)
        {
            GameData.Instance.SetSelectedPlayer(player);
        }
    }

    private void SyncWithGameData()
    {
        if (GameData.Instance == null)
        {
            return;
        }

        if (GameData.Instance.selectedPlayer != null)
        {
            selectedPlayer = GameData.Instance.selectedPlayer;
            return;
        }

        if (selectedPlayer != null)
        {
            GameData.Instance.SetSelectedPlayer(selectedPlayer);
        }
    }
}
