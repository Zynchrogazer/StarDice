using UnityEngine;
//[RequireComponent(typeof(PlayerMovement), typeof(PlayerPathWalker))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    public PlayerData playerData;
    public PlayerData GetData()
    {
        return playerData;
    }
    void Awake()
    {
       
    }

    private void OnEnable()
    {
        PlayerState resolvedPlayerState = ResolvePlayerState();
        if (resolvedPlayerState != null)
        {
            resolvedPlayerState.OnDied += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerState != null)
        {
            playerState.OnDied -= HandlePlayerDeath;
        }
    }

    private PlayerState ResolvePlayerState()
    {
        if (playerState == null)
        {
            playerState = GetComponent<PlayerState>();
        }

        if (playerState == null)
        {
            playerState = GameTurnManager.CurrentPlayer;
        }

        return playerState;
    }

    private void HandlePlayerDeath()
    {
        Debug.LogError($"[PlayerController] {gameObject.name} has died!");
        // ทำลาย object หรือเปลี่ยนสถานะ
        gameObject.SetActive(false);
    }
    public int GetCurrentHP()
    {
        return GameTurnManager.CurrentPlayer.PlayerHealth;
    }
}
