using UnityEngine;

public class RotatingStar : MonoBehaviour
{
    public float floatAmplitude = 10f;   // ระยะทางขึ้นลง (พิกเซล)
    public float floatSpeed = 2f;        // ความเร็วลอย
    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.localPosition;
        offset = Random.Range(0f, Mathf.PI * 2); // ทำให้แต่ละดวงลอยไม่พร้อมกัน
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed + offset) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0f, newY, 0f);
    }
}
