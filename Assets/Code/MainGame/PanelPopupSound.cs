using UnityEngine;

public class PanelPopupSound : MonoBehaviour
{
    [Header("เลือกเสียงสำหรับ Panel นี้")]
    [Tooltip("ลากเสียงที่ต้องการให้ Panel นี้เล่นมาใส่")]
    public AudioClip openSound; // เสียงตอนเปิด

    [Header("ความดัง")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    // ตัวแปรหาลำโพงกลาง
    private AudioSource audioSource;

    private void Awake()
    {
        // หาตัวเล่นเสียงใน Scene (จะได้ไม่ต้องติด AudioSource ทุก Panel)
        audioSource = FindObjectOfType<AudioSource>();
    }

    private void OnEnable()
    {
        // สั่งเล่นเสียงเมื่อ Panel ถูกเปิด (Active)
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound, volume);
        }
    }
}