using UnityEngine;

// บังคับว่า GameObject นี้ต้องมี AudioSource แปะอยู่เสมอ
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (instance == null)
        {
            // 1. ถ้ายังไม่มีใครเล่นเพลง ให้ตั้งตัวเองเป็นหัวหน้า และลอยข้ามซีน
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // 2. ถ้ามีหัวหน้าอยู่แล้ว ให้เช็คว่า "ซีนนี้ขอเล่นเพลงใหม่ไหม?"
            AudioClip newClip = this.audioSource.clip;
            AudioClip currentClip = instance.audioSource.clip;

            // ถ้าเพลงที่ตั้งไว้ในซีนนี้ ไม่เหมือนกับที่หัวหน้ากำลังเล่นอยู่...
            if (newClip != currentClip)
            {
                // 🟢 สั่งให้หัวหน้าเปลี่ยนแผ่นเสียง แล้วกด Play ทันที!
                instance.audioSource.clip = newClip;
                instance.audioSource.Play();
                Debug.Log($"[BGMManager] สลับเพลงเป็น: {(newClip != null ? newClip.name : "ไม่มีเพลง")}");
            }

            // 3. ทำลายตัวเองทิ้ง ปล่อยให้หัวหน้า (instance) จัดการเรื่องเสียงต่อไป
            Destroy(gameObject);
        }
    }
}