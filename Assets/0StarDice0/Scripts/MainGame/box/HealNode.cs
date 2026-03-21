using UnityEngine;

public class HealNode : MonoBehaviour
{
    public PlayerHealth playerHealth;

    private void Awake()
    {
        if (playerHealth == null)
        {
            var playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
    }

    public void ActivateHeal()
    {
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(10);
            Debug.Log("[HealNode] Heal activated! Player gained 10 health.");
        }
        else
        {
            Debug.LogWarning("[HealNode] PlayerHealth not assigned.");
        }
    }
}
