using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddScore(1);
        }
    }
}
