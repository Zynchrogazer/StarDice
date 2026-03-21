using UnityEngine;

public class FloatingStarsManager : MonoBehaviour
{
    [Header("รายการดวงดาวทั้งหมด")]
    public Transform[] starObjects;       // Array ของดาวที่ต้องการให้ลอย

    [Header("พารามิเตอร์การลอย")]
    public float floatAmplitude = 10f;    // ระยะทางขึ้นลง
    public float floatSpeed = 2f;         // ความเร็วในการลอย

    private Vector3[] startPositions;     // ตำแหน่งเริ่มต้นของแต่ละดาว
    private float[] offsets;              // ค่าชดเชยเวลาของแต่ละดาว

    void Start()
    {
        int count = starObjects.Length;
        startPositions = new Vector3[count];
        offsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            startPositions[i] = starObjects[i].localPosition;
            offsets[i] = Random.Range(0f, Mathf.PI * 2f);  // ให้แต่ละดวงลอยต่างกัน
        }
    }

    void Update()
    {
        for (int i = 0; i < starObjects.Length; i++)
        {
            float newY = Mathf.Sin(Time.time * floatSpeed + offsets[i]) * floatAmplitude;
            starObjects[i].localPosition = startPositions[i] + new Vector3(0f, newY, 0f);
        }
    }
}
