using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System; // เพิ่มอันนี้เพื่อให้ใช้ Action ได้

public class BoxOpener : MonoBehaviour
{
    public Image boxImage;
    public Sprite boxOpenSprite;
    public Image rewardImage;
    // public Sprite[] rewardSprites; // บรรทัดนี้ไม่จำเป็นต้องใช้แล้ว เพราะ GameEventManager จะเป็นคนส่งรูปมาให้เอง
    public float revealDelay = 1f;

    public RectTransform boxTransform; 
    public float shakeDuration = 0.5f;
    public float shakeStrength = 10f;

    private bool isOpened = false;

    // แก้ไขฟังก์ชันให้รับพารามิเตอร์ 2 ตัว ตามที่ GameEventManager ส่งมา
    public void OpenBox(Sprite resultSprite, Action onComplete)
    {
        if (isOpened) return;
        isOpened = true;

        // ส่งรูปรางวัลและคำสั่งปิดหน้าต่างเข้าไปใน Coroutine
        StartCoroutine(ShakeAndOpen(resultSprite, onComplete));
    }

    IEnumerator ShakeAndOpen(Sprite resultSprite, Action onComplete)
    {
        // 1. สั่นกล่องก่อนเปิด
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

        // 2. เปลี่ยน Sprite เป็นกล่องเปิด
        boxImage.sprite = boxOpenSprite;

        // 3. รอแล้วโชว์รางวัลตามรูปที่ GameEventManager ส่งมา
        yield return new WaitForSeconds(revealDelay);
        
        if (resultSprite != null)
        {
            rewardImage.sprite = resultSprite;
        }
        rewardImage.gameObject.SetActive(true);

        // 4. รอโชว์ของ 3 วินาที แล้วสั่งให้ GameEventManager ปิดหน้าต่างและจบเทิร์น
        yield return new WaitForSeconds(3f);
        
        onComplete?.Invoke(); // เรียกใช้ Callback ที่ส่งมา
        
        isOpened = false; 
    }
}