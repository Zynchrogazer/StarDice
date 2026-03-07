using UnityEngine;

public class TrapNode : MonoBehaviour
{
    public PlayerHealth playerHealth;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        }
    }

    public void ActivateTrap()
    {
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(-10);
            Debug.Log("Trap activated! -10 HP");
        }
        else
        {
            Debug.LogWarning("PlayerHealth not assigned in TrapNode.");
        }
    }
}
