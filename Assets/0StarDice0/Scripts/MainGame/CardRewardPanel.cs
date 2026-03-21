using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using System.Collections.Generic;

public class CardRewardPanel : MonoBehaviour
{
    [Header("Settings")]
    public float shuffleSpeed = 0.15f; 

    [Header("Data")]
    public List<DiceLockCardItem> rewardPool; 

    [Header("UI References")]
    public Image cardDisplayImage;      
    public TextMeshProUGUI cardNameText; 
    public Button keepButton;           
    public Button discardButton;        

    private DiceLockCardItem finalCard; 

    // ⭐ ส่วนที่เพิ่มเข้ามา: สั่งให้เชื่อมปุ่มอัตโนมัติเมื่อเริ่มเกม ⭐
    private void Start()
    {
        // ถ้ามีปุ่ม Keep ให้ใส่คำสั่งกดแล้วไปที่ OnKeepButtonClicked
        if (keepButton != null)
        {
            keepButton.onClick.RemoveAllListeners(); // ล้างคำสั่งเก่ากันเบิ้ล
            keepButton.onClick.AddListener(OnKeepButtonClicked);
        }

        // ถ้ามีปุ่ม Discard ให้ใส่คำสั่งกดแล้วไปที่ OnDiscardButtonClicked
        if (discardButton != null)
        {
            discardButton.onClick.RemoveAllListeners();
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }
    }

    private void OnEnable()
    {
        Debug.Log("🟢 RewardPanel ถูกเปิดแล้ว! เริ่มสุ่ม...");
        StartShuffle();
    }

    // ฟังก์ชันสั่งเปิด (เผื่อใช้จากที่อื่น)
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void StartShuffle()
    {
        StopAllCoroutines();

        if (rewardPool == null || rewardPool.Count == 0) return;
        if (cardDisplayImage == null) return;

        // ล็อกปุ่มระหว่างสุ่ม
        if (keepButton) keepButton.interactable = false;
        if (discardButton) discardButton.interactable = false;
        if (cardNameText != null) cardNameText.text = "Suffing...";

        StartCoroutine(ShuffleAnimation());
    }

    private IEnumerator ShuffleAnimation()
    {
        int loopCount = 10;
        for (int i = 0; i < loopCount; i++)
        {
            DiceLockCardItem randomCard = rewardPool[Random.Range(0, rewardPool.Count)];
            cardDisplayImage.sprite = randomCard.cardImage;
            yield return new WaitForSecondsRealtime(shuffleSpeed);
        }

        // --- ได้ผลลัพธ์ ---
        int finalIndex = Random.Range(0, rewardPool.Count);
        finalCard = rewardPool[finalIndex];
        
        cardDisplayImage.sprite = finalCard.cardImage;
        if (cardNameText != null) cardNameText.text = finalCard.cardName;

        // ปลดล็อกปุ่มให้กดได้
        if (keepButton) keepButton.interactable = true;
        if (discardButton) discardButton.interactable = true;
    }

    // --- ส่วนการทำงานของปุ่ม ---
    public void OnKeepButtonClicked()
    {
        Debug.Log("🖱️ กด Keep");
        PlayerCardInventory playerCardInventory = FindObjectOfType<PlayerCardInventory>();

        if (finalCard != null && playerCardInventory != null)
        {
            playerCardInventory.ObtainCard(finalCard);
        }
        else if (playerCardInventory == null)
        {
            Debug.LogWarning("⚠️ หา PlayerCardInventory ไม่เจอ (แต่จะปิด Panel ให้นะ)");
        }
        
        CloseThisPanel(); // สั่งปิด
    }

    public void OnDiscardButtonClicked()
    {
        Debug.Log("🖱️ กด Discard");
        CloseThisPanel(); // สั่งปิด
    }

    private void CloseThisPanel()
    {
        // ปิด GameObject นี้ (Panel จะหายไป)
        gameObject.SetActive(false);
    }
}
