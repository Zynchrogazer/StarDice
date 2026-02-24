using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePairPrefab;
    public float spawnRate = 2f;
    public float minY = -2f;
    public float maxY = 2f;
    public float spawnX = 10f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnPipePair), 1f, spawnRate);
    }

    void SpawnPipePair()
    {
        float y = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, y, 0f);
        Instantiate(pipePairPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Spawned PipePair at " + y);
    }
}
