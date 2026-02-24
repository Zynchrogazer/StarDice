using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoxOpener : MonoBehaviour
{
    public Image boxImage;
    public Sprite boxOpenSprite;
    public Image rewardImage;
    public Sprite[] rewardSprites;
    public float revealDelay = 1f;

    public RectTransform boxTransform; // กล่องที่จะสั่น
    public float shakeDuration = 0.5f;
    public float shakeStrength = 10f;

    private bool isOpened = false;
    //public GameEventManager gameEventManager;

    public void OpenBox()
    {
        if (isOpened) return;
        isOpened = true;

        StartCoroutine(ShakeAndOpen());
    }

    IEnumerator ShakeAndOpen()
    {
        // สั่นกล่องก่อนเปิด
        Vector3 originalPos = boxTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeStrength;
            float offsetY = Random.Range(-1f, 1f) * shakeStrength;

            boxTransform.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // คืนตำแหน่ง
        boxTransform.anchoredPosition = originalPos;

        // เปลี่ยน Sprite เป็นกล่องเปิด
        boxImage.sprite = boxOpenSprite;

        // รอแล้วโชว์รางวัล
        yield return new WaitForSeconds(revealDelay);

        int rand = Random.Range(0, rewardSprites.Length);
        rewardImage.sprite = rewardSprites[rand];
        rewardImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        
        
    }
}