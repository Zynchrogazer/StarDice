using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System; // ✅ เพิ่มเพื่อใช้ Action

public class BoxOpener : MonoBehaviour
{
    public Image boxImage;
    public Sprite boxOpenSprite;
    public Image rewardImage;
    // public Sprite[] rewardSprites; // ❌ ไม่ใช้สุ่มเองแล้ว ให้รับมาจาก Manager แทนจะได้ตรงกัน
    public float revealDelay = 1f;

    public RectTransform boxTransform;
    public float shakeDuration = 0.5f;
    public float shakeStrength = 10f;

    private bool isOpened = false;

    // เก็บ Sprite กล่องปิดไว้คืนค่าตอนเริ่มใหม่
    private Sprite defaultBoxSprite;

    private void Awake()
    {
        if (boxImage != null) defaultBoxSprite = boxImage.sprite;
    }

    private void OnEnable()
    {
        // รีเซ็ตค่าทุกครั้งที่เปิด Panel ขึ้นมาใหม่
        isOpened = false;
        if (boxImage != null && defaultBoxSprite != null) boxImage.sprite = defaultBoxSprite;
        if (rewardImage != null) rewardImage.gameObject.SetActive(false);
    }

    // ✅ ฟังก์ชันเปิดกล่องแบบใหม่: รับรูปรางวัล + คำสั่งตอนจบ (onComplete)
    public void OpenBox(Sprite resultSprite, Action onComplete)
    {
        if (isOpened) return;
        isOpened = true;

        StartCoroutine(ShakeAndOpen(resultSprite, onComplete));
    }

    IEnumerator ShakeAndOpen(Sprite resultSprite, Action onComplete)
    {
        // 1. สั่นกล่อง
        Vector3 originalPos = boxTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * shakeStrength;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * shakeStrength;
            boxTransform.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boxTransform.anchoredPosition = originalPos;

        // 2. เปิดกล่อง
        if (boxImage != null) boxImage.sprite = boxOpenSprite;

        // 3. โชว์รางวัล
        yield return new WaitForSeconds(revealDelay);

        if (rewardImage != null)
        {
            rewardImage.sprite = resultSprite; // ใช้รูปที่รับมาจาก Manager
            rewardImage.gameObject.SetActive(true);
        }

        // 4. รอให้คนดูรางวัลแปปนึง (3 วินาที)
        yield return new WaitForSeconds(3f);

        // ✅ 5. ส่งสัญญาณกลับไปบอก Manager ว่า "เสร็จแล้ว! ปิดได้!"
        onComplete?.Invoke();
    }
}