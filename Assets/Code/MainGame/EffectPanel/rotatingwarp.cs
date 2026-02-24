using UnityEngine;

public class rotatingwarp : MonoBehaviour
{
    public float rotationSpeed = 100f; // ความเร็วในการหมุน (องศาต่อวินาที)

    void Update()
    {
        // หมุนรอบ Z (แกน 2D)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
