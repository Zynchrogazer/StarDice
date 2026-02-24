using UnityEngine;
//[RequireComponent(typeof(PlayerMovement), typeof(PlayerPathWalker))]
public class PlayerController : MonoBehaviour
{
 
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
        if (playerData != null)
        {
            playerData.OnDied += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerData != null)
        {
            playerData.OnDied -= HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        Debug.LogError($"[PlayerController] {gameObject.name} has died!");
        // ทำลาย object หรือเปลี่ยนสถานะ
        gameObject.SetActive(false);
    }
    public int GetCurrentHP()
    {
        return PlayerState.Instance.PlayerHealth;
    }
}
