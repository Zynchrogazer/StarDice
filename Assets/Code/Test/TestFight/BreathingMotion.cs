using UnityEngine;

public class BreathingMotion : MonoBehaviour
{
    public float amplitude = 20f;   // ระยะขึ้นลง (px)
    public float speed = 10f;       // ความเร็วในการหายใจ

    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.localPosition;
        offset = Random.Range(0f, Mathf.PI * 2f); // ให้แต่ละตัวไม่ sync กันเป๊ะ
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed + offset) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, y, 0f);
    }
}
