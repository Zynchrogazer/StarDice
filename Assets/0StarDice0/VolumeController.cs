using UnityEngine;
using UnityEngine.Audio; // ต้องใส่เพื่อใช้งาน AudioMixer
using UnityEngine.UI;    // ต้องใส่เพื่อใช้งาน Slider

public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // กำหนดให้ Slider เรียกใช้ฟังก์ชัน SetVolume ทุกครั้งที่ถูกเลื่อน
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        // แปลงค่าจาก Slider (0.0001 ถึง 1) เป็นค่า Decibel สำหรับ Mixer
        // โดยใช้สูตร Log10 * 20 เพื่อให้เสียงค่อยๆ ลดลงอย่างเป็นธรรมชาติ
        float volumeInDecibels = Mathf.Log10(sliderValue) * 20;
        
        // ส่งค่าไปที่ Exposed Parameter ที่เราตั้งชื่อไว้
        audioMixer.SetFloat("MasterVolume", volumeInDecibels);
    }
}